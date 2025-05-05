using Microsoft.Extensions.Logging;
using Plugin.RevenueCat;

namespace MauiSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var revenueCatApiKey =
#if ANDROID
			"goog_kPxQRsYSTBgsBRMlucULuPZIYkW";
#elif IOS || MACCATALYST
			"appl_MhvNaMequbUAzjayKBwJSMVKGqE";
#else
			"";
#endif
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseRevenueCat(revenueCatApiKey);

		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
