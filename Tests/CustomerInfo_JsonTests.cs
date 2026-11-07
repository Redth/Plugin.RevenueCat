using Plugin.RevenueCat.Models;
using System.Text.Json;

namespace Tests
{
	[TestClass]
	public sealed class CustomerInfo_JsonTests
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		[DataRow(false, false)]
		[DataRow(true, false)]
		[DataRow(false, true)]
		[DataRow(true, true)]
		public void Lifetime_CustomerInfo_Preserves_Deprecated_OtherPurchases(bool purchaseResult, bool apiContext)
		{
			var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "revenuecat", "customer_info_lifetime.json"));
			var customerType = apiContext ? Plugin.RevenueCat.Api.V1.ApiV1SerializerContext.Default.CustomerInfo : ModelSerializerContext.Default.CustomerInfo;
			var purchaseType = apiContext ? Plugin.RevenueCat.Api.V1.ApiV1SerializerContext.Default.PurchaseResult : ModelSerializerContext.Default.PurchaseResult;
			var customer = purchaseResult
				? JsonSerializer.Deserialize("{\"customer_info\":" + json + "}", purchaseType)!.CustomerInfo
				: JsonSerializer.Deserialize(json, customerType);

			Assert.IsNotNull(customer);
			Assert.AreEqual("synthetic-lifetime-owner", customer.Subscriber.OriginalAppUserId);
			Assert.AreEqual("lifetime_access", customer.Subscriber.Entitlements["premium"].ProductIdentifier);
			Assert.IsNull(customer.Subscriber.Entitlements["premium"].ExpiresDate);
			var purchase = customer.Subscriber.NonSubscriptions["lifetime_access"].Single();
			Assert.AreEqual("synthetic-lifetime-order", purchase.Id);
			Assert.AreEqual(59.99d, purchase.Price!.Amount);
			Assert.AreEqual(purchase.PurchaseDate, customer.Subscriber.OtherPurchases["lifetime_access"].Single().PurchaseDate);
			Assert.AreEqual(1793985631597L, customer.RequestDateMs);
		}

		[TestMethod]
		public void OtherPurchases_Preserves_Lists_And_Serialization()
		{
			const string json = """
			{"other_purchases":{
			  "legacy":[{"id":"first","price":{"amount":1.99,"currency":"USD"},"extra":"kept"},{"id":"second"}],
			  "single":{"id":"third"},"empty":[],"nullable":null
			}}
			""";
			var subscriber = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.Subscriber)!;
			Assert.AreEqual(2, subscriber.OtherPurchases["legacy"].Count);
			Assert.AreEqual("first", subscriber.OtherPurchases["legacy"][0].Id);
			Assert.AreEqual("second", subscriber.OtherPurchases["legacy"][1].Id);
			Assert.AreEqual(1.99d, subscriber.OtherPurchases["legacy"][0].Price!.Amount);
			Assert.AreEqual("kept", subscriber.OtherPurchases["legacy"][0].ExtensionData!["extra"].GetString());
			Assert.AreEqual("third", subscriber.OtherPurchases["single"].Single().Id);
			Assert.AreEqual(0, subscriber.OtherPurchases["empty"].Count);
			Assert.IsNull(subscriber.OtherPurchases["nullable"]);
			Assert.AreEqual(0, subscriber.NonSubscriptions.Count);

			var serialized = JsonSerializer.Serialize(subscriber, ModelSerializerContext.Default.Subscriber);
			using var document = JsonDocument.Parse(serialized);
			var history = document.RootElement.GetProperty("other_purchases");
			Assert.AreEqual(JsonValueKind.Array, history.GetProperty("legacy").ValueKind);
			Assert.AreEqual(JsonValueKind.Array, history.GetProperty("single").ValueKind);
			Assert.AreEqual(JsonValueKind.Null, history.GetProperty("nullable").ValueKind);
			var restored = JsonSerializer.Deserialize(serialized, ModelSerializerContext.Default.Subscriber)!;
			Assert.AreEqual("second", restored.OtherPurchases["legacy"][1].Id);
			Assert.AreEqual("third", restored.OtherPurchases["single"].Single().Id);
			Assert.AreEqual("kept", restored.OtherPurchases["legacy"][0].ExtensionData!["extra"].GetString());
		}

		[TestMethod]
		public void OtherPurchases_Preserves_Existing_Array_Fixture()
		{
			var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "revenuecat", "customer_info_full.json"));
			var customer = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.CustomerInfo)!;
			var purchase = customer.Subscriber.OtherPurchases["legacy_tip"].Single();
			Assert.AreEqual("legacy_tip", purchase.Id);
			Assert.AreEqual(0.99d, purchase.Price!.Amount);
			Assert.AreEqual("GPA.synthetic.legacy.tip.001", purchase.StoreTransactionId);
		}

		[TestMethod]
		public void OtherPurchases_Respects_Dictionary_Key_Policy_On_Write()
		{
			var options = new JsonSerializerOptions(ModelSerializerContext.Default.Options) { DictionaryKeyPolicy = JsonNamingPolicy.CamelCase };
			var context = new ModelSerializerContext(options);
			var subscriber = JsonSerializer.Deserialize("{\"other_purchases\":{\"LegacyProduct\":[{\"id\":\"OrderId\"}]}}", context.Subscriber)!;
			Assert.AreEqual("OrderId", subscriber.OtherPurchases["LegacyProduct"].Single().Id);
			using var document = JsonDocument.Parse(JsonSerializer.Serialize(subscriber, context.Subscriber));
			Assert.AreEqual("OrderId", document.RootElement.GetProperty("other_purchases").GetProperty("legacyProduct")[0].GetProperty("id").GetString());
		}

		[TestMethod]
		[DataRow("{}")]
		[DataRow("{\"other_purchases\":{}}")]
		public void OtherPurchases_Absent_Or_Empty_Remains_Empty(string json)
		{
			var subscriber = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.Subscriber)!;
			Assert.AreEqual(0, subscriber.OtherPurchases.Count);
		}

		[TestMethod]
		public void OtherPurchases_Null_Remains_Null()
		{
			var subscriber = JsonSerializer.Deserialize("{\"other_purchases\":null}", ModelSerializerContext.Default.Subscriber)!;
			Assert.IsNull(subscriber.OtherPurchases);
		}

		[TestMethod]
		[DataRow("{\"other_purchases\":[]}")]
		[DataRow("{\"other_purchases\":{\"product\":42}}")]
		[DataRow("{\"other_purchases\":{\"product\":[42]}}")]
		public void OtherPurchases_Rejects_Invalid_Shapes(string json)
		{
			Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.Subscriber));
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
