#nullable enable

using Microsoft.Maui.Hosting;
using Plugin.RevenueCat.Paywalls.Rendering.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Plugin.RevenueCat.Paywalls;

public static class RevenueCatPaywallsAppBuilderExtensions
{
	public static MauiAppBuilder UseRevenueCatPaywalls(this MauiAppBuilder builder)
	{
		builder.UseSkiaSharp();
#if ANDROID
		builder.ConfigureMauiHandlers(handlers =>
			handlers.AddHandler<PaywallAccessibleButton, PaywallAccessibleButtonHandler>());
#endif
		return builder;
	}
}
