using System.Text.Json;
using Microsoft.Maui.Controls;
using RoundRectangle = Microsoft.Maui.Controls.Shapes.RoundRectangle;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using PaywallGallerySample;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls;
using Plugin.RevenueCat.Paywalls.Rendering.Maui;

namespace Tests;

[TestClass]
[DoNotParallelize]
public sealed class PaywallGalleryScenarioTests
{
	static readonly string[] Cases =
	[
		"verified_eligibility", "verified_rules", "verified_tab_state", "verified_mixed_packages",
		"verified_window_rules", "verified_localization", "verified_fallbacks", "verified_countdown"
	];

	[TestInitialize]
	public void Initialize() => DispatcherProvider.SetCurrent(new TestDispatcherProvider());

	[TestCleanup]
	public void Cleanup() => DispatcherProvider.SetCurrent(null);

	public static IEnumerable<object[]> Variants()
	{
		foreach (var fixture in Cases)
		{
			var example = Load(fixture);
			for (var i = 0; i < example.Variants.Count; i++)
			{
				yield return [fixture, i, example.Variants[i].Name];
			}
		}
	}

	[TestMethod]
	[DynamicData(nameof(Variants))]
	public void Gallery_Variant_Matches_Documented_Visible_Text_And_Purchase(
		string fixture, int variantIndex, string variantName)
	{
		var example = Load(fixture);
		var variant = example.Variants[variantIndex];
		var view = Create(example, variant);
		Assert.AreEqual(variantName, variant.Name);
		Assert.IsFalse(string.IsNullOrWhiteSpace(variant.ExpectedOutcome));
		Assert.IsTrue(example.GalleryCase!.Sources.All(source =>
			Uri.TryCreate(source, UriKind.Absolute, out var uri) &&
			uri.Host == "github.com" && uri.Segments.Any(segment => segment.Trim('/').Length == 40)));

		foreach (var (id, expected) in variant.ExpectedText)
		{
			var label = Find<Label>(view, id);
			Assert.AreEqual(expected, label.Text, $"{fixture}/{variantName}: {id}");
			Assert.IsTrue(IsVisible(label), $"{id} must actually be visible, not just constructed.");
		}
		foreach (var (id, expected) in variant.ExpectedVisible)
		{
			Assert.AreEqual(expected, IsVisible(Find<View>(view, id)), $"{fixture}/{variantName}: {id}");
		}
		if (variant.ExpectedPurchasePackage is { } package)
		{
			Assert.AreEqual(package, Purchase(view));
		}
	}

