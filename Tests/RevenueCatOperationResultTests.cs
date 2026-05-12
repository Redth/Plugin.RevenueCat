using Plugin.RevenueCat;
using Plugin.RevenueCat.Models;

namespace Tests;

[TestClass]
public sealed class RevenueCatOperationResultTests
{
	[TestMethod]
	public async Task Result_Api_Preserves_Serialized_RevenueCat_Error()
	{
		var manager = CreateManager(new FakePlatform
		{
			LoginAsyncFunc = _ => Task.FromException<string?>(new Exception(
				"""RevenueCatError:{"code":"NetworkError","message":"Network unavailable","domain":"RevenueCat","source":"android"}"""))
		});

		var result = await manager.LoginWithResultAsync("user");
		var nullableResult = await manager.LoginAsync("user");

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("NetworkError", result.Error?.Code);
		Assert.AreEqual("Network unavailable", result.Error?.Message);
		Assert.AreEqual("RevenueCat", result.Error?.Domain);
		Assert.IsNull(nullableResult);
	}

	[TestMethod]
	public async Task Purchase_Result_Api_Preserves_User_Cancellation()
	{
		var manager = CreateManager(new FakePlatform
		{
			PurchaseWithResultAsyncFunc = (_, _, _, _) => Task.FromResult<string?>(
				"""{"user_cancelled":true}""")
		});

		var result = await manager.PurchaseWithOperationResultAsync(null, "default", "$rc_monthly");
		var purchaseResult = await manager.PurchaseWithResultAsync(null, "default", "$rc_monthly");

		Assert.IsFalse(result.IsSuccess);
		Assert.IsTrue(result.UserCancelled);
		Assert.AreEqual("PurchaseCancelledError", result.Error?.Code);
		Assert.IsNotNull(result.Value);
		Assert.IsTrue(result.Value.UserCancelled);
		Assert.IsNotNull(purchaseResult);
		Assert.IsTrue(purchaseResult.UserCancelled);
	}

	[TestMethod]
	public async Task Purchase_Options_Are_Passed_To_Platform_As_Json()
	{
		string? purchaseOptionsJson = null;
		var manager = CreateManager(new FakePlatform
		{
			PurchaseProductAsyncFunc = (_, _, _, optionsJson) =>
			{
				purchaseOptionsJson = optionsJson;
				return Task.FromResult<string?>("""{"user_cancelled":true}""");
			}
		});

		await manager.PurchaseProductWithOperationResultAsync(
			null,
			"monthly",
			RevenueCatProductType.Subscription,
			new PurchaseOptions
			{
				SubscriptionOptionId = "base:offer",
				OldProductIdentifier = "old_monthly",
				ReplacementMode = RevenueCatReplacementMode.WithTimeProration,
				IsPersonalizedPrice = true,
				PresentedOfferingIdentifier = "default",
				StoreProductDiscountIdentifier = "promo_offer"
			});

		Assert.IsNotNull(purchaseOptionsJson);

		using var document = System.Text.Json.JsonDocument.Parse(purchaseOptionsJson);
		var root = document.RootElement;
		Assert.AreEqual("base:offer", root.GetProperty("subscription_option_id").GetString());
		Assert.AreEqual("old_monthly", root.GetProperty("old_product_identifier").GetString());
		Assert.AreEqual("WithTimeProration", root.GetProperty("replacement_mode").GetString());
		Assert.IsTrue(root.GetProperty("is_personalized_price").GetBoolean());
		Assert.AreEqual("default", root.GetProperty("presented_offering_identifier").GetString());
		Assert.AreEqual("promo_offer", root.GetProperty("store_product_discount_identifier").GetString());
	}

	[TestMethod]
	public async Task Result_Api_Returns_Parse_Errors()
	{
		var manager = CreateManager(new FakePlatform
		{
			GetOfferingsAsyncFunc = () => Task.FromResult<string?>("{")
		});

		var result = await manager.GetOfferingsWithResultAsync();

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("invalid_json", result.Error?.Code);
	}

	[TestMethod]
	public async Task Advanced_Result_Apis_Parse_Virtual_Currencies_And_Web_Redemption()
	{
		var manager = CreateManager(new FakePlatform
		{
			GetVirtualCurrenciesAsyncFunc = () => Task.FromResult<string?>(
				"""{"all":{"coins":{"code":"coins","name":"Coins","balance":42,"server_description":"Spendable coins"}}}"""),
			RedeemWebPurchaseAsyncFunc = _ => Task.FromResult<string?>(
				"""{"status":"expired","obfuscated_email":"t***@example.com"}""")
		});

		var virtualCurrencies = await manager.GetVirtualCurrenciesWithResultAsync();
		var redemption = await manager.RedeemWebPurchaseWithResultAsync("https://example.com/redeem");

		Assert.IsTrue(virtualCurrencies.IsSuccess);
		Assert.AreEqual(42, virtualCurrencies.Value?.All["coins"].Balance);
		Assert.IsTrue(redemption.IsSuccess);
		Assert.AreEqual("expired", redemption.Value?.Status);
		Assert.AreEqual("t***@example.com", redemption.Value?.ObfuscatedEmail);
	}


