#if IOS || MACCATALYST
using Foundation;

namespace Plugin.RevenueCat.UI;

internal sealed class RevenueCatUIApple : IRevenueCatUIPlatformImplementation
{
	private readonly global::RevenueCatUI.RevenueCatUIManager revenueCatUIManager = new();

	public async Task<string?> PresentPaywallAsync(object? platformContext, string? offeringIdentifier, string? requiredEntitlementIdentifier, bool displayCloseButton)
	{
		var result = await revenueCatUIManager.PresentPaywallAsync(
			offeringIdentifier is null ? null : new NSString(offeringIdentifier),
			requiredEntitlementIdentifier is null ? null : new NSString(requiredEntitlementIdentifier),
			displayCloseButton);

		return result?.ToString();
	}

	public async Task<string?> PresentCustomerCenterAsync(object? platformContext)
	{
		var result = await revenueCatUIManager.PresentCustomerCenterAsync();
		return result?.ToString();
	}
}
#endif
