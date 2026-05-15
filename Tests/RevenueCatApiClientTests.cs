using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api;
using Plugin.RevenueCat.Api.V1;
using Plugin.RevenueCat.Api.V2;

namespace Tests;

[TestClass]
public sealed class RevenueCatApiClientTests
{
	[TestMethod]
	public async Task V1GetOrCreateCustomer_SendsSubscriberRequestAndDeserializesCustomer()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse(V1SubscriberResponseJson("customer-123")));
		var api = new RevenueCatApiV1(CreateHttpClient(handler, "https://api.revenuecat.com/v1/"));

		var response = await api.GetOrCreateCustomer("customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual(HttpMethod.Get, request.Method);
		Assert.AreEqual("/v1/subscribers/customer-123", request.RequestUri.PathAndQuery);
		Assert.AreEqual("customer-123", response.Subscriber?.OriginalAppUserId);
	}

	[TestMethod]
	public async Task V1GetOfferings_SendsOfferingsRequestAndDeserializesOfferings()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse("""
{
  "current_offering_id": "default",
  "offerings": [
    {
      "identifier": "default",
      "description": "Default offering",
      "packages": []
    }
  ]
}
"""));
		var api = new RevenueCatApiV1(CreateHttpClient(handler, "https://api.revenuecat.com/v1/"));

		var response = await api.GetOfferings("customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual(HttpMethod.Get, request.Method);
		Assert.AreEqual("/v1/subscribers/customer-123/offerings", request.RequestUri.PathAndQuery);
		Assert.AreEqual("default", response.CurrentOfferingId);
		Assert.AreEqual(1, response.Offerings.Count);
		Assert.AreEqual("default", response.Offerings[0].Identifier);
	}

	[TestMethod]
	public async Task V1GetManagementUrl_ReturnsSubscriberManagementUrl()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse(V1SubscriberResponseJson("customer-123")));
		var api = new RevenueCatApiV1(CreateHttpClient(handler, "https://api.revenuecat.com/v1/"));

		var managementUrl = await api.GetManagementUrl("customer-123");

		Assert.AreEqual("https://pay.rev.cat/manage/customer-123", managementUrl);
		var request = AssertSingleRequest(handler);
		Assert.AreEqual("/v1/subscribers/customer-123", request.RequestUri.PathAndQuery);
	}

	[TestMethod]
	public async Task V1GetOrCreateCustomer_WhenResponseIsError_ThrowsWithStatusCode()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse("{\"code\":\"not_found\"}", HttpStatusCode.NotFound));
		var api = new RevenueCatApiV1(CreateHttpClient(handler, "https://api.revenuecat.com/v1/"));

		var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(
			async () => await api.GetOrCreateCustomer("missing-customer"));

		Assert.AreEqual(HttpStatusCode.NotFound, exception.StatusCode);
	}

	[TestMethod]
	public async Task AddRevenueCatApiV1_ConfiguresBearerAuthorizationHeader()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse(V1SubscriberResponseJson("customer-123")));
		var services = new ServiceCollection();
		services.AddRevenueCatApiV1(
			new RevenueCatApiV1Settings { ApiKey = "v1_test_key" },
			builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
		using var provider = services.BuildServiceProvider();
		var api = provider.GetRequiredService<IRevenueCatApiV1>();

		await api.GetOrCreateCustomer("customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual("Bearer", request.Authorization?.Scheme);
		Assert.AreEqual("v1_test_key", request.Authorization?.Parameter);
	}

	[TestMethod]
	public async Task V2GetCustomer_WithDefaultExpandAttributes_AddsExpandQueryAndDeserializesAttributes()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse(V2CustomerResponseJson("proj_123", "customer-123")));
		var api = new RevenueCatApiV2(CreateHttpClient(handler, "https://api.revenuecat.com/v2/"));

		var customer = await api.GetCustomer("proj_123", "customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual(HttpMethod.Get, request.Method);
		Assert.AreEqual("/v2/projects/proj_123/customers/customer-123?expand=attributes", request.RequestUri.PathAndQuery);
		Assert.AreEqual("customer-123", customer.Id);
		Assert.AreEqual("proj_123", customer.ProjectId);
		Assert.AreEqual(1, customer.Attributes.Items.Count);
		Assert.AreEqual("tier", customer.Attributes.Items[0].Name);
	}

	[TestMethod]
	public async Task V2GetCustomer_WithoutExpandAttributes_OmitsExpandQuery()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse(V2CustomerResponseJson("proj_123", "customer-123")));
		var api = new RevenueCatApiV2(CreateHttpClient(handler, "https://api.revenuecat.com/v2/"));

		await api.GetCustomer("proj_123", "customer-123", expandAttributes: false);

		var request = AssertSingleRequest(handler);
		Assert.AreEqual("/v2/projects/proj_123/customers/customer-123", request.RequestUri.PathAndQuery);
	}

	[TestMethod]
	public async Task V2GetOfferings_SendsCustomerIdQueryAndDeserializesOfferings()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse("""
{
  "items": [
    {
      "id": "off_123",
      "lookup_key": "default",
      "display_name": "Default",
      "is_current": true,
      "created_at": 1700000000000,
      "metadata": { "tier": "gold" },
      "project_id": "proj_123",
      "packages": { "items": [], "url": "/v2/projects/proj_123/offerings/off_123/packages", "next_page": null }
    }
  ],
  "url": "/v2/projects/proj_123/offerings",
  "next_page": null
}
"""));
		var api = new RevenueCatApiV2(CreateHttpClient(handler, "https://api.revenuecat.com/v2/"));

		var offerings = await api.GetOfferings("proj_123", "customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual(HttpMethod.Get, request.Method);
		Assert.AreEqual("/v2/projects/proj_123/offerings?customer_id=customer-123", request.RequestUri.PathAndQuery);
		Assert.AreEqual(1, offerings.Items.Count);
		Assert.AreEqual("off_123", offerings.Items[0].Id);
		Assert.AreEqual("Default", offerings.Items[0].DisplayName);
	}

	[TestMethod]
	public async Task V2SetCustomerAttributes_SendsExpectedJsonBody()
	{
		var handler = new RecordingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
		var api = new RevenueCatApiV2(CreateHttpClient(handler, "https://api.revenuecat.com/v2/"));
		var attributes = new[]
		{
			new CustomerAttribute { Name = "test_attribute", Value = "test_value" },
			new CustomerAttribute { Name = "tier", Value = "gold" }
		};

		await api.SetCustomerAttributes("proj_123", "customer-123", attributes);

		var request = AssertSingleRequest(handler);
		Assert.AreEqual(HttpMethod.Post, request.Method);
		Assert.AreEqual("/v2/projects/proj_123/customers/customer-123/attributes", request.RequestUri.PathAndQuery);
		Assert.AreEqual("application/json", request.ContentType);
		Assert.IsNotNull(request.Content);
		using var document = JsonDocument.Parse(request.Content);
		var attributeItems = document.RootElement.GetProperty("attributes");
		Assert.AreEqual(JsonValueKind.Array, attributeItems.ValueKind);
		Assert.AreEqual(2, attributeItems.GetArrayLength());
		Assert.AreEqual("test_attribute", attributeItems[0].GetProperty("name").GetString());
		Assert.AreEqual("test_value", attributeItems[0].GetProperty("value").GetString());
		Assert.AreEqual("tier", attributeItems[1].GetProperty("name").GetString());
		Assert.AreEqual("gold", attributeItems[1].GetProperty("value").GetString());
	}

	[TestMethod]
	public async Task V2GetSubscriptions_SendsSubscriptionsRequestAndDeserializesManagementUrl()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse("""
{
  "items": [
    {
      "id": "sub_123",
      "object": "subscription",
      "customer_id": "customer-123",
      "product_id": "prod_monthly",
      "gives_access": true,
      "pending_payment": false,
      "auto_renewal_status": "will_renew",
      "status": "active",
      "store": "app_store",
      "environment": "production",
      "is_sandbox": false,
      "entitlement_ids": ["pro"],
      "management_url": "https://apps.apple.com/account/subscriptions"
    }
  ],
  "url": "/v2/projects/proj_123/customers/customer-123/subscriptions",
  "next_page": null
}
"""));
		var api = new RevenueCatApiV2(CreateHttpClient(handler, "https://api.revenuecat.com/v2/"));

		var subscriptions = await api.GetSubscriptions("proj_123", "customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual(HttpMethod.Get, request.Method);
		Assert.AreEqual("/v2/projects/proj_123/customers/customer-123/subscriptions", request.RequestUri.PathAndQuery);
		Assert.AreEqual(1, subscriptions.Items.Count);
		Assert.AreEqual("sub_123", subscriptions.Items[0].Id);
		Assert.AreEqual("https://apps.apple.com/account/subscriptions", subscriptions.Items[0].ManagementUrl);
	}

	[TestMethod]
	public async Task V2GetCustomer_WhenResponseIsError_ThrowsWithStatusCode()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse("{\"code\":\"forbidden\"}", HttpStatusCode.Forbidden));
		var api = new RevenueCatApiV2(CreateHttpClient(handler, "https://api.revenuecat.com/v2/"));

		var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(
			async () => await api.GetCustomer("proj_123", "customer-123"));

		Assert.AreEqual(HttpStatusCode.Forbidden, exception.StatusCode);
	}

	[TestMethod]
	public async Task AddRevenueCatApiV2_ConfiguresBearerAuthorizationHeader()
	{
		var handler = new RecordingHttpMessageHandler(JsonResponse(V2CustomerResponseJson("proj_123", "customer-123")));
		var services = new ServiceCollection();
		services.AddRevenueCatApiV2(
			new RevenueCatApiV2Settings { ApiKey = "v2_test_key" },
			builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
		using var provider = services.BuildServiceProvider();
		var api = provider.GetRequiredService<IRevenueCatApiV2>();

		await api.GetCustomer("proj_123", "customer-123");

		var request = AssertSingleRequest(handler);
		Assert.AreEqual("Bearer", request.Authorization?.Scheme);
		Assert.AreEqual("v2_test_key", request.Authorization?.Parameter);
	}

	private static HttpClient CreateHttpClient(RecordingHttpMessageHandler handler, string baseAddress)
	{
		var httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri(baseAddress)
		};
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test_api_key");
		return httpClient;
	}

	private static RecordedRequest AssertSingleRequest(RecordingHttpMessageHandler handler)
	{
		Assert.AreEqual(1, handler.Requests.Count);
		return handler.Requests[0];
	}

	private static HttpResponseMessage JsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK) => new(statusCode)
	{
		Content = new StringContent(json, Encoding.UTF8, "application/json")
	};

	private static string V1SubscriberResponseJson(string customerId) => $$"""
{
  "request_date": "2024-01-01T00:00:00Z",
  "subscriber": {
    "original_app_user_id": "{{customerId}}",
    "management_url": "https://pay.rev.cat/manage/{{customerId}}",
    "entitlements": {},
    "subscriptions": {},
    "non_subscriptions": {}
  }
}
""";

	private static string V2CustomerResponseJson(string projectId, string customerId) => $$"""
{
  "id": "{{customerId}}",
  "project_id": "{{projectId}}",
  "active_entitlements": {
    "items": [
      { "entitlement_id": "pro", "expires_at": 1700000000000 }
    ],
    "url": "/v2/projects/{{projectId}}/customers/{{customerId}}/active_entitlements",
    "next_page": null
  },
  "attributes": {
    "items": [
      { "name": "tier", "value": "gold", "updated_at": 1700000000000 }
    ],
    "url": "/v2/projects/{{projectId}}/customers/{{customerId}}/attributes",
    "next_page": null
  }
}
""";

	private sealed class RecordingHttpMessageHandler : HttpMessageHandler
	{
		private readonly Queue<HttpResponseMessage> _responses;

		public RecordingHttpMessageHandler(params HttpResponseMessage[] responses)
		{
			_responses = new Queue<HttpResponseMessage>(responses);
		}

		public List<RecordedRequest> Requests { get; } = [];

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var content = request.Content is null
				? null
				: await request.Content.ReadAsStringAsync(cancellationToken);
			Requests.Add(new RecordedRequest(
				request.Method,
				request.RequestUri ?? throw new InvalidOperationException("Request URI was not set."),
				request.Headers.Authorization,
				content,
				request.Content?.Headers.ContentType?.MediaType));

			return _responses.Count > 0
				? _responses.Dequeue()
				: new HttpResponseMessage(HttpStatusCode.OK);
		}
	}

	private sealed record RecordedRequest(
		HttpMethod Method,
		Uri RequestUri,
		AuthenticationHeaderValue? Authorization,
		string? Content,
		string? ContentType);
}
