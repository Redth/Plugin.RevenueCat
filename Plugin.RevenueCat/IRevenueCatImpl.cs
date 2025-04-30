using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatImpl
{
	void SetCustomerInfoUpdatedHandler(Action<string> handler);

	void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId);
	Task<string?> LoginAsync(string userId);
	Task<string?> GetCustomerInfoAsync(bool force);

	Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);

	Task<string?> GetOfferingAsync(string offeringIdentifier);

	Task<string?> RestoreAsync();
	
}