using Plugin.RevenueCat.Models;
using System.Text.Json;

namespace Tests
{
	[TestClass]
	public sealed class CustomerInfo_JsonTests
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Lifetime_CustomerInfo_Ignores_Deprecated_OtherPurchases(bool purchaseResult)
		{
			var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "revenuecat", "customer_info_lifetime.json"));
			var customer = purchaseResult
				? JsonSerializer.Deserialize("{\"customer_info\":" + json + "}", ModelSerializerContext.Default.PurchaseResult)!.CustomerInfo
				: JsonSerializer.Deserialize(json, ModelSerializerContext.Default.CustomerInfo);

			Assert.IsNotNull(customer);
			Assert.AreEqual("synthetic-lifetime-owner", customer.Subscriber.OriginalAppUserId);
			Assert.AreEqual("lifetime_access", customer.Subscriber.Entitlements["premium"].ProductIdentifier);
			Assert.IsNull(customer.Subscriber.Entitlements["premium"].ExpiresDate);
			var purchase = customer.Subscriber.NonSubscriptions["lifetime_access"].Single();
			Assert.AreEqual("synthetic-lifetime-order", purchase.Id);
			Assert.AreEqual(59.99d, purchase.Price!.Amount);
			Assert.AreEqual(1793985631597L, customer.RequestDateMs);
		}

		[TestMethod]
		[DeploymentItem("data/customerinfo_json_android.json")]
		[DeploymentItem("data/customerinfo_json_ios.json")]
		[DataRow("android")]
		[DataRow("ios")]
		public void Can_Deserialize_Json(string platform)
		{
			// Simulate creating a log file for this test
			var platformFile = $"customerinfo_json_{platform}.json";
			var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", platformFile));

			var c  = json.ToModel<CustomerInfo>();

			Assert.IsNotNull(c);
		}
	}
}
