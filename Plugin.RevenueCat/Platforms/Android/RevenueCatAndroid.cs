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
	
	public void Initialize(RevenueCatOptions options)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;

		// Detect the correct app store to use
		var amazon = IsAmazon();
		var appStore = amazon ? "amazon" : "google";
		ApiKey = amazon ? options.AmazonApiKey : options.AndroidApiKey;
		
		var context = global::Android.App.Application.Context;
		global::RevenueCat.RevenueCatManager.SetCustomerInfoUpdatedListener(this);
		global::RevenueCat.RevenueCatManager.Initialize(context, options.Debug, appStore, ApiKey, options.UserId);
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
		var s = await global::RevenueCat.RevenueCatManager.Login(userId)!.AsTask<Java.Lang.String>();
		return s?.ToString();
	}

	public async Task<string?> GetCustomerInfoAsync(bool force)
	{
		var s = await global::RevenueCat.RevenueCatManager.GetCustomerInfo(force)!.AsTask<Java.Lang.String>();
		return s?.ToString();
	}

	Action<string>? customerInfoUpdateHandler;

	public async Task<string?> RestoreAsync()
	{
		var s = await global::RevenueCat.RevenueCatManager.Restore()!.AsTask<Java.Lang.String>();
		return s?.ToString();
	}
	
	public async Task<string?> SyncPurchasesAsync()
	{
		var s = await global::RevenueCat.RevenueCatManager.SyncPurchases()!.AsTask<Java.Lang.String>();
		return s?.ToString();
	}

	public async Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
	{
		var s = await global::RevenueCat.RevenueCatManager.Purchase(platformContext as Activity, offeringIdentifier, packageIdentifier)!.AsTask<Java.Lang.String>();
		return s?.ToString();
	}

	public async Task<string?> GetOfferingAsync(string offeringIdentifier)
	{
		var s = await global::RevenueCat.RevenueCatManager.GetOffering(offeringIdentifier)!.AsTask<Java.Lang.String>();
		return s?.ToString();
	}
	
	public async Task SyncOfferingsAndAttributesIfNeeded()
		=> await global::RevenueCat.RevenueCatManager.SyncAttributesAndOfferingsIfNeeded()!.AsTask<Java.Lang.Boolean>().ConfigureAwait(false);

	public void SetEmail(string email)
		=> global::RevenueCat.RevenueCatManager.SetEmail(email);
    
	public void SetDisplayName(string displayName)
		=> global::RevenueCat.RevenueCatManager.SetDisplayName(displayName);

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
}

internal static class CompletableFutureExtensions
{
	public static Task<TResult> AsTask<TResult>(this CompletableFuture completableFuture) where TResult : Java.Lang.Object
	{
		var l = new CompletableFutureListener<TResult>();

		completableFuture.WhenComplete(l);

		return l.Task;
	}
}

internal class CompletableFutureListener<TResult> : Java.Lang.Object, IBiConsumer
	where TResult : Java.Lang.Object
{
	readonly TaskCompletionSource<TResult> _tcs = new();

	public Task<TResult> Task => _tcs.Task;

	public void Accept(Java.Lang.Object? t, Java.Lang.Object? error)
	{
		if (error is not null || t is null)
		{
			if (error.TryJavaCast<Java.Lang.Throwable>(out var javaEx))
			{
				_tcs.TrySetException(javaEx);
			}
			else
			{
				_tcs.TrySetException(new Exception(error.ToString()));
			}
		}
		else
		{
			if (t.TryJavaCast<TResult>(out var result))
				_tcs.TrySetResult(result);
			else
				_tcs.TrySetException(new InvalidCastException(t.GetType().Name));
		}
	}
}
