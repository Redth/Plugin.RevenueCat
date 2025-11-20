using Plugin.RevenueCat.Api.V2;
using System.Net.Http.Json;

namespace Plugin.RevenueCat.Api;

public class RevenueCatApiV2 : IRevenueCatApiV2
{
	private readonly HttpClient _httpClient;

	public RevenueCatApiV2(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<Customer> GetCustomer(string project_id, string customer_id)
	{
		var response = await _httpClient.GetAsync($"/projects/{project_id}/customers/{customer_id}");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync<Customer>(JsonUtil.Settings);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task SetCustomerAttributes(string project_id, string customer_id, IEnumerable<CustomerAttribute> attributes)
	{
		var response = await _httpClient.PostAsJsonAsync(
			$"/projects/{project_id}/customers/{customer_id}/attributes",
			attributes,
			JsonUtil.Settings);
		
		response.EnsureSuccessStatusCode();
	}

	public async Task<PagedList<Offering>> GetOfferings(string customer_id)
	{
		var response = await _httpClient.GetAsync($"/subscribers/{customer_id}");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync<PagedList<Offering>>(JsonUtil.Settings);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}
}
