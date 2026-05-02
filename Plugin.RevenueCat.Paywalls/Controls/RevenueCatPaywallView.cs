#nullable enable

using Microsoft.Maui.Controls;
using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls.Rendering.Maui;

namespace Plugin.RevenueCat.Paywalls;

public class RevenueCatPaywallView : ContentView
{
	readonly IPaywallRenderer renderer;
	readonly IPaywallActionHandler eventActionHandler;
	string? selectedPackageIdentifier;

	public RevenueCatPaywallView()
		: this(new DefaultPaywallRenderer())
	{
	}

	public RevenueCatPaywallView(IPaywallRenderer renderer)
	{
		this.renderer = renderer;
		eventActionHandler = new EventPaywallActionHandler(this);
	}

	public event EventHandler<PaywallPurchaseRequestedEventArgs>? PurchaseRequested;

	public event EventHandler<PaywallRestoreRequestedEventArgs>? RestoreRequested;

	public event EventHandler<PaywallDismissRequestedEventArgs>? DismissRequested;

	public event EventHandler<PaywallNavigationRequestedEventArgs>? NavigationRequested;

	public static readonly BindableProperty PaywallOfferingsProperty = BindableProperty.Create(
		nameof(PaywallOfferings),
		typeof(PaywallOfferingsResponse),
		typeof(RevenueCatPaywallView),
		default(PaywallOfferingsResponse),
		propertyChanged: OnPaywallOfferingsChanged);

	public static readonly BindableProperty PaywallOfferingProperty = BindableProperty.Create(
		nameof(PaywallOffering),
		typeof(PaywallOffering),
		typeof(RevenueCatPaywallView),
		default(PaywallOffering),
		propertyChanged: OnPaywallOfferingChanged);

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

	public static readonly BindableProperty ApplicationNameProperty = BindableProperty.Create(
		nameof(ApplicationName),
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
		default(IPaywallActionHandler),
		propertyChanged: OnRenderPropertyChanged);

	public static readonly BindableProperty VariableProviderProperty = BindableProperty.Create(
		nameof(VariableProvider),
		typeof(IPaywallVariableProvider),
		typeof(RevenueCatPaywallView),
		default(IPaywallVariableProvider),
		propertyChanged: OnRenderPropertyChanged);

	public PaywallOfferingsResponse? PaywallOfferings
	{
		get => (PaywallOfferingsResponse?)GetValue(PaywallOfferingsProperty);
		set => SetValue(PaywallOfferingsProperty, value);
	}

	public PaywallOffering? PaywallOffering
	{
		get => (PaywallOffering?)GetValue(PaywallOfferingProperty);
		set => SetValue(PaywallOfferingProperty, value);
	}

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

