using System.Text.Json;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls;

namespace Tests;

[TestClass]
[DoNotParallelize]
public sealed class PaywallSemanticsTests
{
	[TestInitialize]
	public void Initialize() => DispatcherProvider.SetCurrent(new FakeDispatcherProvider());

	[TestCleanup]
	public void Cleanup() => DispatcherProvider.SetCurrent(null);

	[TestMethod]
	public void Overrides_Match_All_Conditions_In_Order_And_Reject_Unsupported_Conditions()
	{
		var text = new PaywallTextComponent
		{
			Id = "offer",
			FontSize = JsonSerializer.SerializeToElement(10),
			Overrides =
			[
				Override(new { font_size = 14 }, Condition("selected_package_condition", "in", packages: ["annual"])),
				Override(new { font_size = 21 },
					Condition("variable_condition", "=", value: "pro", variable: "segment"),
					Condition("intro_offer_condition", "=", value: true))
			]
		};
		var diagnostics = new List<string>();
		var session = new PaywallSemanticSession(null, new()
		{
			IntroOfferEligibility = new Dictionary<string, PaywallEligibility> { ["annual"] = PaywallEligibility.Eligible },
			CustomVariables = new Dictionary<string, JsonElement> { ["segment"] = JsonSerializer.SerializeToElement("pro") }
		}, diagnostics.Add)
		{
			SelectedPackageIdentifier = "annual"
		};

		Assert.AreEqual(21, session.Resolve(text).Number("font_size"));
		text.Overrides.Add(Override(new { font_size = 90 }, Condition("unknown_future_rule")));
		Assert.IsNull(session.Resolve(text).Number("font_size"));
		session.UpdateContext(new()
		{
			CustomVariables = new Dictionary<string, JsonElement> { ["segment"] = JsonSerializer.SerializeToElement("pro") }
		});
		Assert.IsNull(session.Resolve(text).Number("font_size"));
		Assert.IsTrue(diagnostics.Any(message => message.Contains("unknown_future_rule")));
	}

	[TestMethod]
	public void Unknown_Eligibility_Never_Matches_Negated_Or_Positive_Trial_Rules()
	{
		var session = new PaywallSemanticSession(null);
		var positive = new PaywallTextComponent { Overrides = [Override(new { visible = true }, Condition("intro_offer"))] };
		var negative = new PaywallTextComponent { Overrides = [Override(new { visible = true }, Condition("intro_offer_condition", "!=", value: true))] };
		Assert.IsNull(session.Resolve(positive).Boolean("visible"));
		Assert.IsNull(session.Resolve(negative).Boolean("visible"));
		session.UpdateContext(new()
		{
			IntroOfferEligibility = new Dictionary<string, PaywallEligibility> { ["annual"] = PaywallEligibility.Ineligible }
		});
		session.SelectedPackageIdentifier = "annual";
		Assert.IsTrue(session.Resolve(negative).Boolean("visible"));
	}

	[TestMethod]
	public void State_Updates_Require_Declared_Keys_Typed_Values_And_Payload()
	{
		var data = new PaywallComponentsData
		{
			StateDeclarations = new()
			{
				["plan"] = new() { Type = "string", Default = JsonSerializer.SerializeToElement("annual") },
				["enabled"] = new() { Type = "boolean", Default = JsonSerializer.SerializeToElement(false) }
			}
		};
		var diagnostics = new List<string>();
		var session = new PaywallSemanticSession(data, diagnostic: diagnostics.Add);
		var rule = new PaywallTextComponent
		{
			Overrides = [Override(new { visible = true }, new PaywallComponentOverrideCondition
			{
				Type = "state_condition", Name = "plan", Operator = "=", Value = JsonSerializer.SerializeToElement("monthly")
			})]
		};
		Assert.IsNull(session.Resolve(rule).Boolean("visible"));
		Assert.IsFalse(session.Apply([Update("unknown", "monthly"), Update("enabled", "monthly")], null));
		Assert.IsFalse(session.Apply([Update("plan", "$value")], null));
		Assert.IsTrue(session.Apply([Update("plan", "$value")], JsonSerializer.SerializeToElement("monthly")));
		Assert.IsTrue(session.Resolve(rule).Boolean("visible"));
		Assert.IsTrue(diagnostics.Count >= 3);
	}

