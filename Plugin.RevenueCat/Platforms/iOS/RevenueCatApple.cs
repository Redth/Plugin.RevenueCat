
using Foundation;

namespace Plugin.RevenueCat;

// All the code in this file is only included on iOS.
public class RevenueCatApple : IRevenueCatImpl
{
	readonly global::RevenueCat.RevenueCatManager revenueCatManager = new();

    public void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId)
    {
        revenueCatManager.SetCustomerInfoChangedHandler(nsstr => customerInfoUpdatedHandler?.Invoke(nsstr.ToString()));
        revenueCatManager.Initialize(debugLog, apiKey, userId);
    }

    public Task<string?> LoginAsync(string userId)
        => revenueCatManager.LoginAsync(userId);

    public Task<string?> RestoreAsync()
        => revenueCatManager.RestoreAsync();

    public Task<string?> PurchaseAsync(string offeringIdentifier, string packageIdentifier)
	    => revenueCatManager.PurchaseAsync(new NSString(offeringIdentifier), new NSString(packageIdentifier));
		
	public async Task<string?> GetOfferingAsync(string offeringIdentifier)
	    => revenueCatManager.GetOfferingAsync(new NSString(offeringIdentifier));

    public Task<string?> GetCustomerInfoAsync(bool force)
        => revenueCatManager.GetCustomerInfoAsync(force);

    Action<string>? customerInfoUpdatedHandler = null;

    public void SetCustomerInfoUpdatedHandler(Action<string> handler)
    {
        customerInfoUpdatedHandler = handler; 
    }

}