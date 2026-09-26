#nullable enable

using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Plugin.RevenueCat.Paywalls;

public static class RevenueCatPaywallsAppBuilderExtensions
{
	public static MauiAppBuilder UseRevenueCatPaywalls(this MauiAppBuilder builder) =>
		builder.UseSkiaSharp();
}