	[TestMethod]
	public void State_And_Overrides_Serialize_Using_Source_Generated_Contracts()
	{
		// Derived, sanitized from purchases-ios 5.91.0 ComponentOverrides.swift and StateUpdate.swift (MIT).
		var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "paywall_semantics.json"));
		var data = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallComponentsData)!;
		Assert.AreEqual("annual", data.StateDeclarations["plan"].Default.GetString());
		var tabs = (PaywallTabsComponent)data.ComponentsConfig!.Base!.Stack!.Components[0];
		Assert.AreEqual("$value", tabs.StateUpdates[0].To!.Value.GetString());
		Assert.AreEqual("plan", tabs.Overrides[0].Conditions[0].Name);
		var text = (PaywallTextComponent)data.ComponentsConfig.Base.Stack.Components[1];
		Assert.AreEqual("intro_offer", new PaywallSemanticSession(data, new()
		{
			IntroOfferEligibility = new Dictionary<string, PaywallEligibility> { ["annual"] = PaywallEligibility.Eligible }
		}) { SelectedPackageIdentifier = "annual" }.Resolve(text).String("text_lid"));
		var roundTrip = JsonSerializer.Serialize(data, ModelSerializerContext.Default.PaywallComponentsData);
		Assert.IsTrue(roundTrip.Contains("\"state_declarations\"", StringComparison.Ordinal));
		Assert.IsTrue(roundTrip.Contains("\"state_updates\"", StringComparison.Ordinal));
	}

	[TestMethod]
	public void Eligibility_Change_Reconciles_Visible_Package_And_Trial_Text_In_Place()
	{
		var annualStack = new PaywallStackComponent
		{
			Visible = false,
			Overrides = [Override(new { visible = true }, Condition("intro_offer_condition", "=", value: true))],
			Components = [Text("annual-price", "{{ price }}"), Text("trial", "Try {{ sub_offer_duration }} for {{ sub_offer_price }}")]
		};
		var view = CreateView(new()
		{
			Components =
			[
				Package("monthly"),
				new PaywallPackageComponent { Id = "annual", PackageId = "annual", IsSelectedByDefault = true, Stack = annualStack },
				Text("selected-price", "{{ price }}"),
				new PaywallPurchaseButtonComponent { Id = "purchase" }
			]
		});
		var root = view.Content;
		var label = Find<Label>(view, "selected-price");
		var annualCard = Find<View>(view, "annual");
		Assert.IsFalse(annualCard.IsVisible);
		Assert.AreEqual("$6.99", label.Text);
		Assert.AreEqual("Try  for ", Find<Label>(view, "trial").Text);

		view.SemanticContext = new()
		{
			IntroOfferEligibility = new Dictionary<string, PaywallEligibility> { ["annual"] = PaywallEligibility.Eligible }
		};
		Assert.AreSame(root, view.Content);
		Assert.AreSame(label, Find<Label>(view, "selected-price"));
		Assert.IsTrue(annualCard.IsVisible);
		Tap(annualCard);
		Assert.AreEqual("$49.99", label.Text);
		Assert.AreEqual("Try week for $0.00", Find<Label>(view, "trial").Text);
		string? purchased = null;
		view.PurchaseRequested += (_, args) => purchased = args.Request.PackageIdentifier;
		Tap(Find<View>(view, "purchase"));
		Assert.AreEqual("annual", purchased);

		view.SemanticContext = null;
		Assert.AreSame(root, view.Content);
		Assert.IsFalse(annualCard.IsVisible);
		Assert.IsFalse(annualCard.IsEnabled);
		Assert.AreEqual("$6.99", label.Text);
		Tap(Find<View>(view, "purchase"));
		Assert.AreEqual("monthly", purchased);
	}

	[TestMethod]
	public void Tab_State_Update_Changes_Only_Affected_Text_And_Purchase_Target()
	{
		var tabs = new PaywallTabsComponent
		{
			Id = "plans",
			DefaultTabId = "annual",
			StateUpdates = [Update("plan", "$value")],
			Tabs =
			[
				new() { Id = "annual", Name = "Annual", Stack = new() { Components = [Package("annual")] } },
				new() { Id = "monthly", Name = "Monthly", Stack = new() { Components = [Package("monthly")] } }
			]
		};
		var conditional = Text("state", "Monthly active");
		conditional.Visible = false;
		conditional.Overrides =
		[
			Override(new { visible = true }, new PaywallComponentOverrideCondition
			{
				Type = "state_condition", Name = "plan", Operator = "=", Value = JsonSerializer.SerializeToElement("monthly")
			})
		];
		var view = CreateView(new() { Components = [tabs, conditional, Text("price", "{{ price }}")] });
		view.PaywallData!.StateDeclarations["plan"] = new()
		{
			Type = "string", Default = JsonSerializer.SerializeToElement("annual")
		};
		view.Render();
		var root = view.Content;
		var stateLabel = Find<Label>(view, "state");
		Assert.IsFalse(stateLabel.IsVisible);
		var monthlyButton = Descendants(view).OfType<Border>()
			.Single(b => b.Content is Label { Text: "Monthly" });
		Tap(monthlyButton);

		Assert.AreSame(root, view.Content);
		Assert.AreSame(stateLabel, Find<Label>(view, "state"));
		Assert.IsTrue(stateLabel.IsVisible);
		Assert.AreEqual("$6.99", Find<Label>(view, "price").Text);
	}

	static PaywallComponentOverride Override(object properties, params PaywallComponentOverrideCondition[] conditions) =>
		new() { Properties = JsonSerializer.SerializeToElement(properties), Conditions = [.. conditions] };

	static PaywallComponentOverrideCondition Condition(
		string type, string? op = null, object? value = null, string? variable = null, List<string>? packages = null) =>
		new()
		{
			Type = type, Operator = op, Variable = variable,
			Value = value is null ? null : JsonSerializer.SerializeToElement(value),
			Packages = packages ?? []
		};

	static PaywallStateUpdate Update(string key, string to) =>
		new() { Set = key, To = JsonSerializer.SerializeToElement(to) };

	static PaywallTextComponent Text(string id, string text) => new() { Id = id, TextLocalizationId = text };

	static PaywallPackageComponent Package(string id) =>
		new() { Id = id, PackageId = id, Stack = new() { Components = [Text(id + "-price", "{{ price }}")] } };

	static RevenueCatPaywallView CreateView(PaywallStackComponent stack) => new()
	{
		Packages =
		[
			new() { Identifier = "monthly", StoreProduct = new() { PriceString = "$6.99" } },
			new()
			{
				Identifier = "annual",
				StoreProduct = new()
				{
					PriceString = "$49.99",
					IntroductoryDiscount = new()
					{
						PriceString = "$0.00",
						SubscriptionPeriod = new() { Unit = SubscriptionPeriodUnit.Week, Value = 1 }
					}
				}
			}
		],
		PaywallData = new() { ComponentsConfig = new() { Base = new() { Stack = stack } } }
	};

	static T Find<T>(View root, string id) where T : View =>
		Descendants(root).OfType<T>().Single(v => v.AutomationId == id);

	static void Tap(View view) => view.GestureRecognizers.OfType<TapGestureRecognizer>().Single().Command!.Execute(null);

	static IEnumerable<View> Descendants(View root)
	{
		yield return root;
		var children = root switch
		{
			Layout layout => layout.Children.OfType<View>(),
			ContentView { Content: View content } => [content],
			Border { Content: View content } => [content],
			ScrollView { Content: View content } => [content],
			_ => Enumerable.Empty<View>()
		};
		foreach (var child in children.SelectMany(Descendants))
		{
			yield return child;
		}
	}

	sealed class FakeDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetForCurrentThread() => new FakeDispatcher();
	}

	sealed class FakeDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;
		public bool Dispatch(Action action) { action(); return true; }
		public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException();
		public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
	}
}
