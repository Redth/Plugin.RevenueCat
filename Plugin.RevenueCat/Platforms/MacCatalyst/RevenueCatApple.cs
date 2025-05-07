using Foundation;

namespace Plugin.RevenueCat;

// All the code in this file is only included on iOS.
public class RevenueCatApple : IRevenueCatPlatformImplementation
{
    readonly global::RevenueCat.RevenueCatManager revenueCatManager = new();

    bool initialized = false;
    
    public string? ApiKey { get; private set; }
    
    public void Initialize(RevenueCatOptions options)
    {
	    if (initialized)
	    {
		    return;
	    }
	    initialized = true;

	    revenueCatManager.SetCustomerInfoChangedHandler(nsstr => customerInfoUpdatedHandler?.Invoke(nsstr.ToString()));

#if IOS
	    ApiKey = options.iOSApiKey;
#elif MACCATALYST
	    ApiKey = options.MacCatalystApiKey;
#endif
	    
	    revenueCatManager.Initialize(options.Debug, ApiKey, options.UserId);
    }

    public async Task<string?> LoginAsync(string userId)
    {
        var s = await revenueCatManager.LoginAsync(userId);
        return s.ToString();
    }

    public async Task<string?> RestoreAsync()
    {
        var s = await revenueCatManager.RestoreAsync();
        return s.ToString();
    }

    public async Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
    {
        var s = await revenueCatManager.PurchaseAsync(new NSString(offeringIdentifier), new NSString(packageIdentifier));
        return s.ToString();
    }

    public async Task<string?> GetOfferingAsync(string offeringIdentifier)
    {
        var s = await revenueCatManager.GetOfferingAsync(new NSString(offeringIdentifier));
        return s.ToString();
    }

    public async Task<string?> GetCustomerInfoAsync(bool force)
    {
        var s = await revenueCatManager.GetCustomerInfoAsync(force);
        return s.ToString();
    }

    Action<string>? customerInfoUpdatedHandler = null;

    public void SetCustomerInfoUpdatedHandler(Action<string> handler)
    {
        customerInfoUpdatedHandler = handler; 
    }

}