namespace Plugin.RevenueCat.UI;

public interface IRevenueCatUIManager
{
	Task<PaywallPresentationResult?> PresentPaywallAsync(PaywallPresentationOptions? options = null);

	Task<PaywallPresentationResult?> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallPresentationOptions? options = null);

	Task<CustomerCenterResult?> PresentCustomerCenterAsync();
}
