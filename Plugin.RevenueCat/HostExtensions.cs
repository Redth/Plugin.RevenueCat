using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
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
#if ANDROID
		builder.Services.AddSingleton<IRevenueCatPlatformImplementation, RevenueCatAndroid>();
#elif IOS || MACCATALYST
		builder.Services.AddSingleton<IRevenueCatPlatformImplementation, RevenueCatApple>();
#endif

		builder.Services.AddSingleton<RevenueCatOptions>(options);
		builder.Services.AddSingleton<IRevenueCatManager, RevenueCatManager>();
		
		builder.ConfigureLifecycleEvents(lifecycle =>
		{
#if ANDROID
			lifecycle.AddAndroid(android =>
			{
				android.OnApplicationCreate(app =>
				{
					if (Manager is not null)
					{
						var amazon = RevenueCatAndroid.IsAmazon();

						if (amazon && string.IsNullOrEmpty(options.AmazonApiKey))
							throw new ArgumentException("Amazon API Key is required");
						if (!amazon && string.IsNullOrEmpty(options.AndroidApiKey))
							throw new ArgumentException("Android API Key is required");

						// Initialize the SDK
						Manager.Initialize(options);
					}
				});
			});
#elif MACCATALYST || IOS
			lifecycle.AddiOS(ios =>
			{
				ios.FinishedLaunching((app, launchOptions) =>
				{
					var manager = Application.Current?.Handler?.MauiContext?.Services?.GetService<IRevenueCatManager>();
					
					if (manager is not null)
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
						manager.Initialize();
					}

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
			return Task.FromResult<CustomerInfoRequest?>(null);

		object? platformContext = null;

#if ANDROID
		platformContext = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
#endif

		return manager.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier);
	}
}