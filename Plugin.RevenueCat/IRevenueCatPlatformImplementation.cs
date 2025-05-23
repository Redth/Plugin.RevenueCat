using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatPlatformImplementation
{
	void SetCustomerInfoUpdatedHandler(Action<string> handler);

	string? ApiKey { get; }
	
	void Initialize(RevenueCatOptions options);

	Task<string?> LoginAsync(string userId);

	Task<string?> GetCustomerInfoAsync(bool force);

	Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);

	Task<string?> GetOfferingAsync(string offeringIdentifier);

	Task<string?> RestoreAsync();
	
	Task<string?> SyncPurchasesAsync();
}