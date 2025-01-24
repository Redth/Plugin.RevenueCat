using Foundation;

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
		[Export("login:err:")]
		[Async]
		Task<string> LoginAsync(string userId);

		// GetCustomerInfo method
		[Export("getCustomerInfo:err:")]
		[Async]
		Task<string> GetCustomerInfoAsync(bool force);

		// SetCustomerInfoChangedHandler method
		[Export("setCustomerInfoChangedHandler:")]
		void SetCustomerInfoChangedHandler(Action<string> callback);

		// Restore method
		[Export("restore:")]
		[Async]
		Task<string> RestoreAsync();

		// Purchase method
		[Export("purchase:err:")]
		[Async]
		Task<string> PurchaseAsync(NSObject storeProduct);
	}
}
