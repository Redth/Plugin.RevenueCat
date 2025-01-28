using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatImpl
{
	void SetCustomerInfoUpdatedHandler(Action<string> handler);

	void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId);
	Task<string?> Login(string userId);
	Task<string?> GetCustomerInfo(bool force);
	
}