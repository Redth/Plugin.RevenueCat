using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api.V1;
using Plugin.RevenueCat.Api.V2;
using Refit;

namespace Plugin.RevenueCat.Api;

public static class HostExtensions
{
	public static IServiceCollection AddRevenueCatApiV2(this IServiceCollection services, RevenueCatApiV2Settings settings)
	{
		var refitSettings = new RefitSettings
		{
			AuthorizationHeaderValueGetter = (rq, ct) =>
				Task.FromResult(settings.ApiKey),
			ContentSerializer = new SystemTextJsonContentSerializer(V2.JsonUtil.Settings)
		};

		services.AddRefitClient<IRevenueCatApiV2>(refitSettings)
			.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.revenuecat.com/v2"));

		return services;
	}

	public static IServiceCollection AddRevenueCatApiV1(this IServiceCollection services, RevenueCatApiV1Settings settings)
	{
		var refitSettings = new RefitSettings
		{
			AuthorizationHeaderValueGetter = (rq, ct) =>
				Task.FromResult(settings.ApiKey),
			ContentSerializer = new SystemTextJsonContentSerializer(V1.JsonUtil.Settings)
		};

		services.AddRefitClient<IRevenueCatApiV1>(refitSettings)
			.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.revenuecat.com/v1"));

		return services;
	}
}
