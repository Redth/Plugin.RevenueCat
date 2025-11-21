using Plugin.RevenueCat.Api.V1;
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
		
		var result = await response.Content.ReadFromJsonAsync<SubscriberResponse>(JsonUtil.Settings);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task<OfferingsResponse> GetOfferings(string customer_id)
	{
		var response = await _httpClient.GetAsync($"subscribers/{customer_id}/offerings");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync<OfferingsResponse>(JsonUtil.Settings);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}
}
