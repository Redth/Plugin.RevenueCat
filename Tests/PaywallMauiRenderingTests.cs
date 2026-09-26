using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Text.Json;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls;
using Plugin.RevenueCat.Paywalls.Rendering.Maui;

namespace Tests;

[TestClass]
[DoNotParallelize]
public sealed class PaywallMauiRenderingTests
{
	[TestInitialize]
	public void InitializeDispatcher() => DispatcherProvider.SetCurrent(new TestDispatcherProvider());

	[TestCleanup]
	public void ResetDispatcher() => DispatcherProvider.SetCurrent(null);

	[TestMethod]
	public void Semantic_Refresh_Preserves_Parent_Assigned_Row_Alignment()
	{
		var view = CreateView(new()
		{
			Components =
			[
				new PaywallStackComponent
				{
					Dimension = JsonSerializer.SerializeToElement(new { type = "horizontal" }),
					Components = [Text("row-label", "Plan details")]
				}
			]
		});
		var label = Find<Label>(view, "row-label");
		Assert.AreEqual(LayoutOptions.Center, label.VerticalOptions);

		view.SemanticContext = new PaywallSemanticContext();

		Assert.AreEqual(LayoutOptions.Center, label.VerticalOptions);
	}

	[TestMethod]
	public void Custom_Renderer_Retains_The_Default_Rerender_Callback_Contract()
	{
		var renderer = new RecordingRenderer();
		var view = new RevenueCatPaywallView(renderer)
		{
			Packages = [new() { Identifier = "monthly" }, new() { Identifier = "annual" }]
		};
		var content = view.Content;
		renderer.Request!.PackageSelected!("annual");

		Assert.AreNotSame(content, view.Content);
		Assert.AreEqual("annual", renderer.Request.SelectedPackageIdentifier);
	}

