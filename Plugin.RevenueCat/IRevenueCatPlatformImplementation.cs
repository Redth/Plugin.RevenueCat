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