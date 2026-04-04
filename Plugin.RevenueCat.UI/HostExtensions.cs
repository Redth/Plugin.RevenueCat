using System;

#if ANDROID || MACCATALYST || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
using Plugin.RevenueCat;
#endif

namespace Plugin.RevenueCat.UI;

public static class HostExtensions
{
#if ANDROID || MACCATALYST || IOS
	public static MauiAppBuilder UseRevenueCatUI(this MauiAppBuilder builder)
	{
#if ANDROID
		builder.Services.TryAddSingleton<IRevenueCatPlatformImplementation, RevenueCatAndroid>();
#elif IOS || MACCATALYST
		builder.Services.TryAddSingleton<IRevenueCatPlatformImplementation, RevenueCatApple>();
#endif
		builder.Services.TryAddSingleton<IRevenueCatUIManager, RevenueCatUIManager>();

		return builder;
	}
#endif
}
