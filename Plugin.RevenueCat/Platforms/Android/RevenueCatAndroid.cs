using Android.App;
using Java.Interop;
using Java.Util.Concurrent;
using Java.Util.Functions;
using RevenueCat;

namespace Plugin.RevenueCat;

// All the code in this file is only included on Android.
public class RevenueCatAndroid : Java.Lang.Object, IRevenueCatPlatformImplementation, ICustomerInfoUpdatedListener
{
	bool initialized = false;

	public string? ApiKey { get; private set; }

	public string? AppUserId => initialized ? global::RevenueCat.RevenueCatManager.AppUserId : null;

	public bool IsAnonymous => initialized && global::RevenueCat.RevenueCatManager.IsAnonymous;
	
	public void Initialize(RevenueCatOptions options)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;

		// Detect the correct app store to use
		var configuredStore = options.AppStore?.Trim().ToLowerInvariant();
		var amazon = configuredStore == "amazon" || (string.IsNullOrEmpty(configuredStore) && IsAmazon());
		var appStore = amazon ? "amazon" : "google";
		if (configuredStore is "test" or "test_store")
			appStore = "test";
		ApiKey = amazon ? options.AmazonApiKey : options.AndroidApiKey;
		
		ValidateProxyUrl(options.ProxyUrl);

