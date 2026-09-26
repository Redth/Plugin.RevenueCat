#nullable enable

using System.Diagnostics;
using System.Text.Json;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

public enum PaywallEligibility
{
	Unknown,
	Eligible,
	Ineligible,
	NoIntroOfferExists
}

public readonly record struct PaywallOfferKey(string ProductIdentifier, string? SubscriptionOptionIdentifier = null);

/// <summary>Host-provided presentation facts. An offered trial is not evidence of customer eligibility.</summary>
public sealed class PaywallSemanticContext
{
	/// <summary>Customer-scoped: replace this context on login/logout and whenever the active customer changes.</summary>
	public IReadOnlyDictionary<PaywallOfferKey, PaywallEligibility> IntroOfferEligibility { get; init; } =
		new Dictionary<PaywallOfferKey, PaywallEligibility>();

	public IReadOnlyDictionary<PaywallOfferKey, PaywallEligibility> PromoOfferEligibility { get; init; } =
		new Dictionary<PaywallOfferKey, PaywallEligibility>();

	public IReadOnlyDictionary<string, JsonElement> CustomVariables { get; init; } =
		new Dictionary<string, JsonElement>();

	/// <summary>One of compact, medium, expanded; null means that no screen condition is known.</summary>
	public string? ScreenCondition { get; init; }

	/// <summary>Actual paywall bounds in device-independent units, if measured by the host.</summary>
	public double? Width { get; init; }

	public double? Height { get; init; }
}

internal sealed class PaywallSemanticSession
{
	readonly Dictionary<string, JsonElement> state = new(StringComparer.Ordinal);
	readonly HashSet<string> reported = new(StringComparer.Ordinal);
	readonly IReadOnlyDictionary<string, PaywallStateDeclaration> declarations;
	readonly Action<string>? diagnostic;
	readonly bool discardRules;
	IReadOnlyList<Package> packages;

	public PaywallSemanticSession(
		PaywallComponentsData? data,
		PaywallSemanticContext? context = null,
		Action<string>? diagnostic = null,
		PaywallComponentsConfig? config = null,
		IReadOnlyList<Package>? packages = null)
	{
		declarations = data?.StateDeclarations ?? new Dictionary<string, PaywallStateDeclaration>();
		this.packages = packages ?? [];
		Context = context ?? new PaywallSemanticContext();
		this.diagnostic = diagnostic;
		var root = (data?.ComponentsConfig ?? config)?.Base;
		discardRules = HasUnsupportedConditions(root?.Stack) ||
			HasUnsupportedConditions(root?.Header) ||
			HasUnsupportedConditions(root?.StickyFooter);
		if (discardRules)
		{
			Report("Unsupported condition or unmodeled override found: conditional paywall rules are disabled for this presentation.");
		}
		foreach (var (key, declaration) in declarations)
		{
			if (MatchesType(declaration.Default, declaration.Type))
			{
				state[key] = declaration.Default;
			}
			else
			{
				Report($"Invalid default for paywall state '{key}'.");
			}
		}
	}

	public PaywallSemanticContext Context { get; private set; }

	public string? SelectedPackageIdentifier { get; set; }

	public event Action? Changed;

	public void UpdateContext(PaywallSemanticContext? context)
	{
		Context = context ?? new PaywallSemanticContext();
		Changed?.Invoke();
	}

	public void SelectionChanged() => Changed?.Invoke();

	public void UpdatePackages(IReadOnlyList<Package> updatedPackages) => packages = updatedPackages;

	public PaywallEligibility IntroEligibility(Package? package) =>
		Context.IntroOfferEligibility.TryGetValue(OfferEligibilityKey(package?.Identifier) ?? default, out var eligibility)
			? eligibility : PaywallEligibility.Unknown;

	public bool IsVisible(PaywallComponent? component, string? packageId = null)
	{
		if (component is null)
		{
			return false;
		}
		var baseVisible = component switch
		{
			PaywallStackComponent stack => stack.Visible,
			PaywallTextComponent text => text.Visible,
			PaywallTabsComponent tabs => tabs.Visible,
			PaywallPackageComponent package => package.Visible,
			PaywallIconComponent icon => icon.Visible,
			PaywallImageComponent image => image.Visible,
			PaywallCarouselComponent carousel => carousel.Visible,
			PaywallTimelineComponent timeline => timeline.Visible,
			_ => null
		};
		return Resolve(component, packageId).Boolean("visible") ?? baseVisible ?? true;
	}

