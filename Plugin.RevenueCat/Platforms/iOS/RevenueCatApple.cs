
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Foundation;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

// All the code in this file is only included on iOS.
public class RevenueCatApple : IRevenueCatPlatformImplementation
{
	readonly global::RevenueCat.RevenueCatManager revenueCatManager = new();

    bool initialized = false;

    public string? ApiKey { get; private set; }

    public string? AppUserId => initialized ? revenueCatManager.AppUserID()?.ToString() : null;

    public bool IsAnonymous => initialized && revenueCatManager.IsAnonymous();
    
    public void Initialize(RevenueCatOptions options)
    {
        if (initialized)
        {
            return;
        }
        initialized = true;

        revenueCatManager.SetCustomerInfoChangedHandler(nsstr => customerInfoUpdatedHandler?.Invoke(nsstr.ToString()));

        #if IOS
	    ApiKey = options.iOSApiKey;
        #elif MACCATALYST
	    ApiKey = options.MacCatalystApiKey;
		#endif

        if (string.IsNullOrEmpty(ApiKey))
            throw new InvalidOperationException("A RevenueCat iOS API key is required.");
	    
        ValidateProxyUrl(options.ProxyUrl);

        revenueCatManager.Initialize(
            options.Debug,
            ApiKey,
            options.UserId,
            options.ProxyUrl,
            ToNativePurchasesAreCompletedBy(options.PurchasesAreCompletedBy),
            ToNativeStoreKitVersion(options.StoreKitVersion),
            ToNativeEntitlementVerificationMode(options.EntitlementVerificationMode),
            ToNSNumber(options.DiagnosticsEnabled),
            ToNSNumber(options.AutomaticDeviceIdentifierCollectionEnabled));
    }

    public Task<string?> LoginAsync(string userId)
        => RunStringAsync(() => revenueCatManager.LoginAsync(userId));

    public Task<string?> LogOutAsync()
        => RunStringAsync(revenueCatManager.LogOutAsync);

    public Task<string?> RestoreAsync()
        => RunStringAsync(revenueCatManager.RestoreAsync);
    
    public Task<string?> SyncPurchasesAsync()
	    => RunStringAsync(revenueCatManager.SyncPurchasesAsync);

    public Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
        => RunStringAsync(() => revenueCatManager.PurchaseAsync(new NSString(offeringIdentifier), new NSString(packageIdentifier)));

