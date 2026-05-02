using Plugin.RevenueCat.Paywalls;

namespace PaywallGallerySample;

public partial class PaywallPreviewPage : ContentPage
{
	public PaywallPreviewPage(PaywallExample example)
	{
		InitializeComponent();
		BindingContext = example;

		PaywallView.Packages = example.Packages;
		PaywallView.PaywallOfferings = example.Response;
		AddEvent($"Loaded {example.Title}");
	}

	void OnPurchaseRequested(object? sender, PaywallPurchaseRequestedEventArgs e) =>
		AddEvent($"Purchase requested: offering={e.Request.OfferingIdentifier}, package={e.Request.PackageIdentifier}");

	void OnRestoreRequested(object? sender, PaywallRestoreRequestedEventArgs e) =>
		AddEvent("Restore requested");

	void OnDismissRequested(object? sender, PaywallDismissRequestedEventArgs e) =>
		AddEvent("Dismiss requested");

	void OnNavigationRequested(object? sender, PaywallNavigationRequestedEventArgs e) =>
		AddEvent($"Navigation requested: action={e.Request.ActionType}, destination={e.Request.Destination ?? e.Request.Url ?? "unknown"}");

	void AddEvent(string message)
	{
		EventLogLabel.Text = $"{DateTime.Now:T} - {message}";
	}
}