	public PaywallResolvedOverrides Resolve(PaywallComponent component, string? packageId = null)
	{
		var overrides = component switch
		{
			PaywallStackComponent stack => stack.Overrides,
			PaywallTextComponent text => text.Overrides,
			PaywallTabsComponent tabs => tabs.Overrides,
			_ => null
		};
		var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
		if (overrides is null)
		{
			return new(result, Report);
		}

		var skipRules = discardRules || overrides.Any(entry => entry.Conditions.Any(c => !SupportedCondition(c)));
		foreach (var unsupported in overrides.SelectMany(entry => entry.Conditions).Where(c => !SupportedCondition(c)))
		{
			InvalidCondition(unsupported);
		}
		foreach (var entry in overrides)
		{
			if (skipRules && entry.Conditions.Any(c => IsRule(c.Type) || !SupportedCondition(c)))
			{
				continue;
			}
			if (entry.Conditions.Count == 0 || !entry.Conditions.All(c => Matches(c, packageId)))
			{
				continue;
			}
			if (entry.Properties is not { ValueKind: JsonValueKind.Object } properties)
			{
				Report($"Invalid override properties on '{component.Id ?? component.Type}'.");
				continue;
			}
			foreach (var property in properties.EnumerateObject())
			{
				var key = SnakeCase(property.Name);
				if (!Allowed(component, key))
				{
					Report($"Unsupported override property '{property.Name}' on '{component.Id ?? component.Type}'.");
					continue;
				}
				if (property.Value.ValueKind == JsonValueKind.Null)
				{
					continue;
				}
				result[key] = property.Value;
			}
		}
		return new(result, Report);
	}

	public bool Apply(IReadOnlyList<PaywallStateUpdate> updates, JsonElement? payload)
	{
		var changed = false;
		foreach (var update in updates)
		{
			if (string.IsNullOrWhiteSpace(update.Set) || update.To is not { } value ||
				update.ExtensionData is { Count: > 0 })
			{
				Report("Unsupported paywall state update.");
				continue;
			}
			if (value.ValueKind == JsonValueKind.String && value.GetString() == "$value")
			{
				if (payload is null)
				{
					Report($"State update '{update.Set}' needs an interaction value.");
					continue;
				}
				value = payload.Value;
			}
			if (!declarations.TryGetValue(update.Set, out var declaration) ||
				!MatchesType(value, declaration.Type))
			{
				Report($"State update '{update.Set}' is undeclared or has an incompatible value.");
				continue;
			}
			if (!state.TryGetValue(update.Set, out var previous) || !Equal(previous, value))
			{
				state[update.Set] = value.Clone();
				changed = true;
			}
		}
		if (changed)
		{
			Changed?.Invoke();
		}
		return changed;
	}

	bool Matches(PaywallComponentOverrideCondition condition, string? packageId)
	{
		var op = condition.Operator;
		var value = condition.Value;
		switch (condition.Type)
		{
			case "selected":
				return packageId is not null && packageId == SelectedPackageIdentifier;
			case "compact":
			case "medium":
			case "expanded":
				return ScreenIncludes(Context.ScreenCondition, condition.Type);
			case "intro_offer":
			case "promo_offer":
			case "intro_offer_condition":
			case "promo_offer_condition":
				var eligibilityKey = OfferEligibilityKey(packageId ?? SelectedPackageIdentifier);
				var eligibility = (condition.Type.StartsWith("intro", StringComparison.Ordinal)
					? Context.IntroOfferEligibility : Context.PromoOfferEligibility)
					.TryGetValue(eligibilityKey ?? default, out var known)
						? known : PaywallEligibility.Unknown;
				if (eligibility == PaywallEligibility.Unknown)
				{
					return false;
				}
				var eligible = eligibility == PaywallEligibility.Eligible;
				return condition.Type.EndsWith("_condition", StringComparison.Ordinal)
					? value is { ValueKind: JsonValueKind.True or JsonValueKind.False } &&
					  (op == "=" ? eligible == value.Value.GetBoolean() :
					  op == "!=" && eligible != value.Value.GetBoolean())
					: eligible;
			case "selected_package_condition":
				var selected = SelectedPackageIdentifier;
				return selected is not null && (op switch
				{
					"in" => condition.Packages.Contains(selected, StringComparer.Ordinal),
					"not in" => !condition.Packages.Contains(selected, StringComparer.Ordinal),
					_ => InvalidCondition(condition)
				});
			case "variable_condition":
				if (string.IsNullOrWhiteSpace(condition.Variable) || value is null)
				{
					return InvalidCondition(condition);
				}
				if (!Context.CustomVariables.TryGetValue(condition.Variable, out var variable))
				{
					return op == "!=";
				}
				return Compare(variable, value.Value, op);
			case "state_condition":
				if (string.IsNullOrWhiteSpace(condition.Name) || value is null ||
					!state.TryGetValue(condition.Name, out var current))
				{
					return InvalidCondition(condition);
				}
				return Compare(current, value.Value, op);
			case "window_width_condition":
			case "window_height_condition":
			case "window_aspect_ratio_condition":
				var dimension = condition.Type switch
				{
					"window_width_condition" => Context.Width,
					"window_height_condition" => Context.Height,
					_ => Context.Height > 0 ? Context.Width / Context.Height : null
				};
				if (dimension is null || value is not { ValueKind: JsonValueKind.Number } ||
					!value.Value.TryGetDouble(out var threshold))
				{
					return false;
				}
				return op switch
				{
					">=" => dimension >= threshold,
					">" => dimension > threshold,
					"<=" => dimension <= threshold,
					"<" => dimension < threshold,
					"=" => Math.Abs(dimension.Value - threshold) < 1e-10,
					_ => InvalidCondition(condition)
				};
			default:
				return InvalidCondition(condition);
		}
	}

