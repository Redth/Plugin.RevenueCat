using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatManager
{
	event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	string ApiKey { get; }
	
	void Initialize(string apiKey, bool debugLog = false, string? appStore = null, string? userId = null, Action<CustomerInfoRequest>? customerInfoUpdatedCallback = null);

	Task<CustomerInfoRequest?> LoginAsync(string userId);
	Task<CustomerInfoRequest?> GetCustomerInfoAsync(bool force);

	Task<Offering?> GetOfferingAsync(string offeringIdentifier);
	
	Task<CustomerInfoRequest?> RestoreAsync();

	Task<CustomerInfoRequest?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);
}

