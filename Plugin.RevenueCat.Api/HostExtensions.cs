﻿using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api.V1;
using Plugin.RevenueCat.Api.V2;
using System.Net.Http.Headers;

namespace Plugin.RevenueCat.Api;

public static class HostExtensions
{
	public static IServiceCollection AddRevenueCatApiV2(this IServiceCollection services, RevenueCatApiV2Settings settings)
	{
		services.AddHttpClient<IRevenueCatApiV2, RevenueCatApiV2>(client =>
		{
			client.BaseAddress = new Uri("https://api.revenuecat.com/v2/");
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
		});

		return services;
	}

	public static IServiceCollection AddRevenueCatApiV1(this IServiceCollection services, RevenueCatApiV1Settings settings)
	{
		services.AddHttpClient<IRevenueCatApiV1, RevenueCatApiV1>(client =>
		{
			client.BaseAddress = new Uri("https://api.revenuecat.com/v1/");
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
		});

		return services;
	}
}
