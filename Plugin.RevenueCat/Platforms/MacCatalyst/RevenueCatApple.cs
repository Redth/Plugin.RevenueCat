
using Foundation;

namespace Plugin.RevenueCat;

// All the code in this file is only included on iOS.
public class RevenueCatApple : IRevenueCatImpl
{
	readonly global::RevenueCat.RevenueCatManager revenueCatManager = new();

    public async Task<string?> GetCustomerInfo(bool force)
    {
        var s = await revenueCatManager.GetCustomerInfoAsync(force);
        return s;
    }

    public void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId)
    {
        revenueCatManager.Initialize(debugLog, apiKey, userId);
    }

    public async Task<string?> Login(string userId)
    {
        var s = await revenueCatManager.LoginAsync(userId);


        
        return s;
    }

    public void SetCustomerInfoUpdatedHandler(Action<string> handler)
    {
        
    }
}