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
	bool updatingOffering;

	public RevenueCatPaywallView()
		: this(new DefaultPaywallRenderer())
	{
	}

	public RevenueCatPaywallView(IPaywallRenderer renderer)
	{
		ArgumentNullException.ThrowIfNull(renderer);
		this.renderer = renderer;
		eventActionHandler = new EventPaywallActionHandler(this);
	}

	public event EventHandler<PaywallPurchaseRequestedEventArgs>? PurchaseRequested;

	public event EventHandler<PaywallRestoreRequestedEventArgs>? RestoreRequested;

	public event EventHandler<PaywallDismissRequestedEventArgs>? DismissRequested;

	public event EventHandler<PaywallNavigationRequestedEventArgs>? NavigationRequested;

	public event EventHandler<PaywallActionFailedEventArgs>? ActionFailed;

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
		propertyChanged: OnPaywallDataChanged);

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
		default(object),
		propertyChanged: OnRenderPropertyChanged);

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
		selectedPackageIdentifier = new PaywallSelectionState(
			PaywallData?.ComponentsConfig, Packages, selectedPackageIdentifier).SelectedIdentifier;
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
			ActionFailed = error => ActionFailed?.Invoke(this, new PaywallActionFailedEventArgs(error)),
			PackageSelected = packageIdentifier =>
			{
				if (string.Equals(selectedPackageIdentifier, packageIdentifier, StringComparison.Ordinal))
				{
					return;
				}

				selectedPackageIdentifier = packageIdentifier;
				if (!renderer.HandlesPackageSelectionUpdates)
				{
					Render();
				}
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
		if (bindable is RevenueCatPaywallView view)
		{
			var response = newValue as PaywallOfferingsResponse;
			view.updatingOffering = true;
			try
			{
				view.UiConfig = response?.UiConfig;
				view.PaywallOffering = response?.CurrentOffering ?? response?.Offerings.FirstOrDefault();
			}
			finally
			{
				view.updatingOffering = false;
			}
			view.Render();
		}
	}

	static void OnPaywallOfferingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RevenueCatPaywallView view)
		{
			var offering = newValue as PaywallOffering;
			var wasUpdating = view.updatingOffering;
			view.updatingOffering = true;
			try
			{
				view.selectedPackageIdentifier = null;
				view.OfferingIdentifier = offering?.Identifier;
				view.PaywallData = offering?.PaywallComponents;
			}
			finally
			{
				view.updatingOffering = wasUpdating;
			}
			if (!wasUpdating)
			{
				view.Render();
			}
		}
	}

	static void OnPaywallDataChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var view = (RevenueCatPaywallView)bindable;
		view.selectedPackageIdentifier = null;
		OnRenderPropertyChanged(bindable, oldValue, newValue);
	}

	static void OnRenderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is RevenueCatPaywallView { updatingOffering: false } view)
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
