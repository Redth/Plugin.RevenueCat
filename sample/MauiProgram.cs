using Microsoft.Extensions.Logging;
using Plugin.RevenueCat;

namespace MauiSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var revenueCatApiKey =
#if ANDROID
			"goog_[YOUR_GOOGLE_CLIENT_KEY]";
#elif IOS || MACCATALYST
			"appl_[YOUR_APPLE_CLIENT_KEY]";
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
			.UseRevenueCat(revenueCatApiKey,
				debugLog: true,
				userId: null,
				appStore: null,
				customerInfoUpdatedCallback: null);

		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