	PaywallOfferKey? OfferEligibilityKey(string? packageIdentifier)
	{
		if (string.IsNullOrWhiteSpace(packageIdentifier))
		{
			return null;
		}
		var product = packages.FirstOrDefault(p => p.Identifier == packageIdentifier)?.StoreProduct;
		if (string.IsNullOrWhiteSpace(product?.Identifier))
		{
			return null;
		}
		var optionId = product.DefaultSubscriptionOption?.Id;
		return new PaywallOfferKey(product.Identifier, string.IsNullOrWhiteSpace(optionId) ? null : optionId);
	}

	bool InvalidCondition(PaywallComponentOverrideCondition condition)
	{
		Report($"Unsupported or unresolved paywall condition '{condition.Type ?? "null"}'.");
		return false;
	}

	static bool IsRule(string? type) => type is "intro_offer_condition" or "promo_offer_condition" or
		"variable_condition" or "selected_package_condition" or "state_condition" or
		"window_width_condition" or "window_height_condition" or "window_aspect_ratio_condition";

	static bool SupportedCondition(PaywallComponentOverrideCondition condition) => condition.Type switch
	{
		"selected" or "compact" or "medium" or "expanded" or "intro_offer" or "promo_offer" => true,
		"intro_offer_condition" or "promo_offer_condition" =>
			condition.Operator is "=" or "!=" && condition.Value is { ValueKind: JsonValueKind.True or JsonValueKind.False },
		"variable_condition" =>
			condition.Operator is "=" or "!=" && !string.IsNullOrWhiteSpace(condition.Variable) &&
			IsScalar(condition.Value),
		"state_condition" =>
			condition.Operator is "=" or "!=" && !string.IsNullOrWhiteSpace(condition.Name) &&
			IsScalar(condition.Value),
		"selected_package_condition" => condition.Operator is "in" or "not in",
		"window_width_condition" or "window_height_condition" or "window_aspect_ratio_condition" =>
			condition.Operator is ">=" or ">" or "<=" or "<" or "=" &&
			condition.Value is { ValueKind: JsonValueKind.Number },
		_ => false
	};

	static bool IsScalar(JsonElement? value) =>
		value is { ValueKind: JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False };

