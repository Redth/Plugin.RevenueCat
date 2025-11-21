using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api;
using Plugin.RevenueCat.Api.V2;

namespace Tests;

[TestClass]
public sealed class ApiV2Tests
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task Get_Customer()
	{
		var api = Hosting.ServiceProvider.GetRequiredService<IRevenueCatApiV2>();
		var projectId = Hosting.Configuration["TestSettings:ProjectId"];
		var customerId = Hosting.Configuration["TestSettings:CustomerId"];

		Assert.IsNotNull(projectId, "TestSettings:ProjectId must be configured");
		Assert.IsNotNull(customerId, "TestSettings:CustomerId must be configured");

		var customer = await api.GetCustomer(projectId, customerId);

		Assert.IsNotNull(customer);
		Assert.AreEqual(customerId, customer.Id);
		Assert.IsNotNull(customer.ProjectId);
	}

	[TestMethod]
	public async Task Get_Customer_Has_Active_Entitlements()
	{
		var api = Hosting.ServiceProvider.GetRequiredService<IRevenueCatApiV2>();
		var projectId = Hosting.Configuration["TestSettings:ProjectId"];
		var customerId = Hosting.Configuration["TestSettings:CustomerId"];

		Assert.IsNotNull(projectId, "TestSettings:ProjectId must be configured");
		Assert.IsNotNull(customerId, "TestSettings:CustomerId must be configured");

		var customer = await api.GetCustomer(projectId, customerId);

		Assert.IsNotNull(customer);
		Assert.IsNotNull(customer.ActiveEntitlements);
		Assert.IsNotNull(customer.ActiveEntitlements.Items);
	}

	[TestMethod]
	public async Task Get_Offerings()
	{
		var api = Hosting.ServiceProvider.GetRequiredService<IRevenueCatApiV2>();
		var projectId = Hosting.Configuration["TestSettings:ProjectId"];
		var customerId = Hosting.Configuration["TestSettings:CustomerId"];

		Assert.IsNotNull(projectId, "TestSettings:ProjectId must be configured");
		Assert.IsNotNull(customerId, "TestSettings:CustomerId must be configured");

		var offerings = await api.GetOfferings(projectId, customerId);

		Assert.IsNotNull(offerings);
		Assert.IsNotNull(offerings.Items);
		Assert.IsTrue(offerings.Items.Count > 0);
	}

	[TestMethod]
	public async Task Set_Customer_Attributes()
	{
		// Note: This test requires an API V2 key with the permission:
		// customer_information:customers:read_write
		// If you get a 403 Forbidden error, check your API key permissions in the RevenueCat dashboard.
		
		var api = Hosting.ServiceProvider.GetRequiredService<IRevenueCatApiV2>();
		var projectId = Hosting.Configuration["TestSettings:ProjectId"];
		var customerId = Hosting.Configuration["TestSettings:CustomerId"];

		Assert.IsNotNull(projectId, "TestSettings:ProjectId must be configured");
		Assert.IsNotNull(customerId, "TestSettings:CustomerId must be configured");

		var attributes = new[]
		{
			new CustomerAttribute { Name = "test_attribute", Value = "test_value" }
		};

		try
		{
			// Should not throw
			await api.SetCustomerAttributes(projectId, customerId, attributes);

			// Verify the attribute was set by fetching the customer with expanded attributes
			var customer = await api.GetCustomer(projectId, customerId, expandAttributes: true);
			
			Assert.IsNotNull(customer);
			Assert.IsNotNull(customer.Attributes);
			Assert.IsTrue(customer.Attributes.Items.Any(a => a.Name == "test_attribute"));
		}
		catch (HttpRequestException ex) when (ex.Message.Contains("403"))
		{
			Assert.Inconclusive("API key does not have customer_information:customers:read_write permission. " +
				"This test requires a V2 API key with write permissions for customer attributes. " +
				"Please update your API key permissions in the RevenueCat dashboard.");
		}
	}
}
