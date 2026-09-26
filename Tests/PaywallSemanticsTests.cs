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
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility> { [new("product.annual")] = PaywallEligibility.Eligible },
			CustomVariables = new Dictionary<string, JsonElement> { ["segment"] = JsonSerializer.SerializeToElement("pro") }
		}, diagnostics.Add, packages: [AnnualPackage()])
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
		var session = new PaywallSemanticSession(null, packages: [AnnualPackage()]);
		var positive = new PaywallTextComponent { Overrides = [Override(new { visible = true }, Condition("intro_offer"))] };
		var negative = new PaywallTextComponent { Overrides = [Override(new { visible = true }, Condition("intro_offer_condition", "!=", value: true))] };
		Assert.IsNull(session.Resolve(positive).Boolean("visible"));
		Assert.IsNull(session.Resolve(negative).Boolean("visible"));
		session.UpdateContext(new()
		{
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility> { [new("product.annual")] = PaywallEligibility.Ineligible }
		});
		session.SelectedPackageIdentifier = "annual";
		Assert.IsTrue(session.Resolve(negative).Boolean("visible"));
		session.UpdateContext(new()
		{
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility> { [new("product.annual")] = PaywallEligibility.NoIntroOfferExists }
		});
		Assert.IsTrue(session.Resolve(negative).Boolean("visible"));
	}

	[TestMethod]
	public void Unsupported_Web_View_Without_Fallback_Reports_Diagnostic_Without_Purchase()
	{
		var view = CreateView(new()
		{
			Components =
			[
				new PaywallUnknownComponent { Type = "web_view", Id = "unsupported-web-view" },
				new PaywallPurchaseButtonComponent
				{
					Id = "web-purchase", Method = JsonSerializer.SerializeToElement(new { type = "custom_web_checkout" })
				}
			]
		});
		var messages = new List<string>();
		view.SemanticDiagnostic += (_, message) => messages.Add(message);
		view.Render();
		Assert.IsTrue(messages.Any(message => message.Contains("web_view", StringComparison.Ordinal)));
		Assert.IsFalse(Find<View>(view, "unsupported-web-view").IsVisible);
		var purchases = 0;
		Exception? failed = null;
		view.PurchaseRequested += (_, _) => purchases++;
		view.ActionFailed += (_, args) => failed = args.Exception;
		Tap(Find<View>(view, "web-purchase"));
		Assert.AreEqual(0, purchases);
		Assert.IsInstanceOfType<NotSupportedException>(failed);
	}

	[TestMethod]
	public void Boolean_Conditions_Compare_Both_True_And_False_Including_Inequality()
	{
		var data = new PaywallComponentsData
		{
			StateDeclarations = new()
			{
				["enabled"] = new() { Type = "boolean", Default = JsonSerializer.SerializeToElement(true) }
			}
		};
		var session = new PaywallSemanticSession(data, new()
		{
			CustomVariables = new Dictionary<string, JsonElement>
			{
				["is_member"] = JsonSerializer.SerializeToElement(false)
			}
		});
		var enabled = new PaywallTextComponent
		{
			Overrides =
			[
				Override(new { visible = true }, new PaywallComponentOverrideCondition
				{
					Type = "state_condition", Name = "enabled", Operator = "!=",
					Value = JsonSerializer.SerializeToElement(false)
				})
			]
		};
		var notMember = new PaywallTextComponent
		{
			Overrides = [Override(new { visible = true },
				Condition("variable_condition", "!=", value: true, variable: "is_member"))]
		};
		Assert.IsTrue(session.Resolve(enabled).Boolean("visible"));
		Assert.IsTrue(session.Resolve(notMember).Boolean("visible"));
		Assert.IsTrue(session.Apply([new PaywallStateUpdate
		{
			Set = "enabled", To = JsonSerializer.SerializeToElement(false)
		}], null));
		Assert.IsNull(session.Resolve(enabled).Boolean("visible"));
		session.UpdateContext(new()
		{
			CustomVariables = new Dictionary<string, JsonElement>
			{
				["is_member"] = JsonSerializer.SerializeToElement(true)
			}
		});
		Assert.IsNull(session.Resolve(notMember).Boolean("visible"));
	}

	[TestMethod]
	public void Unsupported_Condition_In_Tab_Control_Disables_Rules_Throughout_Paywall()
	{
		var text = Text("rule", "base");
		text.Overrides = [Override(new { text_lid = "new" },
			Condition("variable_condition", "=", value: true, variable: "enabled"))];
		var control = JsonSerializer.SerializeToElement(new
		{
			stack = new
			{
				type = "stack",
				components = new object[]
				{
					new
					{
						type = "text",
						overrides = new object[]
						{
							new
							{
								conditions = new[] { new { type = "future_rule" } },
								properties = new { visible = true }
							}
						}
					}
				}
			}
		});
		var data = new PaywallComponentsData
		{
			ComponentsConfig = new()
			{
				Base = new() { Stack = new()
				{
					Components = [text, new PaywallTabsComponent { Control = control }]
				} }
			}
		};
		var diagnostics = new List<string>();
		var session = new PaywallSemanticSession(data, new()
		{
			CustomVariables = new Dictionary<string, JsonElement>
			{
				["enabled"] = JsonSerializer.SerializeToElement(true)
			}
		}, diagnostics.Add);
		Assert.IsNull(session.Resolve(text).String("text_lid"));
		Assert.IsTrue(diagnostics.Any(message => message.Contains("disabled")));
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
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility> { [new("product.annual")] = PaywallEligibility.Eligible }
		}, packages: [AnnualPackage()]) { SelectedPackageIdentifier = "annual" }.Resolve(text).String("text_lid"));
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
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility> { [new("product.annual")] = PaywallEligibility.Eligible }
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
	public void Conditional_Size_Restores_Base_Dimensions_And_Layout_In_Place()
	{
		var size = JsonSerializer.SerializeToElement(new
		{
			width = new { type = "fixed", value = 155 },
			height = new { type = "fixed", value = 45 }
		});
		var stack = new PaywallStackComponent
		{
			Id = "sized",
			Overrides = [Override(new { size }, Condition("variable_condition", "=", value: true, variable: "wide"))],
			Components = [Text("sized-text", "Test")]
		};
		var text = (PaywallTextComponent)stack.Components[0];
		text.Overrides = [Override(new { size }, Condition("variable_condition", "=", value: true, variable: "wide"))];
		var view = CreateView(new() { Components = [stack] });
		var container = Find<View>(view, "sized");
		var label = Find<Label>(view, "sized-text");
		var baseWidth = container.WidthRequest;
		var baseHeight = container.HeightRequest;
		var baseHorizontal = container.HorizontalOptions;
		var baseVertical = container.VerticalOptions;
		view.SemanticContext = new()
		{
			CustomVariables = new Dictionary<string, JsonElement> { ["wide"] = JsonSerializer.SerializeToElement(true) }
		};
		Assert.AreEqual(155, container.WidthRequest);
		Assert.AreEqual(45, container.HeightRequest);
		Assert.AreEqual(155, label.WidthRequest);
		view.SemanticContext = null;
		Assert.AreEqual(baseWidth, container.WidthRequest);
		Assert.AreEqual(baseHeight, container.HeightRequest);
		Assert.AreEqual(baseHorizontal, container.HorizontalOptions);
		Assert.AreEqual(baseVertical, container.VerticalOptions);
		Assert.AreEqual(-1, label.WidthRequest);
		Assert.AreSame(label, Find<Label>(view, "sized-text"));
	}

	[TestMethod]
	public void Badged_Stack_Does_Not_Duplicate_Container_Padding_Or_Background()
	{
		var stack = new PaywallStackComponent
		{
			Id = "badged",
			Padding = JsonSerializer.SerializeToElement(new { leading = 12, top = 12, trailing = 12, bottom = 12 }),
			Margin = JsonSerializer.SerializeToElement(new { leading = 4, top = 4, trailing = 4, bottom = 4 }),
			Shape = JsonSerializer.SerializeToElement(new { type = "pill" }),
			Badge = JsonSerializer.SerializeToElement(new
			{
				stack = new { type = "stack", components = new object[] { new { type = "text", text_lid = "New" } } }
			})
		};
		var view = CreateView(new() { Components = [stack] });
		var grid = Find<Grid>(view, "badged");
		var border = (Border)grid.Children.First();
		Assert.AreEqual(new Thickness(0), grid.Padding);
		Assert.AreEqual(new Thickness(12), border.Padding);
		Assert.AreEqual(new Thickness(4), grid.Margin);
		Assert.AreEqual(new Thickness(0), border.Margin);
	}

	[TestMethod]
	public void Intro_Duration_Uses_All_Billing_Cycles_And_Unknown_Does_Not_Promise_Trial()
	{
		var package = new Package
		{
			Identifier = "annual", StoreProduct = new()
			{
				PriceString = "$49.99", DefaultSubscriptionOption = new()
				{
					IntroPhase = new()
					{
						BillingPeriod = new() { Unit = SubscriptionPeriodUnit.Month, Value = 1 },
						BillingCycleCount = 3,
						Price = new() { Formatted = "$1.99" }
					}
				}
			}
		};
		var variables = new DefaultPaywallVariableProvider();
		var unknown = new PaywallVariableContext { Package = package };
		var eligible = new PaywallVariableContext { Package = package, IntroOfferEligibility = PaywallEligibility.Eligible };
		Assert.IsNull(variables.Resolve("sub_offer_duration", unknown));
		Assert.IsNull(variables.Resolve("sub_offer_price", unknown));
		Assert.AreEqual("$49.99", variables.Resolve("price", unknown));
		Assert.AreEqual("3 months", variables.Resolve("sub_offer_duration", eligible));
		Assert.AreEqual("$1.99", variables.Resolve("sub_offer_price", eligible));
	}

	[TestMethod]
	public void Unknown_Eligibility_Does_Not_Block_An_Ordinary_Full_Price_Purchase()
	{
		var view = CreateView(new()
		{
			Components = [Package("annual"), Text("regular-price", "{{ price }}"),
				Text("trial-copy", "{{ sub_offer_duration }}"),
				new PaywallPurchaseButtonComponent { Id = "purchase" }]
		});
		string? purchased = null;
		view.PurchaseRequested += (_, args) => purchased = args.Request.PackageIdentifier;
		Assert.AreEqual("$49.99", Find<Label>(view, "regular-price").Text);
		Assert.AreEqual(string.Empty, Find<Label>(view, "trial-copy").Text);
		Tap(Find<View>(view, "purchase"));
		Assert.AreEqual("annual", purchased);
	}

	[TestMethod]
	public void Changing_Catalog_Updates_Price_Availability_And_Purchase_Without_Replacing_Root()
	{
		var view = CreateView(new()
		{
			Components = [Package("monthly"), Package("annual"), Text("price", "{{ price }}"),
				new PaywallPurchaseButtonComponent { Id = "purchase" }]
		});
		var root = view.Content;
		var price = Find<Label>(view, "price");
		var annual = Find<View>(view, "annual");
		Tap(annual);
		Assert.AreEqual("$49.99", price.Text);

		view.Packages = [new() { Identifier = "monthly", StoreProduct = new() { PriceString = "$8.99" } }];
		Assert.AreSame(root, view.Content);
		Assert.AreSame(price, Find<Label>(view, "price"));
		Assert.AreEqual("$8.99", price.Text);
		Assert.IsFalse(annual.IsEnabled);
		string? purchased = null;
		view.PurchaseRequested += (_, args) => purchased = args.Request.PackageIdentifier;
		Tap(Find<View>(view, "purchase"));
		Assert.AreEqual("monthly", purchased);

		view.Packages =
		[
			new() { Identifier = "monthly", StoreProduct = new() { PriceString = "$9.99" } },
			new() { Identifier = "annual", StoreProduct = new() { PriceString = "$59.99" } }
		];
		Assert.AreSame(root, view.Content);
		Assert.AreEqual("$9.99", price.Text);
		Assert.IsTrue(annual.IsEnabled);
		Tap(annual);
		Assert.AreEqual("$59.99", price.Text);
	}

	[TestMethod]
	public void Replaced_Product_Cannot_Reuse_Stale_Eligibility_For_The_Same_Package_Id()
	{
		var package = new PaywallPackageComponent
		{
			Id = "annual", PackageId = "annual", Stack = new()
			{
				Visible = false,
				Overrides = [Override(new { visible = true },
					Condition("intro_offer_condition", "=", value: true))]
			}
		};
		var view = CreateView(new() { Components = [package, Package("monthly")] });
		view.SemanticContext = new()
		{
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility>
			{
				[new("product.annual")] = PaywallEligibility.Eligible
			}
		};
		var root = view.Content;
		Assert.IsTrue(Find<View>(view, "annual").IsVisible);
		view.Packages =
		[
			new() { Identifier = "monthly", StoreProduct = new() { Identifier = "product.monthly", PriceString = "$6.99" } },
			new() { Identifier = "annual", StoreProduct = new() { Identifier = "replacement.annual", PriceString = "$49.99" } }
		];
		Assert.AreSame(root, view.Content);
		Assert.IsFalse(Find<View>(view, "annual").IsVisible);
	}

	[TestMethod]
	public void Replaced_Subscription_Option_Cannot_Reuse_Stale_Eligibility()
	{
		var package = new PaywallPackageComponent
		{
			Id = "annual", PackageId = "annual", Stack = new()
			{
				Visible = false,
				Overrides = [Override(new { visible = true },
					Condition("intro_offer_condition", "=", value: true))]
			}
		};
		var view = CreateView(new() { Components = [package, Package("monthly")] });
		view.Packages =
		[
			new()
			{
				Identifier = "annual", StoreProduct = new()
				{
					Identifier = "product.annual", DefaultSubscriptionOption = new() { Id = "offer.a" }
				}
			},
			new() { Identifier = "monthly", StoreProduct = new() { Identifier = "product.monthly" } }
		];
		view.SemanticContext = new()
		{
			IntroOfferEligibility = new Dictionary<PaywallOfferKey, PaywallEligibility>
			{
				[new("product.annual", "offer.a")] = PaywallEligibility.Eligible
			}
		};
		Assert.IsTrue(Find<View>(view, "annual").IsVisible);
		view.Packages =
		[
			new()
			{
				Identifier = "annual", StoreProduct = new()
				{
					Identifier = "product.annual", DefaultSubscriptionOption = new() { Id = "offer.b" }
				}
			},
			new() { Identifier = "monthly", StoreProduct = new() { Identifier = "product.monthly" } }
		];
		Assert.IsFalse(Find<View>(view, "annual").IsVisible);
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

	static Package AnnualPackage() => new()
	{
		Identifier = "annual", StoreProduct = new() { Identifier = "product.annual" }
	};

	static RevenueCatPaywallView CreateView(PaywallStackComponent stack) => new()
	{
		Packages =
		[
			new() { Identifier = "monthly", StoreProduct = new() { Identifier = "product.monthly", PriceString = "$6.99" } },
			new()
			{
				Identifier = "annual",
				StoreProduct = new()
				{
					Identifier = "product.annual",
					PriceString = "$49.99",
					IntroductoryDiscount = new()
					{
						PriceString = "$0.00",
						NumberOfPeriods = 1,
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
