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

	public async Task<Customer> GetCustomer(string project_id, string customer_id, bool expandAttributes = true)
	{
		var url = $"projects/{project_id}/customers/{customer_id}";
		if (expandAttributes)
		{
			url += "?expand=attributes";
		}
		
		var response = await _httpClient.GetAsync(url);
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync(ApiV2SerializerContext.Default.Customer);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task SetCustomerAttributes(string project_id, string customer_id, IEnumerable<CustomerAttribute> attributes)
	{
		// RevenueCat V2 API expects attributes as an array of objects with name and value properties
		var payload = new SetAttributesRequest
		{
			Attributes = attributes.Select(a => new SetAttributesRequest.AttributeItem 
			{ 
				Name = a.Name, 
				Value = a.Value 
			}).ToArray()
		};
		
		var request = new HttpRequestMessage(HttpMethod.Post, $"projects/{project_id}/customers/{customer_id}/attributes")
		{
			Content = JsonContent.Create(payload, ApiV2SerializerContext.Default.SetAttributesRequest)
		};
		
		var response = await _httpClient.SendAsync(request);
		response.EnsureSuccessStatusCode();
	}

	public async Task<PagedList<Offering>> GetOfferings(string project_id, string customer_id)
	{
		var response = await _httpClient.GetAsync($"projects/{project_id}/offerings?customer_id={customer_id}");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync(ApiV2SerializerContext.Default.PagedListOffering);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}

	public async Task<PagedList<Subscription>> GetSubscriptions(string project_id, string customer_id)
	{
		var response = await _httpClient.GetAsync($"projects/{project_id}/customers/{customer_id}/subscriptions");
		response.EnsureSuccessStatusCode();
		
		var result = await response.Content.ReadFromJsonAsync(ApiV2SerializerContext.Default.PagedListSubscription);
		return result ?? throw new InvalidOperationException("Failed to deserialize response");
	}
}
