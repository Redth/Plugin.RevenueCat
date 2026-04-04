using Microsoft.Extensions.Logging;
using Plugin.RevenueCat;
using Plugin.RevenueCat.UI;

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
			})
			.UseRevenueCat(o => o
				.WithAndroidApiKey("goog_[YOUR_GOOGLE_CLIENT_KEY]")
				.WithAppleApiKey("appl_[YOUR_APPLE_CLIENT_KEY]")
				.WithDebug(true))
			.UseRevenueCatUI();

		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
