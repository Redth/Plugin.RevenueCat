using System.Text.Json.Serialization;
using Plugin.RevenueCat.Api.V1;
using Plugin.RevenueCat.Models;
using V1Json = Plugin.RevenueCat.Api.V1.JsonUtil;
using V2Json = Plugin.RevenueCat.Api.V2.JsonUtil;

namespace Tests;

[TestClass]
public sealed class JsonUtilTests
{
	// Regression for https://github.com/Redth/Plugin.RevenueCat/issues/28
	[TestMethod]
	public void V1_GenericJsonUtil_RoundTripsEmptyCustomerInfo()
	{
		var json = V1Json.Serialize(new CustomerInfo());
		var roundTrip = V1Json.Deserialize<CustomerInfo>(json);

		Assert.IsNotNull(roundTrip);
	}

	[TestMethod]
	public void V1_GenericJsonUtil_RoundTripsCustomerInfoFixture()
	{
		var original = ReadFixture("customer_info_full.json").ToCustomerInfo()!;

		var roundTrip = V1Json.Deserialize<CustomerInfo>(V1Json.Serialize(original))!;

		AssertCustomerInfoEqual(original, roundTrip);
	}

	[TestMethod]
	public void V1_TypedJsonUtil_RoundTripsCustomerInfoFixture()
	{
		var original = ReadFixture("customer_info_full.json").ToCustomerInfo()!;

		var json = original.Serialize(ApiV1SerializerContext.Default.CustomerInfo);
		var roundTrip = json.Deserialize(ApiV1SerializerContext.Default.CustomerInfo)!;

		AssertCustomerInfoEqual(original, roundTrip);
	}

	[TestMethod]
	public void V1_GenericJsonUtil_RoundTripsOfferingsFixture()
	{
		var original = ReadFixture("offerings_full.json").ToOfferings()!;

		var roundTrip = V1Json.Deserialize<Offerings>(V1Json.Serialize(original))!;

		Assert.AreEqual(original.Current?.Identifier, roundTrip.Current?.Identifier);
		Assert.AreEqual(original.All.Count, roundTrip.All.Count);
	}

	[TestMethod]
	public void V1_GenericJsonUtil_RoundTripsStoreProductsFixture()
	{
		var original = ReadFixture("products_full.json").ToStoreProducts()!;

		var roundTrip = V1Json.Deserialize<List<StoreProduct>>(V1Json.Serialize(original))!;

		CollectionAssert.AreEqual(
			original.Select(p => p.Identifier).ToList(),
			roundTrip.Select(p => p.Identifier).ToList());
	}

	[TestMethod]
	public void V1_GenericJsonUtil_RoundTripsPurchaseResultFixture()
	{
		var original = ReadFixture("purchase_result_success.json").ToPurchaseResult()!;

		var roundTrip = V1Json.Deserialize<PurchaseResult>(V1Json.Serialize(original))!;

		Assert.AreEqual(original.UserCancelled, roundTrip.UserCancelled);
		Assert.AreEqual(original.StoreTransaction?.ProductIdentifier, roundTrip.StoreTransaction?.ProductIdentifier);
		Assert.AreEqual(original.CustomerInfo?.Subscriber.OriginalAppUserId, roundTrip.CustomerInfo?.Subscriber.OriginalAppUserId);
	}

	[TestMethod]
	public void V1_Settings_ResolvesEveryPublicModelType()
	{
		var modelTypes = typeof(ModelSerializerContext)
			.GetCustomAttributesData()
			.Where(d => d.AttributeType == typeof(JsonSerializableAttribute))
			.Select(d => (Type)d.ConstructorArguments[0].Value!)
			.ToList();

		Assert.IsNotEmpty(modelTypes);
		foreach (var type in modelTypes)
			Assert.IsNotNull(V1Json.Settings.GetTypeInfo(type), $"V1 JsonUtil.Settings cannot resolve {type}");
	}

	[TestMethod]
	public void V2_TypedJsonUtil_RoundTripsCustomer()
	{
		var original = new Plugin.RevenueCat.Api.V2.Customer { Id = "user-1" };

		var json = original.Serialize(Plugin.RevenueCat.Api.V2.ApiV2SerializerContext.Default.Customer);
		var roundTrip = json.Deserialize(Plugin.RevenueCat.Api.V2.ApiV2SerializerContext.Default.Customer)!;

		Assert.AreEqual("user-1", roundTrip.Id);
		Assert.AreEqual(json, V2Json.Serialize(original));
	}

	static void AssertCustomerInfoEqual(CustomerInfo expected, CustomerInfo actual)
	{
		Assert.AreEqual(expected.RequestDate, actual.RequestDate);
		Assert.AreEqual(expected.SchemaVersion, actual.SchemaVersion);
		Assert.AreEqual(expected.ManagementUrl, actual.ManagementUrl);
		Assert.AreEqual(expected.LatestExpirationDate, actual.LatestExpirationDate);
		CollectionAssert.AreEqual(expected.ActiveSubscriptions, actual.ActiveSubscriptions);
		Assert.AreEqual(expected.Subscriber.OriginalAppUserId, actual.Subscriber.OriginalAppUserId);
		CollectionAssert.AreEquivalent(
			expected.Subscriber.Entitlements.Keys.ToList(),
			actual.Subscriber.Entitlements.Keys.ToList());
	}

	static string ReadFixture(string fileName)
		=> File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "revenuecat", fileName));
}
