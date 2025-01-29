using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatManager
{
	event EventHandler<Plugin.RevenueCat.CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId);
	Task<CustomerInfoRequest?> LoginAsync(string userId);
	Task<CustomerInfoRequest?> GetCustomerInfoAsync(bool force);

	Task<Offering?> GetOfferingAsync(string offeringIdentifier);
	//void SetEntitlementsUpdatedHandler(System.Action<string[]> entitlementsHandler);
}

