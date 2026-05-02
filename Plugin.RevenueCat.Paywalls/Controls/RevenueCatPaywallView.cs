#nullable enable

using Microsoft.Maui.Controls;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls.Rendering.Maui;

namespace Plugin.RevenueCat.Paywalls;

public class RevenueCatPaywallView : ContentView
{
	readonly IPaywallRenderer renderer;
	string? selectedPackageIdentifier;

	public RevenueCatPaywallView()
		: this(new DefaultPaywallRenderer())
	{
	}

	public RevenueCatPaywallView(IPaywallRenderer renderer)
	{
		this.renderer = renderer;
	}

	public static readonly BindableProperty PaywallDataProperty = BindableProperty.Create(
		nameof(PaywallData),
		typeof(PaywallComponentsData),
		typeof(RevenueCatPaywallView),
		default(PaywallComponentsData),
		propertyChanged: OnRenderPropertyChanged);

	public static readonly BindableProperty UiConfigProperty = BindableProperty.Create(
		nameof(UiConfig),
		typeof(PaywallUiConfig),
		typeof(RevenueCatPaywallView),
		default(PaywallUiConfig),
		propertyChanged: OnRenderPropertyChanged);

	public static readonly BindableProperty PackagesProperty = BindableProperty.Create(
		nameof(Packages),
		typeof(IReadOnlyList<Package>),
		typeof(RevenueCatPaywallView),
		Array.Empty<Package>(),
		propertyChanged: OnRenderPropertyChanged);

	public static readonly BindableProperty LocaleProperty = BindableProperty.Create(
		nameof(Locale),
		typeof(string),
		typeof(RevenueCatPaywallView),
		default(string),
		propertyChanged: OnRenderPropertyChanged);

	public static readonly BindableProperty OfferingIdentifierProperty = BindableProperty.Create(
		nameof(OfferingIdentifier),
		typeof(string),
		typeof(RevenueCatPaywallView),
		default(string),
		propertyChanged: OnRenderPropertyChanged);

	public static readonly BindableProperty PlatformContextProperty = BindableProperty.Create(
		nameof(PlatformContext),
		typeof(object),
		typeof(RevenueCatPaywallView),
		default(object));

	public static readonly BindableProperty ActionHandlerProperty = BindableProperty.Create(
		nameof(ActionHandler),
		typeof(IPaywallActionHandler),
		typeof(RevenueCatPaywallView),
		default(IPaywallActionHandler));

	public PaywallComponentsData? PaywallData
	{
		get => (PaywallComponentsData?)GetValue(PaywallDataProperty);
		set => SetValue(PaywallDataProperty, value);
	}

	public PaywallUiConfig? UiConfig
	{
		get => (PaywallUiConfig?)GetValue(UiConfigProperty);
		set => SetValue(UiConfigProperty, value);
	}

	public IReadOnlyList<Package> Packages
	{
		get => (IReadOnlyList<Package>)GetValue(PackagesProperty);
		set => SetValue(PackagesProperty, value);
	}

	public string? Locale
	{
		get => (string?)GetValue(LocaleProperty);
		set => SetValue(LocaleProperty, value);
	}

	public string? OfferingIdentifier
	{
		get => (string?)GetValue(OfferingIdentifierProperty);
		set => SetValue(OfferingIdentifierProperty, value);
	}

	public object? PlatformContext
	{
		get => GetValue(PlatformContextProperty);
		set => SetValue(PlatformContextProperty, value);
	}

	public IPaywallActionHandler? ActionHandler
	{
		get => (IPaywallActionHandler?)GetValue(ActionHandlerProperty);
		set => SetValue(ActionHandlerProperty, value);
	}

	public void Render()
	{
		selectedPackageIdentifier ??= Packages.FirstOrDefault()?.Identifier;

		Content = renderer.Render(new PaywallRenderRequest
		{
			PaywallData = PaywallData,
			UiConfig = UiConfig,
			Packages = Packages,
			Locale = Locale,
			OfferingIdentifier = OfferingIdentifier,
			SelectedPackageIdentifier = selectedPackageIdentifier,
			PlatformContext = PlatformContext,
			ActionHandler = ActionHandler,
			PackageSelected = packageIdentifier =>
			{
				selectedPackageIdentifier = packageIdentifier;
				Render();
			}
		});
	}

	static void OnRenderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RevenueCatPaywallView view)
		{
			view.Render();
		}
	}
}
