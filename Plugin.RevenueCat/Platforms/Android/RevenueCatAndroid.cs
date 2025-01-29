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
		=> global::RevenueCat.RevenueCatManager.Login(userId)!.AsTask<Java.Lang.String>();

	public Task<string?> GetCustomerInfoAsync(bool force)
		=> global::RevenueCat.RevenueCatManager.GetCustomerInfo(force)!.AsTask<Java.Lang.String>();

	Action<string>? customerInfoUpdateHandler;

	public Task<string?> RestoreAsync()
		=> global::RevenueCat.RevenueCatManager.Restore()!.AsTask<Java.Lang.String>();

	public Task<string?> PurchaseAsync(string offeringIdentifier, string packageIdentifier)
		=> global::RevenueCat.RevenueCatManager.Purchase(offeringIdentifier, packageIdentifier)!.AsTask<Java.Lang.String>();
		
	public Task<string?> GetOfferingAsync(string offeringIdentifier)
		=> global::RevenueCat.RevenueCatManager.GetOffering(offeringIdentifier)!.AsTask<Java.Lang.String>();

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