	static RevenueCatManager CreateManager(IRevenueCatPlatformImplementation platform)
		=> new(new RevenueCatOptions(null, null, null, null, false, null, null, null), platform);

	sealed class FakePlatform : IRevenueCatPlatformImplementation
	{
		public Func<string, Task<string?>>? LoginAsyncFunc { get; init; }
		public Func<Task<string?>>? GetOfferingsAsyncFunc { get; init; }
		public Func<object?, string, string, string?, Task<string?>>? PurchaseWithResultAsyncFunc { get; init; }
		public Func<object?, string, string?, string?, Task<string?>>? PurchaseProductAsyncFunc { get; init; }
		public Func<Task<string?>>? GetVirtualCurrenciesAsyncFunc { get; init; }
		public Func<string, Task<string?>>? RedeemWebPurchaseAsyncFunc { get; init; }

		public string? ApiKey => null;
		public string? AppUserId => null;
		public bool IsAnonymous => true;

		public void SetCustomerInfoUpdatedHandler(Action<string> handler) { }
		public void Initialize(RevenueCatOptions options) { }

		public Task<string?> LoginAsync(string userId)
			=> LoginAsyncFunc?.Invoke(userId) ?? Task.FromResult<string?>(null);

		public Task<string?> LogOutAsync()
			=> Task.FromResult<string?>(null);

		public Task<string?> GetCustomerInfoAsync(bool force)
			=> Task.FromResult<string?>(null);

		public void InvalidateCustomerInfoCache() { }

		public Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
			=> Task.FromResult<string?>(null);

		public Task<string?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, string? purchaseOptionsJson)
			=> PurchaseWithResultAsyncFunc?.Invoke(platformContext, offeringIdentifier, packageIdentifier, purchaseOptionsJson) ?? Task.FromResult<string?>(null);

		public Task<string?> PurchaseProductAsync(object? platformContext, string productIdentifier, string? type, string? purchaseOptionsJson)
			=> PurchaseProductAsyncFunc?.Invoke(platformContext, productIdentifier, type, purchaseOptionsJson) ?? Task.FromResult<string?>(null);

		public Task<string?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, string? type, string? purchaseOptionsJson)
			=> Task.FromResult<string?>(null);

		public Task<string?> GetOfferingAsync(string offeringIdentifier)
			=> Task.FromResult<string?>(null);

		public Task<string?> GetOfferingsAsync()
			=> GetOfferingsAsyncFunc?.Invoke() ?? Task.FromResult<string?>(null);

		public Task<string?> GetOfferingForPlacementAsync(string placementIdentifier)
			=> Task.FromResult<string?>(null);

		public Task<string?> GetProductsAsync(string productIdentifiersCsv, string? type)
			=> Task.FromResult<string?>(null);

		public Task<string?> RestoreAsync()
			=> Task.FromResult<string?>(null);

		public Task<string?> SyncPurchasesAsync()
			=> Task.FromResult<string?>(null);

		public Task<bool> CanMakePaymentsAsync(object? platformContext)
			=> Task.FromResult(true);

		public Task<string?> GetStorefrontAsync()
			=> Task.FromResult<string?>(null);

		public Task<string?> GetVirtualCurrenciesAsync()
			=> GetVirtualCurrenciesAsyncFunc?.Invoke() ?? Task.FromResult<string?>(null);

		public void InvalidateVirtualCurrenciesCache() { }

		public Task<string?> RedeemWebPurchaseAsync(string redemptionLink)
			=> RedeemWebPurchaseAsyncFunc?.Invoke(redemptionLink) ?? Task.FromResult<string?>(null);

		public Task<string?> GetAmazonLwaConsentStatusAsync()
			=> Task.FromResult<string?>(null);

		public Task SyncOfferingsAndAttributesIfNeeded()
			=> Task.CompletedTask;

		public void CollectDeviceIdentifiers() { }
		public void SetEmail(string email) { }
		public void SetPhoneNumber(string? phoneNumber) { }
		public void SetPushToken(string? pushToken) { }
		public void SetDisplayName(string displayName) { }
		public void SetMediaSource(string? mediaSource) { }
		public void SetAd(string ad) { }
		public void SetAdGroup(string adGroup) { }
		public void SetCampaign(string campaign) { }
		public void SetCreative(string creative) { }
		public void SetKeyword(string keyword) { }
		public void SetAttribute(string key, string? value) { }
		public void SetAttributes(IDictionary<string, string> attributes) { }
	}
}
