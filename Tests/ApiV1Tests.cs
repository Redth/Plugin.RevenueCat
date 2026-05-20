using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api;

namespace Tests;

[TestClass]
[TestCategory("Integration")]
[TestCategory("RequiresRevenueCatSecrets")]
public sealed class ApiV1Tests
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task Get_Or_Create_Customer_Has_Entitlements()
	{
		var api = Hosting.RequireApiV1();
		var customerId = Hosting.RequireSetting("TestSettings:CustomerId");

		var r = await api.GetOrCreateCustomer(customerId);

		Assert.IsNotNull(r);
		Assert.IsNotNull(r.Subscriber?.Entitlements);
	}


	[TestMethod]
	public async Task Get_Offerings()
	{
		var api = Hosting.RequireApiV1();
		var customerId = Hosting.RequireSetting("TestSettings:CustomerId");

		var r = await api.GetOfferings(customerId);

		Assert.IsNotNull(r);
		Assert.IsNotNull(r.CurrentOfferingId);
	}

	[TestMethod]
	public async Task Get_Management_Url()
	{
		var api = Hosting.RequireApiV1();
		var customerId = Hosting.RequireSetting("TestSettings:CustomerId");

		// GetManagementUrl should not throw; it may return null if no management URL exists
		var managementUrl = await api.GetManagementUrl(customerId);

		// ManagementUrl may be null for customers without web subscriptions
		// Just verify the call completes successfully
	}
}
