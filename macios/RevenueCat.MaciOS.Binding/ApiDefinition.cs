using Foundation;
using System;
using System.Threading.Tasks;

namespace RevenueCat
{
	// @interface DotnetNewBinding : NSObject
	[BaseType (typeof(NSObject))]
	interface RevenueCatManager
	{
		// Initialize method
		[Export("initialize:apiKey:userId:")]
		void Initialize(bool debugLog, string apiKey, [NullAllowed] string userId);

		// Login method
		[Export("login:callback:")]
		[Async]
		void Login(string userId, System.Action<NSString?, NSError?> callback);

		// GetCustomerInfo method
		[Export("getCustomerInfo:callback:")]
		[Async]
		void GetCustomerInfo(bool force, System.Action<NSString?, NSError?> callback);

		// SetCustomerInfoChangedHandler method
		[Export("setCustomerInfoChangedHandler:")]
		void SetCustomerInfoChangedHandler(Action<NSString?> callback);
		
		// Restore method
		[Export("restore:")]
		[Async]
		void Restore(System.Action<NSString?, NSError?> callback);
		
		[Export("syncPurchases:")]
		[Async]
		void SyncPurchases(System.Action<NSString?, NSError?> callback);
		
		// Purchase method
		[Export("purchase:packageIdentifier:callback:")]
		[Async]
		void Purchase(NSString offeringIdentifier, NSString packageIdentifier, System.Action<NSString?, NSError?> callback);

		[Export("getOffering:callback:")]
		[Async]
		void GetOffering(NSString offeringIdentifier, System.Action<NSString?, NSError?> callback);
		
		[Export("setEmail:")]
		void SetEmail(NSString email);
		
		[Export("setDisplayName:")]
		void SetDisplayName(NSString displayName);
		
		[Export("setAd:")]
		void SetAd(NSString ad);
		
		[Export("setAdGroup:")]
		void SetAdGroup(NSString adGroup);
		
		[Export("setCampaign:")]
		void SetCampaign(NSString campaign);
		
		[Export("setCreative:")]
		void SetCreative(NSString creative);
		
		[Export("setKeyword:")]
		void SetKeyword(NSString keyword);
		
		[Export("setAttribute:value:")]
		void SetAttribute(NSString key, NSString? value);
		
		[Export("setAttributes:")]
		void SetAttributes(NSDictionary<NSString, NSString> attributes);
		
		[Export("syncOfferingsAndAttributesIfNeeded:")]
		[Async]
		void SyncOfferingsAndAttributesIfNeeded(System.Action<NSError?> callback);
	}
}
