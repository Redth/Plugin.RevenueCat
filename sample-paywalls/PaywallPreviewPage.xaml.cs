using Plugin.RevenueCat.Paywalls;

namespace PaywallGallerySample;

public partial class PaywallPreviewPage : ContentPage
{
	readonly PaywallExample example;

	public PaywallPreviewPage(PaywallExample example)
	{
		this.example = example;
		InitializeComponent();
		BindingContext = example;
		PaywallView.ActionFailed += (_, args) => AddEvent($"Action failed: {args.Exception.Message}");
		PaywallView.SemanticDiagnostic += (_, message) => AddEvent($"Diagnostic: {message}");

		PaywallView.Packages = example.Packages;
		PaywallView.PaywallOfferings = example.Response;
		AddEvent($"Loaded {example.Title}");
		if (example.HasVariants)
		{
			ScenarioPicker.SelectedIndex = 0;
		}
	}

	void OnScenarioChanged(object? sender, EventArgs e)
	{
		if (ScenarioPicker.SelectedItem is not PaywallGalleryVariant variant)
		{
			return;
		}
		// Revoke any earlier eligibility before replacing the mocked catalog/customer facts.
		PaywallView.SemanticContext = null;
		PaywallView.Packages = variant.SelectPackages(example.Packages);
		PaywallView.Locale = variant.Locale;
		PaywallView.SemanticContext = variant.CreateContext();
		if (variant.CreateTimedPaywall(example.Response.CurrentOffering?.PaywallComponents, DateTimeOffset.UtcNow) is { } timed)
		{
			PaywallView.PaywallData = timed;
		}
		ExpectedOutcomeLabel.Text = $"Expected: {variant.ExpectedOutcome}";
		AddEvent($"Mocked input: {variant.Name}");
	}

	void OnPurchaseRequested(object? sender, PaywallPurchaseRequestedEventArgs e) =>
		AddEvent($"Purchase requested: offering={e.Request.OfferingIdentifier}, package={e.Request.PackageIdentifier}");

	void OnRestoreRequested(object? sender, PaywallRestoreRequestedEventArgs e) =>
		AddEvent("Restore requested");

	void OnDismissRequested(object? sender, PaywallDismissRequestedEventArgs e) =>
		AddEvent("Dismiss requested");

	void OnNavigationRequested(object? sender, PaywallNavigationRequestedEventArgs e) =>
		AddEvent($"Navigation requested: action={e.Request.ActionType}, destination={e.Request.Url ?? e.Request.Destination ?? "unknown"}");

	void AddEvent(string message)
	{
		EventLogLabel.Text = $"{DateTime.Now:T} - {message}";
	}
}