	[TestMethod]
	[DataRow("basic")]
	[DataRow("header_media")]
	[DataRow("package_selector")]
	[DataRow("actions_fallback")]
	[DataRow("best_value_badge")]
	[DataRow("dark_gradient")]
	[DataRow("feature_checklist")]
	[DataRow("lifetime_offer")]
	[DataRow("bottom_sheet")]
	[DataRow("two_column_benefits")]
	[DataRow("trial_story")]
	[DataRow("premium_comparison")]
	[DataRow("carousel_onboarding")]
	[DataRow("tabbed_plans")]
	[DataRow("timeline_countdown")]
	public void Every_Gallery_Fixture_Constructs_A_Managed_Maui_View_Tree(string fixture)
	{
		var response = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "paywalls", fixture + ".json")).ToPaywallOfferingsResponse();
		Assert.IsNotNull(response?.CurrentOffering?.PaywallComponents);
		var view = new RevenueCatPaywallView
		{
			Packages = response.CurrentOffering.Packages.Select(p => new Package
			{
				Identifier = p.Identifier,
				StoreProduct = new() { Title = p.Identifier, PriceString = "$1.00" }
			}).ToArray(),
			PaywallOfferings = response
		};

		Assert.IsInstanceOfType<Grid>(view.Content);
		Assert.IsTrue(Descendants(view).Any());
	}

	[TestMethod]
	public void Selecting_Package_Updates_Existing_Labels_And_Strokes_Without_Replacing_Root()
	{
		var view = CreateView(new PaywallStackComponent
		{
			Components = [PackageCard("monthly"), PackageCard("annual"), Text("selected-price", "{{ price }}"), PurchaseButton()]
		});
		var originalRoot = view.Content;
		var monthly = Find<Border>(view, "monthly");
		var annual = Find<Border>(view, "annual");
		var selectedPrice = Find<Label>(view, "selected-price");
		var monthlyPrice = Find<Label>(view, "monthly-price");
		var originalMonthlyThickness = monthly.StrokeThickness;
		var originalAnnualThickness = annual.StrokeThickness;

		Tap(annual);

		Assert.AreSame(originalRoot, view.Content);
		Assert.AreSame(selectedPrice, Find<Label>(view, "selected-price"));
		Assert.AreEqual("$49.99", selectedPrice.Text);
		Assert.AreEqual("$6.99", monthlyPrice.Text);
		Assert.AreEqual(originalMonthlyThickness, monthly.StrokeThickness);
		Assert.AreEqual(originalAnnualThickness, annual.StrokeThickness);
		Assert.AreNotEqual(monthly.Stroke, annual.Stroke);

		Tap(annual);
		Assert.AreSame(originalRoot, view.Content);

		Tap(monthly);
		Assert.AreEqual("$6.99", selectedPrice.Text);
	}

	[TestMethod]
	public void Tab_Change_Selects_Visible_Package_And_Purchase_Target_Without_Resetting_Tabs()
	{
		var view = CreateView(new PaywallStackComponent
		{
			Components =
			[
				new PaywallTabsComponent
				{
					Id = "plans",
					DefaultTabId = "annual-tab",
					Tabs =
					[
						new() { Id = "monthly-tab", Name = "Monthly", Stack = new() { Components = [PackageCard("monthly")] } },
						new() { Id = "annual-tab", Name = "Annual", Stack = new() { Components = [PackageCard("annual", true)] } }
					]
				},
				Text("footer-price", "{{ price }}"),
				PurchaseButton()
			]
		});
		var root = view.Content;
		string? purchasedPackage = null;
		view.PurchaseRequested += (_, args) => purchasedPackage = args.Request.PackageIdentifier;
		Assert.AreEqual("$49.99", Find<Label>(view, "footer-price").Text);

		var monthlyTabButton = Descendants(view).OfType<Border>()
			.Single(b => b.Content is Label { Text: "Monthly" });
		Tap(monthlyTabButton);
		var monthlyCard = Find<Border>(view, "monthly");
		Tap(monthlyCard);
		Tap(Find<View>(view, "purchase"));

		Assert.AreSame(root, view.Content);
		Assert.AreSame(monthlyCard, Find<Border>(view, "monthly"));
		Assert.AreEqual("$6.99", Find<Label>(view, "footer-price").Text);
		Assert.AreEqual("monthly", purchasedPackage);
		Assert.IsFalse(Descendants(view).Any(v => v.AutomationId == "annual"));
	}

	[TestMethod]
	public void Clearing_Offering_Removes_Previous_Paywall_And_Does_Not_Show_Draft_Automatically()
	{
		var data = new PaywallComponentsData { ComponentsConfig = new() { Base = new() { Stack = new() } } };
		var view = new RevenueCatPaywallView
		{
			PaywallOffering = new() { Identifier = "live", PaywallComponents = data }
		};
		view.PaywallOffering = null;
		Assert.IsNull(view.PaywallData);
		Assert.IsNull(view.OfferingIdentifier);

		view.PaywallOffering = new() { Identifier = "draft", DraftPaywallComponents = data };
		Assert.IsNull(view.PaywallData);
	}

	[TestMethod]
	public void Hidden_Packages_Are_Not_Selectable_Or_Default()
	{
		var hidden = PackageCard("annual", true);
		hidden.Visible = false;
		var view = CreateView(new() { Components = [hidden, PackageCard("monthly"), Text("price", "{{ price }}")] });

		Assert.AreEqual("$6.99", Find<Label>(view, "price").Text);
		Assert.IsFalse(Descendants(view).Any(v => v.AutomationId == "annual" && v.IsVisible));
	}

	[TestMethod]
	public void Unavailable_Package_Does_Not_Receive_Purchase()
	{
		var view = CreateView(new() { Components = [PackageCard("missing"), PurchaseButton()] });
		Assert.IsFalse(Find<Border>(view, "missing").IsEnabled);
		var purchases = 0;
		Exception? failure = null;
		view.PurchaseRequested += (_, _) => purchases++;
		view.ActionFailed += (_, e) => failure = e.Exception;
		Tap(Find<View>(view, "purchase"));
		Assert.AreEqual(0, purchases);
		Assert.IsNotNull(failure);
	}

	[TestMethod]
	public void Purchase_Handler_Failure_Is_Reported_And_Does_Not_Escape_Async_Gesture()
	{
		var failure = new InvalidOperationException("Synthetic purchase failure");
		var view = CreateView(new() { Components = [PurchaseButton()] });
		view.ActionHandler = new TestActionHandler(() => Task.FromException<CustomerInfo?>(failure));
		Exception? reported = null;
		view.ActionFailed += (_, args) => reported = args.Exception;

		Tap(Find<View>(view, "purchase"));

		Assert.AreSame(failure, reported);
	}

	[TestMethod]
	public void Duplicate_Purchase_Taps_Do_Not_Start_Concurrent_Operations()
	{
		var pending = new TaskCompletionSource<CustomerInfo?>();
		var calls = 0;
		var view = CreateView(new() { Components = [PurchaseButton(), new PaywallPurchaseButtonComponent { Id = "second-purchase" }] });
		view.ActionHandler = new TestActionHandler(() =>
		{
			calls++;
			return pending.Task;
		});
		var button = Find<View>(view, "purchase");
		Tap(button);
		Tap(button);
		Tap(Find<View>(view, "second-purchase"));
		Assert.AreEqual(1, calls);
		pending.SetResult(null);
	}

	[TestMethod]
	public void Cyclic_Color_Alias_Reports_Invalid_Data_Instead_Of_Overflowing_The_Stack()
	{
		var alias = JsonSerializer.SerializeToElement(new { type = "alias", value = "loop" });
		var config = new PaywallUiConfig();
		config.App.Colors["loop"] = alias;
		Assert.ThrowsExactly<JsonException>(() => PaywallMauiStyleResolver.ResolveColor(alias, config));
	}

	[TestMethod]
	public void Web_Checkout_Is_Not_Silently_Replaced_With_In_App_Purchase()
	{
		var view = CreateView(new()
		{
			Components = [new PaywallPurchaseButtonComponent { Id = "purchase", Action = "web_checkout" }]
		});
		var purchases = 0;
		Exception? failure = null;
		view.PurchaseRequested += (_, _) => purchases++;
		view.ActionFailed += (_, e) => failure = e.Exception;

		Tap(Find<View>(view, "purchase"));

		Assert.AreEqual(0, purchases);
		Assert.IsInstanceOfType<NotSupportedException>(failure);
	}

	[TestMethod]
	public void Navigation_Resolves_Localized_Url_Instead_Of_Sending_The_Localization_Key()
	{
		var view = CreateView(new()
		{
			Components =
			[
				new PaywallButtonComponent
				{
					Id = "terms",
					Action = new() { Type = "navigate_to", Url = JsonSerializer.SerializeToElement(new { url_lid = "terms_url" }) }
				}
			]
		});
		view.PaywallData!.DefaultLocale = "en";
		view.PaywallData.ComponentsLocalizations["en"] = new()
		{
			["terms_url"] = new() { Text = "https://example.com/terms" }
		};
		string? url = null;
		view.NavigationRequested += (_, e) => url = e.Request.Url;

		Tap(Find<View>(view, "terms"));

		Assert.AreEqual("https://example.com/terms", url);
	}

	[TestMethod]
	public void Carousel_Templates_Create_Distinct_Views_For_The_Same_Page_Model()
	{
		var page = new PaywallStackComponent { Components = [Text("page-title", "Hello")] };
		var view = CreateView(new()
		{
			Components = [new PaywallCarouselComponent { Id = "carousel", Pages = [page, page] }]
		});
		var carousel = Find<CarouselView>(view, "carousel");
		var first = (ContentView)carousel.ItemTemplate.CreateContent();
		var second = (ContentView)carousel.ItemTemplate.CreateContent();
		first.BindingContext = page;
		second.BindingContext = page;

		Assert.IsNotNull(first.Content);
		Assert.IsNotNull(second.Content);
		Assert.AreNotSame(first.Content, second.Content);
	}

	static RevenueCatPaywallView CreateView(PaywallStackComponent stack) => new()
	{
		Packages =
		[
			new() { Identifier = "monthly", StoreProduct = new() { PriceString = "$6.99" } },
			new() { Identifier = "annual", StoreProduct = new() { PriceString = "$49.99" } }
		],
		PaywallData = new() { ComponentsConfig = new() { Base = new() { Stack = stack } } }
	};

	static PaywallPackageComponent PackageCard(string identifier, bool selected = false) => new()
	{
		Id = identifier,
		PackageId = identifier,
		IsSelectedByDefault = selected,
		Stack = new() { Components = [Text(identifier + "-price", "{{ price }}")] }
	};

	static PaywallTextComponent Text(string id, string text) => new() { Id = id, TextLocalizationId = text };

	static PaywallPurchaseButtonComponent PurchaseButton() => new() { Id = "purchase" };

	static T Find<T>(View root, string id) where T : View =>
		Descendants(root).OfType<T>().Single(v => v.AutomationId == id);

	static void Tap(View view)
	{
		var command = view.GestureRecognizers.OfType<TapGestureRecognizer>().Single().Command;
		Assert.IsNotNull(command);
		command.Execute(null);
	}

	static IEnumerable<View> Descendants(View root)
	{
		yield return root;
		IEnumerable<View> children = root switch
		{
			Layout layout => layout.Children.OfType<View>(),
			ContentView { Content: View content } => [content],
			Border { Content: View content } => [content],
			ScrollView { Content: View content } => [content],
			_ => []
		};
		foreach (var child in children.SelectMany(Descendants))
		{
			yield return child;
		}
	}

	sealed class TestDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetForCurrentThread() => new TestDispatcher();
	}

	sealed class RecordingRenderer : IPaywallRenderer
	{
		public PaywallRenderRequest? Request { get; private set; }
		public View Render(PaywallRenderRequest request)
		{
			Request = request;
			return new Label { Text = request.SelectedPackageIdentifier };
		}
	}

	sealed class TestDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;
		public bool Dispatch(Action action) { action(); return true; }
		public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException("No native scheduling in managed view-tree tests.");
		public IDispatcherTimer CreateTimer() => throw new NotSupportedException("Timers must not run for unloaded views.");
	}

	sealed class TestActionHandler(Func<Task<CustomerInfo?>> purchase) : IPaywallActionHandler
	{
		public Task<CustomerInfo?> PurchaseAsync(PaywallPurchaseRequest request, CancellationToken cancellationToken = default) => purchase();
		public Task<CustomerInfo?> RestoreAsync(CancellationToken cancellationToken = default) => Task.FromResult<CustomerInfo?>(null);
		public Task DismissAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
		public Task NavigateAsync(PaywallNavigationRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
	}
}
