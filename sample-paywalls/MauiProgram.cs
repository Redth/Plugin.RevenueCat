using Microsoft.Extensions.Logging;
using Microsoft.Maui.DevFlow.Agent;
using Plugin.RevenueCat.Paywalls;

namespace PaywallGallerySample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseRevenueCatPaywalls()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.AddMauiDevFlowAgent();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
