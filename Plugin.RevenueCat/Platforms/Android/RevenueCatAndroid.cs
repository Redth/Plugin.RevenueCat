using Android.App;
using Java.Interop;
using Java.Util.Concurrent;
using Java.Util.Functions;
using RevenueCat;

namespace Plugin.RevenueCat;

// All the code in this file is only included on Android.
public class RevenueCatAndroid : Java.Lang.Object, IRevenueCatImpl, ICustomerInfoUpdatedListener
{
	public void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId)
	{
		global::RevenueCat.RevenueCatManager.SetCustomerInfoUpdatedListener(this);
		global::RevenueCat.RevenueCatManager.Initialize(platformContext as Android.Content.Context, debugLog, appStore, apiKey, userId);
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