    public Task<string?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, string? purchaseOptionsJson)
        => RunStringAsync(() => revenueCatManager.PurchaseWithResultAsync(new NSString(offeringIdentifier), new NSString(packageIdentifier), ToNSString(purchaseOptionsJson)));

    public Task<string?> PurchaseProductAsync(object? platformContext, string productIdentifier, string? type, string? purchaseOptionsJson)
        => RunStringAsync(() => revenueCatManager.PurchaseProductAsync(new NSString(productIdentifier), ToNSString(type), ToNSString(purchaseOptionsJson)));

    public Task<string?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, string? type, string? purchaseOptionsJson)
        => RunStringAsync(() => revenueCatManager.PurchaseProductAsync(new NSString(productIdentifier), ToNSString(type), ToNSString(purchaseOptionsJson)));

    public Task<string?> GetOfferingAsync(string offeringIdentifier)
        => RunStringAsync(() => revenueCatManager.GetOfferingAsync(new NSString(offeringIdentifier)));

    public Task<string?> GetOfferingsAsync()
        => RunStringAsync(revenueCatManager.GetOfferingsAsync);

    public Task<string?> GetOfferingForPlacementAsync(string placementIdentifier)
        => RunStringAsync(() => revenueCatManager.GetOfferingForPlacementAsync(new NSString(placementIdentifier)));

    public Task<string?> GetProductsAsync(string productIdentifiersCsv, string? type)
        => RunStringAsync(() => revenueCatManager.GetProductsAsync(new NSString(productIdentifiersCsv), ToNSString(type)));

    public Task<string?> GetCustomerInfoAsync(bool force)
        => RunStringAsync(() => revenueCatManager.GetCustomerInfoAsync(force));

    public void InvalidateCustomerInfoCache()
        => revenueCatManager.InvalidateCustomerInfoCache();

    Action<string>? customerInfoUpdatedHandler = null;

    public void SetCustomerInfoUpdatedHandler(Action<string> handler)
    {
        customerInfoUpdatedHandler = handler; 
    }

    public async Task SyncOfferingsAndAttributesIfNeeded()
    {
        try
        {
            await revenueCatManager.SyncOfferingsAndAttributesIfNeededAsync().ConfigureAwait(false);
        }
        catch (NSErrorException ex)
        {
            throw ToRevenueCatException(ex);
        }
    }

    public Task<bool> CanMakePaymentsAsync(object? platformContext)
        => Task.FromResult(revenueCatManager.CanMakePayments());

    public Task<string?> GetStorefrontAsync()
        => RunStringAsync(revenueCatManager.GetStorefrontAsync);

    public Task<string?> GetVirtualCurrenciesAsync()
        => RunStringAsync(revenueCatManager.GetVirtualCurrenciesAsync);

    public void InvalidateVirtualCurrenciesCache()
        => revenueCatManager.InvalidateVirtualCurrenciesCache();

    public Task<string?> RedeemWebPurchaseAsync(string redemptionLink)
        => RunStringAsync(() => revenueCatManager.RedeemWebPurchaseAsync(new NSString(redemptionLink)));

    public Task<string?> GetAmazonLwaConsentStatusAsync()
        => throw new PlatformNotSupportedException("Amazon LWA consent status is only available on Android.");

    public void CollectDeviceIdentifiers()
        => revenueCatManager.CollectDeviceIdentifiers();

    public void SetEmail(string email)
	    => revenueCatManager.SetEmail(new NSString(email));

    public void SetPhoneNumber(string? phoneNumber)
	    => revenueCatManager.SetPhoneNumber(ToNSString(phoneNumber));

    public void SetPushToken(string? pushToken)
	    => revenueCatManager.SetPushToken(ToNSString(pushToken));
    
    public void SetDisplayName(string displayName)
	    => revenueCatManager.SetDisplayName(new NSString(displayName));

    public void SetMediaSource(string? mediaSource)
	    => revenueCatManager.SetMediaSource(ToNSString(mediaSource));

    public void SetAd(string ad)
	    => revenueCatManager.SetAd(new NSString(ad));

    public void SetAdGroup(string adGroup)
	    => revenueCatManager.SetAdGroup(new NSString(adGroup));

    public void SetCampaign(string campaign)
	    => revenueCatManager.SetCampaign(new NSString(campaign));

    public void SetCreative(string creative)
	    => revenueCatManager.SetCreative(new NSString(creative));

    public void SetKeyword(string keyword)
	    => revenueCatManager.SetKeyword(new NSString(keyword));

    public void SetAttribute(string key, string? value)
	    => revenueCatManager.SetAttribute(new NSString(key), ToNSString(value));

    public void SetAttributes(IDictionary<string, string> attributes)
    {
	    var md = new NSMutableDictionary<NSString, NSString>();

	    foreach (var kvp in attributes)
	    {
		    var key = new NSString(kvp.Key);
		    var value = new NSString(kvp.Value);
		    md.SetValueForKey(value, key);
	    }
	    
	    var d = new NSDictionary<NSString, NSString>(md.Keys, md.Values); 
	    revenueCatManager.SetAttributes(d);
    }

    static NSString? ToNSString(string? value)
	    => value is null ? null : new NSString(value);

    static NSNumber? ToNSNumber(bool? value)
        => value.HasValue ? NSNumber.FromBoolean(value.Value) : null;

    static string? ToNativePurchasesAreCompletedBy(RevenueCatPurchasesAreCompletedBy? value)
        => value switch
        {
            RevenueCatPurchasesAreCompletedBy.RevenueCat => "revenuecat",
            RevenueCatPurchasesAreCompletedBy.MyApp => "my_app",
            _ => null
        };

    static string? ToNativeStoreKitVersion(RevenueCatStoreKitVersion? value)
        => value switch
        {
            RevenueCatStoreKitVersion.StoreKit1 => "storekit1",
            RevenueCatStoreKitVersion.StoreKit2 => "storekit2",
            _ => null
        };

    static string? ToNativeEntitlementVerificationMode(RevenueCatEntitlementVerificationMode? value)
        => value switch
        {
            RevenueCatEntitlementVerificationMode.Disabled => "disabled",
            RevenueCatEntitlementVerificationMode.Informational => "informational",
            _ => null
        };

    static void ValidateProxyUrl(string? proxyUrl)
    {
        if (!string.IsNullOrWhiteSpace(proxyUrl) &&
            !Uri.TryCreate(proxyUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("RevenueCat proxy URL must be an absolute URL.");
        }
    }

    static async Task<string?> RunStringAsync(Func<Task<NSString>> operation)
    {
        try
        {
            var value = await operation().ConfigureAwait(false);
            return value?.ToString();
        }
        catch (NSErrorException ex)
        {
            throw ToRevenueCatException(ex);
        }
    }

    static Exception ToRevenueCatException(NSErrorException exception)
    {
        var error = exception.Error;
        var revenueCatError = new RevenueCatError
        {
            Code = error?.Code.ToString(),
            Message = error?.LocalizedDescription ?? exception.Message,
            Domain = error?.Domain,
            Source = "ios",
            ExceptionType = exception.GetType().FullName
        };

        var json = JsonSerializer.Serialize(revenueCatError, ModelSerializerContext.Default.RevenueCatError);
        return new Exception(RevenueCatError.SerializedExceptionPrefix + json, exception);
    }
}
