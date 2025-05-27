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

    public async Task<string?> SyncPurchasesAsync()
    {
	    var s = await revenueCatManager.SyncPurchasesAsync();
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

    public Task SyncOfferingsAndAttributesIfNeeded()
	    => revenueCatManager.SyncOfferingsAndAttributesIfNeeded();

    public void SetEmail(string email)
	    => revenueCatManager.SetEmail(email);
    
    public void SetDisplayName(string displayName)
	    => revenueCatManager.SetDisplayName(displayName);

    public void SetAd(string ad)
	    => revenueCatManager.SetAd(ad);

    public void SetAdGroup(string adGroup)
	    => revenueCatManager.SetAdGroup(adGroup);

    public void SetCampaign(string campaign)
	    => revenueCatManager.SetCampaign(campaign);

    public void SetCreative(string creative)
	    => revenueCatManager.SetCreative(creative);

    public void SetKeyword(string keyword)
	    => revenueCatManager.SetKeyword(keyword);

    public void SetAttribute(string key, string? value)
	    => revenueCatManager.SetAttribute(key, value);

    public void SetAttributes(IDictionary<string, string> attributes)
	    => revenueCatManager.SetAttributes(attributes);
}