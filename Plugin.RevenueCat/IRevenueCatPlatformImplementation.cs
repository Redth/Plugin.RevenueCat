using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatPlatformImplementation
{
	void SetCustomerInfoUpdatedHandler(Action<string> handler);

	string? ApiKey { get; }

	string? AppUserId { get; }

	bool IsAnonymous { get; }
	
	void Initialize(RevenueCatOptions options);

	Task<string?> LoginAsync(string userId);
	Task<string?> LogOutAsync();

	Task<string?> GetCustomerInfoAsync(bool force);
	void InvalidateCustomerInfoCache();

	Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);
	Task<string?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, string? purchaseOptionsJson);
	Task<string?> PurchaseProductAsync(object? platformContext, string productIdentifier, string? type, string? purchaseOptionsJson);
	Task<string?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, string? type, string? purchaseOptionsJson);

	Task<string?> GetOfferingAsync(string offeringIdentifier);
	Task<string?> GetOfferingsAsync();
	Task<string?> GetOfferingForPlacementAsync(string placementIdentifier);
	Task<string?> GetProductsAsync(string productIdentifiersCsv, string? type);

	Task<string?> RestoreAsync();
	
	Task<string?> SyncPurchasesAsync();

	Task<bool> CanMakePaymentsAsync(object? platformContext);
	Task<string?> GetStorefrontAsync();
	Task<string?> GetVirtualCurrenciesAsync();
	void InvalidateVirtualCurrenciesCache();
	Task<string?> RedeemWebPurchaseAsync(string redemptionLink);
	Task<string?> GetAmazonLwaConsentStatusAsync();
	
	Task SyncOfferingsAndAttributesIfNeeded();

	void CollectDeviceIdentifiers();
	
	void SetEmail(string email);

	void SetPhoneNumber(string? phoneNumber);

	void SetPushToken(string? pushToken);
	
	void SetDisplayName(string displayName);

	void SetMediaSource(string? mediaSource);
	
	void SetAd(string ad);
	
	void SetAdGroup(string adGroup);
	
	void SetCampaign(string campaign);
	
	void SetCreative(string creative);
	
	void SetKeyword(string keyword);
	
	void SetAttribute(string key, string? value);

	void SetAttributes(IDictionary<string, string> attributes);
}