	static bool HasUnsupportedConditions(PaywallComponent? component)
	{
		if (component is null)
		{
			return false;
		}
		var overrides = component switch
		{
			PaywallStackComponent stack => stack.Overrides,
			PaywallTextComponent text => text.Overrides,
			PaywallTabsComponent tabs => tabs.Overrides,
			_ => null
		};
		if (overrides?.Any(entry => entry.Conditions.Any(c => !SupportedCondition(c))) == true)
		{
			return true;
		}
		if (component.ExtensionData?.ContainsKey("overrides") == true)
		{
			return true;
		}
		return HasUnsupportedConditions(component.Fallback) || (component switch
		{
			PaywallStackComponent stack => stack.Components.Any(HasUnsupportedConditions) ||
				HasUnsupportedConditions(EmbeddedStack(stack.Badge)),
			PaywallPackageComponent package => HasUnsupportedConditions(package.Stack),
			PaywallButtonComponent button => HasUnsupportedConditions(button.Stack),
			PaywallPurchaseButtonComponent purchase => HasUnsupportedConditions(purchase.Stack),
			PaywallHeaderComponent header => HasUnsupportedConditions(header.Stack),
			PaywallStickyFooterComponent footer => HasUnsupportedConditions(footer.Stack),
			PaywallTabsComponent tabs => tabs.Tabs.Any(t => HasUnsupportedConditions(t.Stack)) ||
				HasUnsupportedConditions(EmbeddedStack(tabs.Control)),
			PaywallCarouselComponent carousel => carousel.Pages.Any(HasUnsupportedConditions),
			PaywallCountdownComponent countdown =>
				HasUnsupportedConditions(countdown.CountdownStack) || HasUnsupportedConditions(countdown.EndStack),
			_ => false
		});
	}

	static PaywallStackComponent? EmbeddedStack(JsonElement? data)
	{
		if (data is not { ValueKind: JsonValueKind.Object } parent ||
			!parent.TryGetProperty("stack", out var stack) ||
			stack.ValueKind != JsonValueKind.Object)
		{
			return null;
		}
		return stack.Deserialize(ModelSerializerContext.Default.PaywallStackComponent);
	}

	void Report(string message)
	{
		if (reported.Add(message))
		{
			Trace.TraceWarning("{0}", message);
			diagnostic?.Invoke(message);
		}
	}

	static bool Allowed(PaywallComponent component, string key) =>
		component switch
		{
			PaywallTextComponent => key is "visible" or "text_lid" or "color" or "font_size" or
				"font_weight" or "font_weight_int" or "horizontal_alignment" or "background_color" or
				"padding" or "margin" or "size",
			PaywallStackComponent or PaywallTabsComponent => key is "visible" or "background" or
				"background_color" or "padding" or "margin" or "size",
			_ => false
		};

	static string SnakeCase(string key) =>
		string.Concat(key.SelectMany((c, index) =>
			char.IsUpper(c) ? (index == 0 ? "" : "_") + char.ToLowerInvariant(c) : c.ToString()));

	static bool ScreenIncludes(string? actual, string? expected) =>
		(actual, expected) switch
		{
			("expanded", "expanded" or "medium" or "compact") => true,
			("medium", "medium" or "compact") => true,
			("compact", "compact") => true,
			_ => false
		};

	static bool MatchesType(JsonElement value, string? type) => type switch
	{
		"boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
		"string" => value.ValueKind == JsonValueKind.String,
		"integer" => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
		"double" => value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out _),
		_ => false
	};

	static bool Equal(JsonElement left, JsonElement right) =>
		left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number
			? left.TryGetDouble(out var a) && right.TryGetDouble(out var b) && Math.Abs(a - b) < 1e-10
			: left.ValueKind == right.ValueKind && left.ToString() == right.ToString();

	static bool Compare(JsonElement actual, JsonElement expected, string? op)
	{
		if (op is not ("=" or "!=") ||
			actual.ValueKind != expected.ValueKind &&
			!(IsBoolean(actual) && IsBoolean(expected)) &&
			!(actual.ValueKind == JsonValueKind.Number && expected.ValueKind == JsonValueKind.Number))
		{
			return false;
		}
		var matches = Equal(actual, expected);
		return op == "=" ? matches : !matches;
	}

	static bool IsBoolean(JsonElement value) =>
		value.ValueKind is JsonValueKind.True or JsonValueKind.False;
}

internal sealed class PaywallResolvedOverrides(
	IReadOnlyDictionary<string, JsonElement> properties,
	Action<string> report)
{
	public JsonElement? Element(string key) => properties.TryGetValue(key, out var value) ? value : null;

	public bool? Boolean(string key)
	{
		if (Element(key) is not { } value)
		{
			return null;
		}
		if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
		{
			return value.GetBoolean();
		}
		report($"Invalid boolean override '{key}'.");
		return null;
	}

	public string? String(string key)
	{
		if (Element(key) is not { } value)
		{
			return null;
		}
		if (value.ValueKind == JsonValueKind.String)
		{
			return value.GetString();
		}
		report($"Invalid string override '{key}'.");
		return null;
	}

	public double? Number(string key)
	{
		if (Element(key) is not { } value)
		{
			return null;
		}
		if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
		{
			return number;
		}
		report($"Invalid numeric override '{key}'.");
		return null;
	}
}
