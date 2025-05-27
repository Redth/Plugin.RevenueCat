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

	Task SyncOfferingsAndAttributesIfNeeded();
	
	void SetEmail(string email);
	
	void SetDisplayName(string displayName);
	
	void SetAd(string ad);
	
	void SetAdGroup(string adGroup);
	
	void SetCampaign(string campaign);
	
	void SetCreative(string creative);
	
	void SetKeyword(string keyword);
	
	void SetAttribute(string key, string? value);

	void SetAttributes(IDictionary<string, string> attributes);

}

