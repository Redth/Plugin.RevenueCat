namespace Plugin.RevenueCat.UI;

internal interface IRevenueCatUIPlatformImplementation
{
	Task<string?> PresentPaywallAsync(object? platformContext, string? offeringIdentifier, string? requiredEntitlementIdentifier, bool displayCloseButton);

	Task<string?> PresentCustomerCenterAsync(object? platformContext);
}
