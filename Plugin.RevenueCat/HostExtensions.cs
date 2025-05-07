using Microsoft.Maui.LifecycleEvents;

using Plugin.RevenueCat;
using Plugin.RevenueCat.Models;


public static class HostExtensions
{
	static IRevenueCatManager? Manager { get; set; }

	public static MauiAppBuilder UseRevenueCat(this MauiAppBuilder builder, Action<RevenueCatOptionsBuilder>? configure = null)
	{
		var optionsBuilder = new RevenueCatOptionsBuilder();
		configure?.Invoke(optionsBuilder);

		var options = optionsBuilder.Build();
		
		return builder.UseRevenueCat(options);
	}
	
	public static MauiAppBuilder UseRevenueCat(this MauiAppBuilder builder, RevenueCatOptions options)
	{
		IRevenueCatImpl? impl = null;

#if ANDROID
		impl = new RevenueCatAndroid();
		builder.Services.AddSingleton<IRevenueCatImpl>(impl);
#elif IOS || MACCATALYST
		impl = new RevenueCatApple();
		builder.Services.AddSingleton<IRevenueCatImpl>(impl);
#endif

		if (impl is not null)
		{
			Manager = new RevenueCatManager(impl);
			builder.Services.AddSingleton<IRevenueCatManager>(Manager);
		}
		else
		{
			throw new PlatformNotSupportedException("Plugin.RevenueCat is not supported on this Platform.");
		}
		
		if (Manager is null)
			throw new InvalidOperationException("Plugin.RevenueCat not initialized or not supported on this platform. Call AddRevenueCat first.");

		builder.ConfigureLifecycleEvents(lifecycle =>
		{
#if ANDROID
			lifecycle.AddAndroid(android =>
			{
				android.OnApplicationCreate(app =>
				{
					if (string.IsNullOrEmpty(options.AndroidApiKey))
						throw new ArgumentException("Android API Key is required");
					
					// Initialize the SDK
					Manager.Initialize(options.AndroidApiKey, options.Debug ?? false, options.AppStore, options.UserId, options.CustomerInfoUpdatedCallback);
				});
			});
#elif MACCATALYST || IOS
			lifecycle.AddiOS(ios =>
			{
				ios.FinishedLaunching((app, launchOptions) =>
				{
					string? apiKey = null;
#if IOS
					apiKey = options.iOSApiKey;
					if (string.IsNullOrEmpty(apiKey))
						throw new ArgumentException("RevenueCat iOS (or Apple) API Key is required");
#elif MACCATALYST
					apiKey = options.MacCatalystApiKey;
					if (string.IsNullOrEmpty(apiKey))
						throw new ArgumentException("RevenueCat MacCatalyst (or Apple) API Key is required");
#endif
					
					// Initialize the SDK
					Manager.Initialize(apiKey, options.Debug ?? false, options.AppStore, options.UserId, options.CustomerInfoUpdatedCallback);
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