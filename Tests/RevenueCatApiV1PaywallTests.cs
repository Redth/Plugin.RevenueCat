using System.Net;
using System.Text;
using Plugin.RevenueCat.Api;
using Plugin.RevenueCat.Api.V1;

namespace Tests;

[TestClass]
public sealed class RevenueCatApiV1PaywallTests
{
	[TestMethod]
	[DeploymentItem("data/paywall_offerings_response.json")]
	public async Task Get_Paywall_Offerings_Uses_Runtime_Offerings_Endpoint()
	{
		var responseJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "paywall_offerings_response.json"));
		var handler = new CaptureHandler(responseJson);
		var httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri("https://api.revenuecat.com/v1/")
		};
		var api = new RevenueCatApiV1(httpClient);

		var response = await api.GetPaywallOfferings(
			"app/user",
			new PaywallOfferingsRequest
			{
				Platform = "android",
				PlatformVersion = "36",
				ClientVersion = "1.0.0",
				AppVersion = "42",
				Storefront = "USA",
				PreferredLocales = ["en-US", "fr-CA"]
			});

		Assert.AreEqual("default", response.CurrentOfferingId);
		Assert.IsNotNull(response.CurrentOffering?.PaywallComponents);
		Assert.IsNotNull(response.UiConfig);

		if (handler.Request is not { } request)
		{
			Assert.Fail("Expected the API client to send a request.");
			return;
		}

		Assert.AreEqual(
			"https://api.revenuecat.com/v1/subscribers/app%2Fuser/offerings",
			request.RequestUri?.ToString());
		Assert.AreEqual("android", request.Headers.GetValues("X-Platform").Single());
		Assert.AreEqual("36", request.Headers.GetValues("X-Platform-Version").Single());
		Assert.AreEqual("1.0.0", request.Headers.GetValues("X-Client-Version").Single());
		Assert.AreEqual("42", request.Headers.GetValues("X-Client-Build-Version").Single());
		Assert.AreEqual("USA", request.Headers.GetValues("X-Storefront").Single());
		Assert.AreEqual("en-US,fr-CA", request.Headers.GetValues("X-Preferred-Locales").Single());
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
