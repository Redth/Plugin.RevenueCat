#nullable enable

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
		[Export("initialize:apiKey:userId:proxyURL:purchasesAreCompletedBy:storeKitVersion:entitlementVerificationMode:diagnosticsEnabled:automaticDeviceIdentifierCollectionEnabled:")]
		void Initialize(bool debugLog, string apiKey, [NullAllowed] string userId, [NullAllowed] string proxyURL, [NullAllowed] string purchasesAreCompletedBy, [NullAllowed] string storeKitVersion, [NullAllowed] string entitlementVerificationMode, [NullAllowed] NSNumber diagnosticsEnabled, [NullAllowed] NSNumber automaticDeviceIdentifierCollectionEnabled);

		// Login method
		[Export("login:callback:")]
		[Async]
		void Login(string userId, System.Action<NSString?, NSError?> callback);

		[Export("logOut:")]
		[Async]
		void LogOut(System.Action<NSString?, NSError?> callback);

		[Export("appUserID")]
		NSString AppUserID();

		[Export("isAnonymous")]
		bool IsAnonymous();

		// GetCustomerInfo method
		[Export("getCustomerInfo:callback:")]
		[Async]
		void GetCustomerInfo(bool force, System.Action<NSString?, NSError?> callback);

		[Export("invalidateCustomerInfoCache")]
		void InvalidateCustomerInfoCache();

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

		[Export("purchaseWithResult:packageIdentifier:purchaseOptionsJson:callback:")]
		[Async]
		void PurchaseWithResult(NSString offeringIdentifier, NSString packageIdentifier, [NullAllowed] NSString purchaseOptionsJson, System.Action<NSString?, NSError?> callback);

		[Export("purchaseProduct:productType:purchaseOptionsJson:callback:")]
		[Async]
		void PurchaseProduct(NSString productIdentifier, [NullAllowed] NSString productType, [NullAllowed] NSString purchaseOptionsJson, System.Action<NSString?, NSError?> callback);

		[Export("getOffering:callback:")]
		[Async]
		void GetOffering(NSString offeringIdentifier, System.Action<NSString?, NSError?> callback);

		[Export("getOfferings:")]
		[Async]
		void GetOfferings(System.Action<NSString?, NSError?> callback);

		[Export("getOfferingForPlacement:callback:")]
		[Async]
		void GetOfferingForPlacement(NSString placementIdentifier, System.Action<NSString?, NSError?> callback);

		[Export("getProducts:productType:callback:")]
		[Async]
		void GetProducts(NSString productIdentifiersCsv, [NullAllowed] NSString productType, System.Action<NSString?, NSError?> callback);
		
		[Export("setEmail:")]
		void SetEmail(NSString email);

		[Export("setPhoneNumber:")]
		void SetPhoneNumber([NullAllowed] NSString phoneNumber);

		[Export("setPushToken:")]
		void SetPushToken([NullAllowed] NSString pushToken);
		
		[Export("setDisplayName:")]
		void SetDisplayName(NSString displayName);

		[Export("setMediaSource:")]
		void SetMediaSource([NullAllowed] NSString mediaSource);
		
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

		[Export("collectDeviceIdentifiers")]
		void CollectDeviceIdentifiers();

		[Export("canMakePayments")]
		bool CanMakePayments();

		[Export("getStorefront:")]
		[Async]
		void GetStorefront(System.Action<NSString?, NSError?> callback);

		[Export("getVirtualCurrencies:")]
		[Async]
		void GetVirtualCurrencies(System.Action<NSString?, NSError?> callback);

		[Export("invalidateVirtualCurrenciesCache")]
		void InvalidateVirtualCurrenciesCache();

		[Export("redeemWebPurchase:callback:")]
		[Async]
		void RedeemWebPurchase(NSString redemptionLink, System.Action<NSString?, NSError?> callback);
	}
}
