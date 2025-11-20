using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api;

namespace Tests;

[TestClass]
public sealed class ApiV1Tests
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task Get_Or_Create_Customer_Has_Entitlements()
	{
		var api = Hosting.ServiceProvider.GetRequiredService<IRevenueCatApiV1>();

		var r = await api.GetOrCreateCustomer("tfp-120465");

		Assert.IsNotNull(r);
		Assert.IsNotNull(r.Subscriber?.Entitlements);
		Assert.IsTrue(r.Subscriber.Entitlements.ContainsKey("pro1year"));
	}


	[TestMethod]
	public async Task Get_Offerings()
	{
		var api = Hosting.ServiceProvider.GetRequiredService<IRevenueCatApiV1>();

		var r = await api.GetOfferings("tfp-120465");

		Assert.IsNotNull(r);
		Assert.AreEqual("pro1year", r.CurrentOfferingId);
	}
}
