using System;

#if ANDROID || MACCATALYST || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
#endif

namespace Plugin.RevenueCat.UI;

public static class HostExtensions
{
#if ANDROID || MACCATALYST || IOS
	public static MauiAppBuilder UseRevenueCatUI(this MauiAppBuilder builder)
	{
#if ANDROID
		builder.Services.TryAddSingleton<IRevenueCatUIPlatformImplementation, RevenueCatUIAndroid>();
#elif IOS || MACCATALYST
		builder.Services.TryAddSingleton<IRevenueCatUIPlatformImplementation, RevenueCatUIApple>();
#endif
		builder.Services.TryAddSingleton<IRevenueCatUIManager, RevenueCatUIManager>();

		return builder;
	}
#endif
}