		var context = global::Android.App.Application.Context;
		global::RevenueCat.RevenueCatManager.SetCustomerInfoUpdatedListener(this);
		global::RevenueCat.RevenueCatManager.Initialize(
			context,
			options.Debug,
			appStore,
			ApiKey,
			options.UserId,
			options.ProxyUrl,
			ToNativePurchasesAreCompletedBy(options.PurchasesAreCompletedBy),
			ToNativeEntitlementVerificationMode(options.EntitlementVerificationMode),
			ToJavaBoolean(options.DiagnosticsEnabled),
			ToJavaBoolean(options.AutomaticDeviceIdentifierCollectionEnabled),
			ToJavaBoolean(options.PendingTransactionsForPrepaidPlansEnabled));
	}

	public static bool IsAmazon()
	{
		var pkgManager = Android.App.Application.Context.PackageManager;

		string? installerPackageName = null;

		var pkgName = Android.App.Application.Context.PackageName;

		if (string.IsNullOrEmpty(pkgName) || pkgManager is null)
			return false;
		
		if (OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			var installSourceInfo = pkgManager.GetInstallSourceInfo(pkgName);
			installerPackageName = installSourceInfo?.InitiatingPackageName;
		}
		
		if (string.IsNullOrEmpty(installerPackageName))
		{
			// Fallback to the old method, it's deprecated on > 30 but still try if need be
#pragma warning disable CA1422
			installerPackageName = pkgManager.GetInstallerPackageName(pkgName);
#pragma warning restore CA1422
		}
				
		return (installerPackageName?.StartsWith("com.amazon", StringComparison.OrdinalIgnoreCase) ?? false)
			|| (Android.OS.Build.Manufacturer?.StartsWith("amazon", StringComparison.OrdinalIgnoreCase) ?? false);
	}
	
	public async Task<string?> LoginAsync(string userId)
	{
		return await global::RevenueCat.RevenueCatManager.Login(userId)!.AsStringTask();
	}

	public async Task<string?> LogOutAsync()
	{
		return await global::RevenueCat.RevenueCatManager.Logout()!.AsStringTask();
	}

	public async Task<string?> GetCustomerInfoAsync(bool force)
	{
		return await global::RevenueCat.RevenueCatManager.GetCustomerInfo(force)!.AsStringTask();
	}

	public void InvalidateCustomerInfoCache()
		=> global::RevenueCat.RevenueCatManager.InvalidateCustomerInfoCache();

	Action<string>? customerInfoUpdateHandler;

	public async Task<string?> RestoreAsync()
	{
		return await global::RevenueCat.RevenueCatManager.Restore()!.AsStringTask();
	}
	
	public async Task<string?> SyncPurchasesAsync()
	{
		return await global::RevenueCat.RevenueCatManager.SyncPurchases()!.AsStringTask();
	}

	public async Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
	{
		return await global::RevenueCat.RevenueCatManager.Purchase(platformContext as Activity, offeringIdentifier, packageIdentifier)!.AsStringTask();
	}

	public async Task<string?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, string? purchaseOptionsJson)
	{
		return await global::RevenueCat.RevenueCatManager.PurchaseWithResult(platformContext as Activity, offeringIdentifier, packageIdentifier, purchaseOptionsJson)!.AsStringTask();
	}

	public async Task<string?> PurchaseProductAsync(object? platformContext, string productIdentifier, string? type, string? purchaseOptionsJson)
	{
		return await global::RevenueCat.RevenueCatManager.PurchaseProduct(platformContext as Activity, productIdentifier, type, purchaseOptionsJson)!.AsStringTask();
	}

	public async Task<string?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, string? type, string? purchaseOptionsJson)
	{
		return await global::RevenueCat.RevenueCatManager.PurchaseSubscriptionOption(platformContext as Activity, productIdentifier, subscriptionOptionIdentifier, type, purchaseOptionsJson)!.AsStringTask();
	}

	public async Task<string?> GetOfferingAsync(string offeringIdentifier)
	{
		return await global::RevenueCat.RevenueCatManager.GetOffering(offeringIdentifier)!.AsStringTask();
	}

	public async Task<string?> GetOfferingsAsync()
	{
		return await global::RevenueCat.RevenueCatManager.Offerings!.AsStringTask();
	}

	public async Task<string?> GetOfferingForPlacementAsync(string placementIdentifier)
	{
		return await global::RevenueCat.RevenueCatManager.GetOfferingForPlacement(placementIdentifier)!.AsStringTask();
	}

	public async Task<string?> GetProductsAsync(string productIdentifiersCsv, string? type)
	{
		return await global::RevenueCat.RevenueCatManager.GetProducts(productIdentifiersCsv, type)!.AsStringTask();
	}
	
	public async Task SyncOfferingsAndAttributesIfNeeded()
		=> await global::RevenueCat.RevenueCatManager.SyncAttributesAndOfferingsIfNeeded()!.AsBooleanTask().ConfigureAwait(false);

	public async Task<bool> CanMakePaymentsAsync(object? platformContext)
	{
		var context = (platformContext as Activity) ?? global::Android.App.Application.Context;
		return await global::RevenueCat.RevenueCatManager.CanMakePayments(context)!.AsBooleanTask();
	}

	public async Task<string?> GetStorefrontAsync()
	{
		return await global::RevenueCat.RevenueCatManager.Storefront!.AsStringTask();
	}

	public async Task<string?> GetVirtualCurrenciesAsync()
	{
		return await global::RevenueCat.RevenueCatManager.VirtualCurrencies!.AsStringTask();
	}

	public void InvalidateVirtualCurrenciesCache()
		=> global::RevenueCat.RevenueCatManager.InvalidateVirtualCurrenciesCache();

	public async Task<string?> RedeemWebPurchaseAsync(string redemptionLink)
	{
		return await global::RevenueCat.RevenueCatManager.RedeemWebPurchase(redemptionLink)!.AsStringTask();
	}

	public async Task<string?> GetAmazonLwaConsentStatusAsync()
	{
		return await global::RevenueCat.RevenueCatManager.AmazonLwaConsentStatus!.AsStringTask();
	}

	public void CollectDeviceIdentifiers()
		=> global::RevenueCat.RevenueCatManager.CollectDeviceIdentifiers();

	public void SetEmail(string email)
		=> global::RevenueCat.RevenueCatManager.SetEmail(email);

	public void SetPhoneNumber(string? phoneNumber)
		=> global::RevenueCat.RevenueCatManager.SetPhoneNumber(phoneNumber);

	public void SetPushToken(string? pushToken)
		=> global::RevenueCat.RevenueCatManager.SetPushToken(pushToken);
    
	public void SetDisplayName(string displayName)
		=> global::RevenueCat.RevenueCatManager.SetDisplayName(displayName);

	public void SetMediaSource(string? mediaSource)
		=> global::RevenueCat.RevenueCatManager.SetMediaSource(mediaSource);

	public void SetAd(string ad)
		=> global::RevenueCat.RevenueCatManager.SetAd(ad);

	public void SetAdGroup(string adGroup)
		=> global::RevenueCat.RevenueCatManager.SetAdGroup(adGroup);

	public void SetCampaign(string campaign)
		=> global::RevenueCat.RevenueCatManager.SetCampaign(campaign);

	public void SetCreative(string creative)
		=> global::RevenueCat.RevenueCatManager.SetCreative(creative);

	public void SetKeyword(string keyword)
		=> global::RevenueCat.RevenueCatManager.SetKeyword(keyword);

	public void SetAttribute(string key, string? value)
		=> global::RevenueCat.RevenueCatManager.SetAttribute(key, value);

	public void SetAttributes(IDictionary<string, string> attributes)
		=> global::RevenueCat.RevenueCatManager.SetAttributes(attributes);

	public void SetCustomerInfoUpdatedHandler(Action<string> handler)
		=> customerInfoUpdateHandler = handler;

	public void OnCustomerInfoUpdated(string json)
		=> customerInfoUpdateHandler?.Invoke(json);

	static Java.Lang.Boolean? ToJavaBoolean(bool? value)
		=> value.HasValue ? Java.Lang.Boolean.ValueOf(value.Value) : null;

	static string? ToNativePurchasesAreCompletedBy(RevenueCatPurchasesAreCompletedBy? value)
		=> value switch
		{
			RevenueCatPurchasesAreCompletedBy.RevenueCat => "revenuecat",
			RevenueCatPurchasesAreCompletedBy.MyApp => "my_app",
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
}

