using System.Net;
using System.Text;
using Plugin.RevenueCat.Api;
using Plugin.RevenueCat.Api.V2;
using Plugin.RevenueCat.Models;

namespace Tests;

[TestClass]
public sealed class RevenueCatApiV2PaywallTests
{
	[TestMethod]
	[DeploymentItem("data/paywall_v2_response.json")]
	public async Task Get_Paywall_Expands_Components_And_Deserializes_Response()
	{
		var responseJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "paywall_v2_response.json"));
		var handler = new CaptureHandler(responseJson);
		var httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri("https://api.revenuecat.com/v2/")
		};
		var api = new RevenueCatApiV2(httpClient);

		var paywall = await api.GetPaywall("proj/id", "pw/id", expandComponents: true, expandOffering: true);

		Assert.AreEqual("paywall", paywall.Object);
		Assert.AreEqual("pw123456789abcdef", paywall.Id);
		Assert.IsNotNull(paywall.Components?.Published);
		Assert.AreEqual(3, paywall.Components.Published.Revision);
		Assert.AreEqual("en_US", paywall.Components.Published.DefaultLocale);
		Assert.AreEqual("Go Pro", paywall.Components.Published.ComponentsLocalizations["en_US"]["title_lid"].Text);
		Assert.IsTrue(paywall.Components.Published.ParsedComponentsConfig?.Base?.Stack?.Components[0] is PaywallTextComponent);

		if (handler.Request is not { } request)
		{
			Assert.Fail("Expected the API client to send a request.");
			return;
		}

		Assert.AreEqual(
			"https://api.revenuecat.com/v2/projects/proj%2Fid/paywalls/pw%2Fid?expand=components,offering",
			request.RequestUri?.ToString());
	}

	[TestMethod]
	public async Task Get_Paywalls_Uses_Documented_List_Endpoint()
	{
		const string responseJson = """
		{
		  "object": "list",
		  "items": [],
		  "next_page": null,
		  "url": "/v2/projects/proj1/paywalls"
		}
		""";
		var handler = new CaptureHandler(responseJson);
		var httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri("https://api.revenuecat.com/v2/")
		};
		var api = new RevenueCatApiV2(httpClient);

		var paywalls = await api.GetPaywalls("proj1", expandOffering: true, limit: 10, startingAfter: "pw1");

		Assert.AreEqual(0, paywalls.Items.Count);
		Assert.IsNotNull(handler.Request);
		Assert.AreEqual(
			"https://api.revenuecat.com/v2/projects/proj1/paywalls?expand=items.offering&limit=10&starting_after=pw1",
			handler.Request?.RequestUri?.ToString());
	}

	sealed class CaptureHandler : HttpMessageHandler
	{
		readonly string responseJson;

		public CaptureHandler(string responseJson)
		{
			this.responseJson = responseJson;
		}

		public HttpRequestMessage? Request { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Request = request;

			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
			});
		}
	}
}
