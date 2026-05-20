using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public interface IRevenueCatManager
{
	event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	string? ApiKey { get; }

	string? AppUserId { get; }

	bool IsAnonymous { get; }
	
	void Initialize();

	Task<CustomerInfo?> LoginAsync(string userId);
	Task<RevenueCatOperationResult<CustomerInfo>> LoginWithResultAsync(string userId);
	Task<CustomerInfo?> LogOutAsync();
	Task<RevenueCatOperationResult<CustomerInfo>> LogOutWithResultAsync();

	Task<CustomerInfo?> GetCustomerInfoAsync(bool force);
	Task<RevenueCatOperationResult<CustomerInfo>> GetCustomerInfoWithResultAsync(bool force);
	void InvalidateCustomerInfoCache();

	Task<Offering?> GetOfferingAsync(string offeringIdentifier);
	Task<RevenueCatOperationResult<Offering>> GetOfferingWithResultAsync(string offeringIdentifier);
	Task<Offerings?> GetOfferingsAsync();
	Task<RevenueCatOperationResult<Offerings>> GetOfferingsWithResultAsync();
	Task<Offering?> GetCurrentOfferingAsync();
	Task<RevenueCatOperationResult<Offering>> GetCurrentOfferingWithResultAsync();
	Task<Offering?> GetOfferingForPlacementAsync(string placementIdentifier);
	Task<RevenueCatOperationResult<Offering>> GetOfferingForPlacementWithResultAsync(string placementIdentifier);
	Task<IReadOnlyList<StoreProduct>> GetProductsAsync(IEnumerable<string> productIdentifiers, RevenueCatProductType? type = null);
	Task<RevenueCatOperationResult<IReadOnlyList<StoreProduct>>> GetProductsWithResultAsync(IEnumerable<string> productIdentifiers, RevenueCatProductType? type = null);
	
	Task<CustomerInfo?> RestoreAsync();
	Task<RevenueCatOperationResult<CustomerInfo>> RestoreWithResultAsync();
	
	Task<CustomerInfo?> SyncPurchasesAsync();
	Task<RevenueCatOperationResult<CustomerInfo>> SyncPurchasesWithResultAsync();

	Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);
	Task<PurchaseResult?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);
	Task<PurchaseResult?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, PurchaseOptions? options);
	Task<RevenueCatOperationResult<PurchaseResult>> PurchaseWithOperationResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier);
	Task<RevenueCatOperationResult<PurchaseResult>> PurchaseWithOperationResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, PurchaseOptions? options);
	Task<PurchaseResult?> PurchaseProductAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type = null);
	Task<PurchaseResult?> PurchaseProductAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type, PurchaseOptions? options);
	Task<RevenueCatOperationResult<PurchaseResult>> PurchaseProductWithOperationResultAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type = null);
	Task<RevenueCatOperationResult<PurchaseResult>> PurchaseProductWithOperationResultAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type, PurchaseOptions? options);
	Task<PurchaseResult?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, RevenueCatProductType? type = null, PurchaseOptions? options = null);
	Task<RevenueCatOperationResult<PurchaseResult>> PurchaseSubscriptionOptionWithOperationResultAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, RevenueCatProductType? type = null, PurchaseOptions? options = null);

	Task<bool> CanMakePaymentsAsync(object? platformContext = null);
	Task<RevenueCatOperationResult<bool>> CanMakePaymentsWithResultAsync(object? platformContext = null);
	Task<string?> GetStorefrontAsync();
	Task<RevenueCatOperationResult<string>> GetStorefrontWithResultAsync();
	Task<VirtualCurrencies?> GetVirtualCurrenciesAsync();
	Task<RevenueCatOperationResult<VirtualCurrencies>> GetVirtualCurrenciesWithResultAsync();
	void InvalidateVirtualCurrenciesCache();
	Task<WebPurchaseRedemptionResult?> RedeemWebPurchaseAsync(string redemptionLink);
	Task<RevenueCatOperationResult<WebPurchaseRedemptionResult>> RedeemWebPurchaseWithResultAsync(string redemptionLink);
	Task<string?> GetAmazonLwaConsentStatusAsync();
	Task<RevenueCatOperationResult<string>> GetAmazonLwaConsentStatusWithResultAsync();

	Task SyncOfferingsAndAttributesIfNeeded();
	Task<RevenueCatOperationResult<bool>> SyncOfferingsAndAttributesIfNeededWithResult();

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
