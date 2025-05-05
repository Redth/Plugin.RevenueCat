using Microsoft.Maui.LifecycleEvents;

using Plugin.RevenueCat;
using Plugin.RevenueCat.Models;

public static class HostExtensions
{
	internal static IRevenueCatManager? Manager { get; set; }


	public static IServiceCollection AddRevenueCat(this IServiceCollection services)
	{
		IRevenueCatImpl? impl = null;

#if ANDROID
		impl = new RevenueCatAndroid();
		services.AddSingleton<IRevenueCatImpl>(impl);
#elif IOS || MACCATALYST
		impl = new RevenueCatApple();
		services.AddSingleton<IRevenueCatImpl>(impl);
#endif

		if (impl is not null)
		{
			Manager = new RevenueCatManager(impl);
			services.AddSingleton<IRevenueCatManager>(Manager);
		}
		else
		{
			throw new PlatformNotSupportedException("Plugin.RevenueCat is not supported on this Platform.");
		}

		return services;
	}

	public static MauiAppBuilder UseRevenueCat(this MauiAppBuilder builder, string apiKey, string? userId = null, string? appStore = null, bool debugLog = false, Action<CustomerInfoRequest>? customerInfoUpdatedCallback = null)
	{
		builder.Services.AddRevenueCat();

		if (Manager is null)
			throw new InvalidOperationException("Plugin.RevenueCat not initialized or not supported on this platform. Call AddRevenueCat first.");

		builder.ConfigureLifecycleEvents(lifecycle =>
		{
#if ANDROID
			lifecycle.AddAndroid(android =>
			{
				android.OnApplicationCreate(app =>
				{
					// Initialize the SDK
					Manager.Initialize(apiKey, debugLog, appStore, userId, customerInfoUpdatedCallback);
				});
			});
#elif MACCATALYST || IOS
			lifecycle.AddiOS(ios =>
			{
				ios.FinishedLaunching((app, options) =>
				{
					// Initialize the SDK
					Manager.Initialize(apiKey, debugLog, appStore, userId, customerInfoUpdatedCallback);
					return true;
				});
			});
#endif
		});
		return builder;
	}

	public static Task<CustomerInfoRequest?> PurchaseAsync(this IRevenueCatManager? manager, string offeringIdentifier, string packageIdentifier)
	{
		if (manager is null)
			return Task.FromResult<CustomerInfoRequest?>(default);

		object? platformContext = null;

#if ANDROID
		platformContext = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
#endif

		return manager.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier);
	}
}