// Only a fixed set of result types come back from RevenueCatManager's CompletableFutures, so each
// gets a concrete listener rather than a generic one. Calling TryJavaCast<T> with an open generic T
// requires DynamicallyAccessedMembers annotations to be trim/NativeAOT safe (see issue #33).
internal static class CompletableFutureExtensions
{
	public static Task<string?> AsStringTask(this CompletableFuture completableFuture)
	{
		var l = new StringCompletableFutureListener();
		completableFuture.WhenComplete(l);
		return l.Task;
	}

	public static Task<bool> AsBooleanTask(this CompletableFuture completableFuture)
	{
		var l = new BooleanCompletableFutureListener();
		completableFuture.WhenComplete(l);
		return l.Task;
	}

	internal static Exception ToException(Java.Lang.Object? error)
		=> error.TryJavaCast<Java.Lang.Throwable>(out var javaEx)
			? javaEx
			: new Exception(error?.ToString() ?? "CompletableFuture completed without a result.");
}

internal sealed class StringCompletableFutureListener : Java.Lang.Object, IBiConsumer
{
	readonly TaskCompletionSource<string?> _tcs = new();

	public Task<string?> Task => _tcs.Task;

	public void Accept(Java.Lang.Object? t, Java.Lang.Object? error)
	{
		if (error is not null || t is null)
			_tcs.TrySetException(CompletableFutureExtensions.ToException(error));
		else if (t.TryJavaCast<Java.Lang.String>(out var result))
			_tcs.TrySetResult(result.ToString());
		else
			_tcs.TrySetException(new InvalidCastException(t.GetType().Name));
	}
}

internal sealed class BooleanCompletableFutureListener : Java.Lang.Object, IBiConsumer
{
	readonly TaskCompletionSource<bool> _tcs = new();

	public Task<bool> Task => _tcs.Task;

	public void Accept(Java.Lang.Object? t, Java.Lang.Object? error)
	{
		if (error is not null || t is null)
			_tcs.TrySetException(CompletableFutureExtensions.ToException(error));
		else if (t.TryJavaCast<Java.Lang.Boolean>(out var result))
			_tcs.TrySetResult(result.BooleanValue());
		else
			_tcs.TrySetException(new InvalidCastException(t.GetType().Name));
	}
}
