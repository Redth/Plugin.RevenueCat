#if ANDROID
using Android.App;
using Java.Interop;
using Java.Util.Concurrent;

namespace Plugin.RevenueCat.UI;

internal sealed class RevenueCatUIAndroid : IRevenueCatUIPlatformImplementation
{
	public async Task<string?> PresentPaywallAsync(object? platformContext, string? offeringIdentifier, string? requiredEntitlementIdentifier, bool displayCloseButton)
	{
		var result = await global::RevenueCatUI.RevenueCatUIManager.PresentPaywall(
			platformContext as Activity,
			offeringIdentifier,
			requiredEntitlementIdentifier,
			displayCloseButton)!.AsTask<Java.Lang.String>().ConfigureAwait(false);

		return result?.ToString();
	}

	public async Task<string?> PresentCustomerCenterAsync(object? platformContext)
	{
		var result = await global::RevenueCatUI.RevenueCatUIManager.PresentCustomerCenter(
			platformContext as Activity)!.AsTask<Java.Lang.String>().ConfigureAwait(false);

		return result?.ToString();
	}
}

internal static class CompletableFutureExtensions
{
	public static Task<TResult> AsTask<TResult>(this CompletableFuture completableFuture) where TResult : Java.Lang.Object
	{
		var listener = new CompletableFutureListener<TResult>();
		completableFuture.WhenComplete(listener);
		return listener.Task;
	}
}

internal sealed class CompletableFutureListener<TResult> : Java.Lang.Object, Java.Util.Functions.IBiConsumer
	where TResult : Java.Lang.Object
{
	private readonly TaskCompletionSource<TResult> taskCompletionSource = new();

	public Task<TResult> Task => taskCompletionSource.Task;

	public void Accept(Java.Lang.Object? result, Java.Lang.Object? error)
	{
		if (error is not null || result is null)
		{
			if (error.TryJavaCast<Java.Lang.Throwable>(out var javaException))
			{
				taskCompletionSource.TrySetException(javaException);
			}
			else
			{
				taskCompletionSource.TrySetException(new Exception(error?.ToString()));
			}

			return;
		}

		if (result.TryJavaCast<TResult>(out var typedResult))
		{
			taskCompletionSource.TrySetResult(typedResult);
			return;
		}

		taskCompletionSource.TrySetException(new InvalidCastException(result.GetType().Name));
	}
}
#endif
