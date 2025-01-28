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
		void Login(string userId, System.Action<NSData?, NSError?> callback);

		// GetCustomerInfo method
		[Export("getCustomerInfo:callback:")]
		[Async]
		void GetCustomerInfo(bool force, System.Action<NSString?, NSError?> callback);

		// // SetCustomerInfoChangedHandler method
		// [Export("setCustomerInfoChangedHandler:")]
		// void SetCustomerInfoChangedHandler(Action<string> callback);
		//
		// // Restore method
		// [Export("restore:")]
		// [Async]
		// Task<string> RestoreAsync();
		//
		// // Purchase method
		// [Export("purchase:err:")]
		// [Async]
		// Task<string> PurchaseAsync(NSObject storeProduct);
	}
}
