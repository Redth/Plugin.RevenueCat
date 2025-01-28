using Java.Interop;
using Java.Util.Concurrent;
using Java.Util.Functions;
using RevenueCat;

namespace Plugin.RevenueCat;

// All the code in this file is only included on Android.
public class RevenueCatAndroid : Java.Lang.Object, IRevenueCatImpl, ICustomerInfoUpdatedListener
{
	public async Task<string?> Login(string userId)
	{
		var s = await global::RevenueCat.RevenueCatManager.Login(userId)!.AsTask<Java.Lang.String>();
		return s.ToString();
	}

	public void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId)
	{
		global::RevenueCat.RevenueCatManager.SetCustomerInfoUpdatedListener(this);
		global::RevenueCat.RevenueCatManager.Initialize(platformContext as Android.Content.Context, debugLog, appStore, apiKey, userId);
	}

	public async Task<string?> GetCustomerInfo(bool force)
	{
		var s = await global::RevenueCat.RevenueCatManager.GetCustomerInfo(force)!.AsTask<Java.Lang.String>();
		return s.ToString();
	}

	Action<string>? customerInfoUpdateHandler;

	public void SetCustomerInfoUpdatedHandler(Action<string> handler)
	{
		customerInfoUpdateHandler = handler;
	}

	public void OnCustomerInfoUpdated(string json)
	{
		customerInfoUpdateHandler?.Invoke(json);
	}
}

public static class CompletableFutureExtensions
{
	public static Task<TResult> AsTask<TResult>(this CompletableFuture completableFuture) where TResult : Java.Lang.Object
	{
		var l = new CompletableFutureListener<TResult>();

		completableFuture.WhenComplete(l);

		return l.Task;
	}
}

public class CompletableFutureListener<TResult> : Java.Lang.Object, IBiConsumer
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