	[TestMethod]
	public async Task Real_Gallery_Catalog_Registers_All_Fixtures_And_Has_Unique_Preview_Ids()
	{
		var loaded = await PaywallExampleLoader.LoadAsync(asset =>
			Task.FromResult<Stream>(File.OpenRead(Path.Combine(AppContext.BaseDirectory, asset))));
		Assert.AreEqual(23, loaded.Count);
		Assert.AreEqual(8, loaded.Count(e => e.HasVariants));
		Assert.AreEqual(23, loaded.Sum(e => e.Variants.Count));
		Assert.AreEqual(loaded.Count, loaded.Select(e => e.AutomationId).Distinct(StringComparer.Ordinal).Count());
		Assert.AreEqual(Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "paywalls"), "*.json").Length, loaded.Count);
	}

	[TestMethod]
	public void Every_Source_Backed_Gallery_Fixture_Is_In_The_Assertion_Corpus()
	{
		var files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "paywalls"), "verified_*.json")
			.Select(Path.GetFileNameWithoutExtension).Order(StringComparer.Ordinal).ToArray();
		CollectionAssert.AreEqual(Cases.Order(StringComparer.Ordinal).ToArray(), files);
		Assert.AreEqual(23, Variants().Count());
	}

	[TestMethod]
	public void Eligibility_Presets_Update_Existing_Accessible_Copy_Without_Promising_A_Trial_For_Unknown()
	{
		var example = Load("verified_eligibility");
		var view = Create(example, example.Variants[0]);
		var root = view.Content;
		var card = Find<View>(view, "annual");
		var surface = PaywallAccessibility.GetSurface(card)!;
		Assert.IsFalse(SemanticProperties.GetDescription(surface).Contains("Try", StringComparison.Ordinal));
		Assert.AreEqual("$rc_annual", Purchase(view));

		ApplyInputs(view, example, example.Variants[1]);
		Assert.AreSame(root, view.Content);
		Assert.IsTrue(SemanticProperties.GetDescription(surface).Contains("week at $0.00", StringComparison.Ordinal));
		Assert.AreEqual("$rc_annual", Purchase(view));

		foreach (var variant in new[] { example.Variants[2], example.Variants[3], example.Variants[0] })
		{
			ApplyInputs(view, example, variant);
			Assert.AreSame(root, view.Content);
			Assert.AreSame(surface, PaywallAccessibility.GetSurface(card));
			Assert.IsFalse(IsVisible(Find<View>(view, "trial-detail")));
			Assert.IsFalse(SemanticProperties.GetDescription(surface).Contains("Try", StringComparison.Ordinal));
			Assert.AreEqual("$rc_annual", Purchase(view));
		}
	}

	[TestMethod]
	public void Audience_And_Selected_Package_Rules_Use_All_Conditions_And_Last_Match()
	{
		var example = Load("verified_rules");
		var view = Create(example, example.Variants[1]);
		var root = view.Content;
		Tap(view, "annual");
		Assert.AreEqual("VIP annual access", Find<Label>(view, "audience-message").Text);
		Assert.AreEqual("$rc_annual", Purchase(view));
		Assert.AreEqual("Monthly Pro - $6.99", Find<Label>(view, "monthly-price").Text);
		Assert.AreEqual("Annual Pro - $49.99", Find<Label>(view, "annual-price").Text);

		ApplyInputs(view, example, example.Variants[0]);
		Assert.AreSame(root, view.Content);
		Assert.AreEqual("Standard access", Find<Label>(view, "audience-message").Text);
		Assert.AreEqual("$rc_annual", Purchase(view));
		ApplyInputs(view, example, example.Variants[2]);
		Assert.AreEqual("Standard access", Find<Label>(view, "audience-message").Text);
	}

	[TestMethod]
	public void Carousel_Uses_Page_Indicators_Without_Native_Scrollbars()
	{
		var example = Load("carousel_onboarding");
		var view = Create(example, new PaywallGalleryVariant());
		var carousel = Descendants(Find<View>(view, "value-carousel")).OfType<CarouselView>().Single();
		var indicator = Descendants(view).OfType<IndicatorView>().Single();
		Assert.AreEqual(ScrollBarVisibility.Never, carousel.HorizontalScrollBarVisibility);
		Assert.AreEqual(ScrollBarVisibility.Never, carousel.VerticalScrollBarVisibility);
		Assert.IsTrue(carousel.IsSwipeEnabled);
		Assert.IsTrue(carousel.Loop);
		Assert.IsTrue(indicator.IsVisible);
		var body = Descendants(view).OfType<ScrollView>().Single();
		Assert.AreEqual(ScrollOrientation.Vertical, body.Orientation);
		Assert.AreEqual(ScrollBarVisibility.Default, body.VerticalScrollBarVisibility);
	}

	[TestMethod]
	[DataRow("best_value_badge", "annual-card", "monthly-card")]
	[DataRow("premium_comparison", "annual", "monthly")]
	public void Badged_Package_Selection_Uses_The_Authored_Card_Border(
		string fixture, string annualId, string monthlyId)
	{
		var example = Load(fixture);
		var view = Create(example, new PaywallGalleryVariant());
		var root = view.Content;
		var outer = Find<Border>(view, annualId);
		var card = Descendants(outer).OfType<Border>().Skip(1).First();
		var badge = Descendants(outer).OfType<Border>().Last();
		var action = PaywallAccessibility.GetSurface(outer);
		var shape = card.StrokeShape;
		Assert.IsInstanceOfType<RoundRectangle>(shape);
		Assert.AreEqual(new CornerRadius(28), ((RoundRectangle)shape).CornerRadius);
		Assert.AreEqual(0, outer.StrokeThickness, "The accessibility wrapper must not add an outline.");
		Assert.AreEqual(2, card.StrokeThickness);
		Assert.IsInstanceOfType<SolidColorBrush>(card.Stroke);
		Assert.AreEqual(Colors.DeepSkyBlue, ((SolidColorBrush)card.Stroke).Color);
		Assert.AreEqual(0, badge.StrokeThickness);

		Tap(view, monthlyId);
		Assert.AreSame(shape, card.StrokeShape);
		Assert.AreEqual(2, card.StrokeThickness);
		Assert.AreEqual(0, outer.StrokeThickness);
		Assert.IsFalse(PaywallAccessibility.GetSelected(outer));
		Assert.AreEqual(
			fixture == "best_value_badge" ? Color.FromArgb("#22c55e") : Colors.Transparent,
			((SolidColorBrush)card.Stroke).Color);

		Tap(view, annualId);
		Assert.AreSame(root, view.Content);
		Assert.AreSame(action, PaywallAccessibility.GetSurface(outer));
		Assert.AreSame(shape, card.StrokeShape);
		Assert.AreEqual(0, outer.StrokeThickness);
		Assert.AreEqual(2, card.StrokeThickness);
		Assert.AreEqual(Colors.DeepSkyBlue, ((SolidColorBrush)card.Stroke).Color);
		Assert.AreEqual("$rc_annual", Purchase(view));
	}

	[TestMethod]
	[DataRow("verified_tab_state", "weekly-tab", "monthly-tab")]
	[DataRow("verified_mixed_packages", "details-tab", "weekly-tab")]
	public void Tab_Content_Is_Centered_Within_The_Accessible_Hit_Target(
		string fixture, string firstTab, string secondTab)
	{
		var example = Load(fixture);
		var view = Create(example, example.Variants[0]);

		void AssertCentered(string id)
		{
			var tab = Find<Border>(view, id);
			var layers = tab.Content as Grid;
			Assert.IsNotNull(layers);
			var decoration = layers.Children.OfType<ContentView>().Single();
			var action = PaywallAccessibility.GetSurface(tab);
			Assert.IsNotNull(action);
			Assert.AreEqual(LayoutOptions.Center, decoration.VerticalOptions);
			Assert.AreEqual(LayoutOptions.Fill, decoration.HorizontalOptions);
			Assert.AreEqual(8, decoration.Padding.Top);
			Assert.AreEqual(8, decoration.Padding.Bottom);
			Assert.AreEqual(LayoutOptions.Fill, action.VerticalOptions);
			Assert.IsTrue(action.MinimumHeightRequest >= 44);
		}

		AssertCentered(firstTab);
		AssertCentered(secondTab);
		Tap(view, secondTab);
		AssertCentered(firstTab);
		AssertCentered(secondTab);
		view.SemanticContext = new PaywallSemanticContext();
		AssertCentered(firstTab);
		AssertCentered(secondTab);
	}

	[TestMethod]
	public void Three_Tab_Selections_Keep_State_Price_Accessibility_And_Purchase_In_Sync()
	{
		var example = Load("verified_tab_state");
		var view = Create(example, example.Variants[0]);
		var root = view.Content;
		var state = Find<Label>(view, "state-caption");
		foreach (var (tab, text, package, price) in new[]
		{
			("weekly", "Weekly selected", "$rc_weekly", "$2.99"),
			("monthly", "Monthly selected", "$rc_monthly", "$6.99"),
			("annual", "Annual selected", "$rc_annual", "$49.99")
		})
		{
			Tap(view, tab + "-tab");
			Assert.AreSame(root, view.Content);
			Assert.AreSame(state, Find<Label>(view, "state-caption"));
			Assert.AreEqual(text, state.Text);
			Assert.AreEqual("Continue for " + price, Find<Label>(view, "purchase-copy").Text);
			Assert.IsTrue(PaywallAccessibility.GetSelected(Find<View>(view, tab + "-package")));
			Assert.AreEqual(package, Purchase(view));
		}
	}

	[TestMethod]
	public void Leaving_A_Package_Tab_Does_Not_Restore_Hidden_Default_And_Catalog_Changes_Disable_Actions()
	{
		var example = Load("verified_mixed_packages");
		var view = Create(example, example.Variants[0]);
		var root = view.Content;
		Assert.AreEqual("$rc_monthly", Purchase(view));
		Tap(view, "weekly-tab");
		Assert.AreEqual("$rc_weekly", Purchase(view));
		Tap(view, "details-tab");
		Assert.AreEqual("$rc_monthly", Purchase(view));
		ApplyInputs(view, example, example.Variants[1]);
		Tap(view, "annual");
		Assert.AreEqual("$rc_annual", Purchase(view));
		ApplyInputs(view, example, example.Variants[2]);
		Assert.IsFalse(PaywallAccessibility.GetSurface(Find<View>(view, "annual"))!.IsEnabled);
		Assert.AreEqual("$rc_monthly", Purchase(view));
		Assert.AreSame(root, view.Content);
	}

	[TestMethod]
	public void Window_Threshold_And_Unmatch_Restore_Original_Size_And_Text()
	{
		var example = Load("verified_window_rules");
		var view = Create(example, example.Variants[1]);
		var root = view.Content;
		var panel = Find<View>(view, "responsive-panel");
		Assert.AreEqual(-1, panel.HeightRequest);
		Assert.AreEqual(17, Find<Label>(view, "width-copy").FontSize);
		ApplyInputs(view, example, example.Variants[2]);
		Assert.AreEqual(88, panel.HeightRequest);
		Assert.AreEqual(22, Find<Label>(view, "width-copy").FontSize);
		ApplyInputs(view, example, example.Variants[3]);
		Assert.IsTrue(IsVisible(Find<View>(view, "landscape-note")));
		ApplyInputs(view, example, example.Variants[1]);
		Assert.AreSame(root, view.Content);
		Assert.AreSame(panel, Find<View>(view, "responsive-panel"));
		Assert.AreEqual(-1, panel.HeightRequest);
		Assert.AreEqual(17, Find<Label>(view, "width-copy").FontSize);
		Assert.IsFalse(IsVisible(Find<View>(view, "landscape-note")));
	}

	[TestMethod]
	[DataRow(0, "https://example.com/en/terms")]
	[DataRow(1, "https://example.com/es/terms")]
	[DataRow(2, "https://example.com/es/terms")]
	[DataRow(3, "https://example.com/de/terms")]
	[DataRow(4, "https://example.com/en/terms")]
	public void Legal_Links_Are_Localized_Urls_Not_Resource_Keys(int variant, string expected)
	{
		var example = Load("verified_localization");
		var view = Create(example, example.Variants[variant]);
		string? url = null;
		view.NavigationRequested += (_, e) => url = e.Request.Url;
		Tap(view, "terms");
		Assert.AreEqual(expected, url);
	}

	[TestMethod]
	public void Unsupported_Checkout_Fails_Without_Purchase_And_Media_Uses_Author_Fallbacks()
	{
		var example = Load("verified_fallbacks");
		var view = Create(example, example.Variants[0]);
		var purchases = 0;
		Exception? failure = null;
		view.PurchaseRequested += (_, _) => purchases++;
		view.ActionFailed += (_, e) => failure = e.Exception;
		Tap(view, "unsupported-purchase");
		Assert.AreEqual(0, purchases);
		Assert.IsInstanceOfType<NotSupportedException>(failure);
		Assert.IsFalse(Descendants(view).Any(v => v is Image or WebView or SvgPaywallImageView));
	}

	static PaywallExample Load(string fixture)
	{
		var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "paywalls", fixture + ".json"));
		var response = json.ToPaywallOfferingsResponse() ?? throw new JsonException(fixture);
		return new PaywallExample(fixture, "Source-backed case", "", response);
	}

	static RevenueCatPaywallView Create(PaywallExample example, PaywallGalleryVariant variant)
	{
		var view = new RevenueCatPaywallView
		{
			Packages = variant.SelectPackages(example.Packages),
			SemanticContext = variant.CreateContext(),
			Locale = variant.Locale,
			PaywallOfferings = example.Response
		};
		if (variant.CreateTimedPaywall(example.Response.CurrentOffering?.PaywallComponents, DateTimeOffset.UtcNow) is { } timed)
		{
			view.PaywallData = timed;
		}
		return view;
	}

	[TestMethod]
	public void Timed_Presets_Use_A_New_Deadline_Without_Mutating_The_Shared_Fixture()
	{
		var example = Load("verified_countdown");
		var data = example.Response.CurrentOffering!.PaywallComponents!;
		var instant = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
		var running = example.Variants[0].CreateTimedPaywall(data, instant)!;
		var expired = example.Variants[1].CreateTimedPaywall(data, instant)!;
		DateTimeOffset Date(PaywallComponentsData value) => value.ComponentsConfig!.Base!.Stack!.Components
			.OfType<PaywallCountdownComponent>().Single().Style!.Value.GetProperty("date").GetDateTimeOffset();
		Assert.AreEqual(instant.AddSeconds(10), Date(running));
		Assert.AreEqual(instant.AddSeconds(-1), Date(expired));
		Assert.AreEqual(2000, Date(data).Year);
		var view = Create(example, example.Variants[1]);
		Assert.AreEqual("Offer ended", Find<Label>(view, "expired-copy").Text);
		Assert.IsFalse(Descendants(view).Any(v => v.AutomationId == "remaining"));
	}

	static void ApplyInputs(RevenueCatPaywallView view, PaywallExample example, PaywallGalleryVariant variant)
	{
		view.SemanticContext = null;
		view.Packages = variant.SelectPackages(example.Packages);
		view.Locale = variant.Locale;
		view.SemanticContext = variant.CreateContext();
	}

	static string? Purchase(RevenueCatPaywallView view)
	{
		string? package = null;
		void Capture(object? sender, PaywallPurchaseRequestedEventArgs e) => package = e.Request.PackageIdentifier;
		view.PurchaseRequested += Capture;
		try { Tap(view, "purchase"); }
		finally { view.PurchaseRequested -= Capture; }
		return package;
	}

	static void Tap(View root, string id)
	{
		var target = Find<View>(root, id);
		var button = PaywallAccessibility.GetSurface(target);
		Assert.IsNotNull(button, id);
		Assert.IsNotNull(button.Command, id);
		button.Command.Execute(null);
	}

	static T Find<T>(View root, string id) where T : View =>
		Descendants(root).OfType<T>().Single(v => v.AutomationId == id);

	static bool IsVisible(View view)
	{
		for (Element? element = view; element is not null; element = element.Parent)
			if (element is VisualElement { IsVisible: false }) return false;
		return true;
	}

	static IEnumerable<View> Descendants(View view)
	{
		yield return view;
		IEnumerable<View> children = view switch
		{
			Layout layout => layout.Children.OfType<View>(),
			Border { Content: View child } => [child],
			ContentView { Content: View child } => [child],
			ScrollView { Content: View child } => [child],
			_ => []
		};
		foreach (var child in children.SelectMany(Descendants)) yield return child;
	}

	sealed class TestDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetForCurrentThread() => new TestDispatcher();
	}

	sealed class TestDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;
		public bool Dispatch(Action action) { action(); return true; }
		public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException();
		public IDispatcherTimer CreateTimer() => throw new NotSupportedException("No timer should start in an unloaded gallery view.");
	}
}