	public string? ApplicationName
	{
		get => (string?)GetValue(ApplicationNameProperty);
		set => SetValue(ApplicationNameProperty, value);
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

	public IPaywallVariableProvider? VariableProvider
	{
		get => (IPaywallVariableProvider?)GetValue(VariableProviderProperty);
		set => SetValue(VariableProviderProperty, value);
	}

	public void Render()
	{
		selectedPackageIdentifier ??= FindDefaultSelectedPackageIdentifier(PaywallData) ?? Packages.FirstOrDefault()?.Identifier;
		var actionHandler = ActionHandler ?? eventActionHandler;

		Content = renderer.Render(new PaywallRenderRequest
		{
			PaywallData = PaywallData,
			UiConfig = UiConfig,
			Packages = Packages,
			Locale = Locale,
			ApplicationName = ApplicationName,
			OfferingIdentifier = OfferingIdentifier,
			SelectedPackageIdentifier = selectedPackageIdentifier,
			PlatformContext = PlatformContext,
			ActionHandler = actionHandler,
			VariableProvider = VariableProvider,
			PackageSelected = packageIdentifier =>
			{
				selectedPackageIdentifier = packageIdentifier;
				Render();
			}
		});
	}

	protected virtual void OnPurchaseRequested(PaywallPurchaseRequestedEventArgs e) =>
		PurchaseRequested?.Invoke(this, e);

	protected virtual void OnRestoreRequested(PaywallRestoreRequestedEventArgs e) =>
		RestoreRequested?.Invoke(this, e);

	protected virtual void OnDismissRequested(PaywallDismissRequestedEventArgs e) =>
		DismissRequested?.Invoke(this, e);

	protected virtual void OnNavigationRequested(PaywallNavigationRequestedEventArgs e) =>
		NavigationRequested?.Invoke(this, e);

	static void OnPaywallOfferingsChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RevenueCatPaywallView view && newValue is PaywallOfferingsResponse response)
		{
			view.UiConfig = response.UiConfig;
			view.PaywallOffering = response.CurrentOffering ?? response.Offerings.FirstOrDefault();
		}
	}

	static void OnPaywallOfferingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RevenueCatPaywallView view && newValue is PaywallOffering offering)
		{
			var paywallData = offering.PaywallComponents ?? offering.DraftPaywallComponents;
			view.selectedPackageIdentifier = FindDefaultSelectedPackageIdentifier(paywallData);
			view.OfferingIdentifier = offering.Identifier;
			view.PaywallData = paywallData;
		}
	}

	static string? FindDefaultSelectedPackageIdentifier(PaywallComponentsData? paywallData)
	{
		var config = paywallData?.ComponentsConfig;
		if (config?.Base is null)
		{
			return null;
		}

		return FindDefaultSelectedPackageIdentifier(config.Base.Header)
			?? FindDefaultSelectedPackageIdentifier(config.Base.Stack)
			?? FindDefaultSelectedPackageIdentifier(config.Base.StickyFooter);
	}

	static string? FindDefaultSelectedPackageIdentifier(PaywallComponent? component)
	{
		return component switch
		{
			PaywallPackageComponent { IsSelectedByDefault: true, PackageId: { Length: > 0 } packageId } => packageId,
			PaywallPackageComponent package => FindDefaultSelectedPackageIdentifier(package.Stack),
			PaywallStackComponent stack => FindDefaultSelectedPackageIdentifier(stack.Components),
			PaywallButtonComponent button => FindDefaultSelectedPackageIdentifier(button.Stack),
			PaywallHeaderComponent header => FindDefaultSelectedPackageIdentifier(header.Stack),
			PaywallStickyFooterComponent footer => FindDefaultSelectedPackageIdentifier(footer.Stack),
			PaywallCarouselComponent carousel => FindDefaultSelectedPackageIdentifier(carousel.Pages),
			PaywallTabsComponent tabs => FindDefaultSelectedPackageIdentifier(tabs.Tabs),
			PaywallTabComponent tab => FindDefaultSelectedPackageIdentifier(tab.Stack),
			PaywallTabControlButtonComponent tabButton => FindDefaultSelectedPackageIdentifier(tabButton.Stack),
			PaywallUnknownComponent unknown => FindDefaultSelectedPackageIdentifier(unknown.Fallback),
			_ => null
		};
	}

	static string? FindDefaultSelectedPackageIdentifier<TComponent>(IEnumerable<TComponent> components)
		where TComponent : PaywallComponent
	{
		foreach (var component in components)
		{
			var packageId = FindDefaultSelectedPackageIdentifier(component);
			if (!string.IsNullOrWhiteSpace(packageId))
			{
				return packageId;
			}
		}

		return null;
	}

	static void OnRenderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RevenueCatPaywallView view)
		{
			view.Render();
		}
	}

	sealed class EventPaywallActionHandler : IPaywallActionHandler
	{
		readonly RevenueCatPaywallView owner;

		public EventPaywallActionHandler(RevenueCatPaywallView owner)
		{
			this.owner = owner;
		}

		public Task<CustomerInfo?> PurchaseAsync(PaywallPurchaseRequest request, CancellationToken cancellationToken = default)
		{
			owner.OnPurchaseRequested(new PaywallPurchaseRequestedEventArgs(request));
			return Task.FromResult<CustomerInfo?>(null);
		}

		public Task<CustomerInfo?> RestoreAsync(CancellationToken cancellationToken = default)
		{
			owner.OnRestoreRequested(new PaywallRestoreRequestedEventArgs());
			return Task.FromResult<CustomerInfo?>(null);
		}

		public Task DismissAsync(CancellationToken cancellationToken = default)
		{
			owner.OnDismissRequested(new PaywallDismissRequestedEventArgs());
			return Task.CompletedTask;
		}

		public Task NavigateAsync(PaywallNavigationRequest request, CancellationToken cancellationToken = default)
		{
			owner.OnNavigationRequested(new PaywallNavigationRequestedEventArgs(request));
			return Task.CompletedTask;
		}
	}
}
