using Plugin.RevenueCat.Api.V1;
using Plugin.RevenueCat.Models;
using System.Net.Http.Json;

namespace Plugin.RevenueCat.Api;

public class RevenueCatApiV1 : IRevenueCatApiV1
{
	private readonly HttpClient _httpClient;

	public RevenueCatApiV1(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<SubscriberResponse> GetOrCreateCustomer(string customer_id)
	{
		var response = await _httpClient.GetAsync($"subscribers/{customer_id}");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync(ApiV1SerializerContext.Default.SubscriberResponse);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task<OfferingsResponse> GetOfferings(string customer_id)
	{
		var response = await _httpClient.GetAsync($"subscribers/{customer_id}/offerings");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync(ApiV1SerializerContext.Default.OfferingsResponse);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task<PaywallOfferingsResponse> GetPaywallOfferings(string customer_id, PaywallOfferingsRequest? request = null)
	{
		using var message = new HttpRequestMessage(HttpMethod.Get, $"subscribers/{Uri.EscapeDataString(customer_id)}/offerings");
		AddOptionalHeader(message, "X-Platform", request?.Platform);
		AddOptionalHeader(message, "X-Platform-Version", request?.PlatformVersion);
		AddOptionalHeader(message, "X-Client-Version", request?.ClientVersion);
		AddOptionalHeader(message, "X-Client-Build-Version", request?.AppVersion);
		AddOptionalHeader(message, "X-Storefront", request?.Storefront);

		if (request?.PreferredLocales is { Count: > 0 } locales)
		{
			AddOptionalHeader(message, "X-Preferred-Locales", string.Join(",", locales.Where(static locale => !string.IsNullOrWhiteSpace(locale))));
		}

		var response = await _httpClient.SendAsync(message);
		response.EnsureSuccessStatusCode();

		var result = await response.Content.ReadFromJsonAsync(ApiV1SerializerContext.Default.PaywallOfferingsResponse);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task<string?> GetManagementUrl(string customer_id)
	{
		var subscriber = await GetOrCreateCustomer(customer_id);
		return subscriber.Subscriber?.ManagementUrl;
	}

	static void AddOptionalHeader(HttpRequestMessage message, string name, string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			message.Headers.TryAddWithoutValidation(name, value);
		}
	}
}
