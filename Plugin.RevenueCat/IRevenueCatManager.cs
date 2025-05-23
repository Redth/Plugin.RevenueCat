using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatManager
{
	event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	string? ApiKey { get; }
	
	void Initialize();

	Task<CustomerInfo?> LoginAsync(string userId);
	Task<CustomerInfo?> GetCustomerInfoAsync(bool force);

	Task<Offering?> GetOfferingAsync(string offeringIdentifier);
	
	Task<CustomerInfo?> RestoreAsync();
	
	Task<CustomerInfo?> SyncPurchasesAsync();

	Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);
}

