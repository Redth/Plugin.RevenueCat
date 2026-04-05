#nullable enable

using Foundation;

namespace RevenueCatUI
{
	[BaseType(typeof(NSObject))]
	interface RevenueCatUIManager
	{
		[Export("presentPaywall:requiredEntitlementIdentifier:displayCloseButton:callback:")]
		[Async]
		void PresentPaywall([NullAllowed] NSString offeringIdentifier, [NullAllowed] NSString requiredEntitlementIdentifier, bool displayCloseButton, System.Action<NSString?, NSError?> callback);

		[Export("presentCustomerCenter:")]
		[Async]
		void PresentCustomerCenter(System.Action<NSString?, NSError?> callback);
	}
}
