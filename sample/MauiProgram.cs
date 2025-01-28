using Microsoft.Extensions.Logging;
using Plugin.RevenueCat;

namespace MauiSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddTransient<MainPage>();

#if ANDROID
		builder.Services.AddSingleton<IRevenueCatImpl, RevenueCatAndroid>();
		builder.Services.AddSingleton<IRevenueCatManager, RevenueCatManager>();
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<IRevenueCatImpl, RevenueCatApple>();
		builder.Services.AddSingleton<IRevenueCatManager, RevenueCatManager>();
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
