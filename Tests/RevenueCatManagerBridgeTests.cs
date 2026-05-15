using System.Text.Json;
using Plugin.RevenueCat;
using Plugin.RevenueCat.Models;

namespace Tests;

[TestClass]
public sealed class RevenueCatManagerBridgeTests
{
	[TestMethod]
	public async Task OfferingAndProductApis_ForwardParametersAndParseFixtures()
	{
		var platform = new RecordingPlatform
		{
			OfferingJson = CurrentOfferingJson(),
			OfferingsJson = ReadFixture("offerings_full.json"),
			ProductsJson = ReadFixture("products_full.json")
		};
		var manager = CreateManager(platform);

		var offering = await manager.GetOfferingAsync("default");
		var offerings = await manager.GetOfferingsWithResultAsync();
		var placement = await manager.GetOfferingForPlacementAsync("home_screen");
		var products = await manager.GetProductsWithResultAsync(new[] { " pro_monthly ", "", "coins_pack", "pro_monthly" }, RevenueCatProductType.InApp);

		Assert.AreEqual(1, platform.OfferingIdentifiers.Count);
		Assert.AreEqual("default", platform.OfferingIdentifiers[0]);
		Assert.AreEqual(1, platform.PlacementIdentifiers.Count);
		Assert.AreEqual("home_screen", platform.PlacementIdentifiers[0]);
		Assert.AreEqual(1, platform.ProductRequests.Count);
		Assert.AreEqual("pro_monthly,coins_pack", platform.ProductRequests[0].ProductIdentifiersCsv);
		Assert.AreEqual("inapp", platform.ProductRequests[0].Type);
		Assert.IsNotNull(offering);
		Assert.AreEqual("Spring Test Offering", offering.Description);
		Assert.IsTrue(offerings.IsSuccess);
		Assert.AreEqual(2, offerings.Value!.All.Count);
		Assert.IsNotNull(placement);
		Assert.AreEqual("default", placement.Identifier);
		Assert.IsTrue(products.IsSuccess);
		Assert.AreEqual(2, products.Value!.Count);
		Assert.AreEqual("coins_pack", products.Value[1].Identifier);
	}

	[TestMethod]
	public async Task PurchaseSubscriptionOption_ForwardsParametersOptionsAndParsesSuccess()
	{
		var context = new object();
		var platform = new RecordingPlatform
		{
			PurchaseSubscriptionOptionJson = ReadFixture("purchase_result_success.json")
		};
		var manager = CreateManager(platform);

		var result = await manager.PurchaseSubscriptionOptionWithOperationResultAsync(
		context,
		"pro_monthly",
		"base-monthly:intro",
		RevenueCatProductType.Subscription,
		new PurchaseOptions
		{
			OldProductIdentifier = "legacy_monthly",
			ReplacementMode = RevenueCatReplacementMode.Deferred,
			IsPersonalizedPrice = true,
			PresentedOfferingIdentifier = "default",
			StoreProductDiscountIdentifier = "winback-half-price",
			DiscountIdentifier = "intro-free-trial"
		});

		Assert.AreEqual(1, platform.PurchaseSubscriptionOptionRequests.Count);
		var request = platform.PurchaseSubscriptionOptionRequests[0];
		Assert.AreSame(context, request.Context);
		Assert.AreEqual("pro_monthly", request.ProductIdentifier);
		Assert.AreEqual("base-monthly:intro", request.SubscriptionOptionIdentifier);
		Assert.AreEqual("subs", request.Type);
		Assert.IsNotNull(request.PurchaseOptionsJson);
		using var document = JsonDocument.Parse(request.PurchaseOptionsJson);
		var root = document.RootElement;
		Assert.AreEqual("base-monthly:intro", root.GetProperty("subscription_option_id").GetString());
		Assert.AreEqual("legacy_monthly", root.GetProperty("old_product_identifier").GetString());
		Assert.AreEqual("Deferred", root.GetProperty("replacement_mode").GetString());
		Assert.IsTrue(root.GetProperty("is_personalized_price").GetBoolean());
		Assert.AreEqual("default", root.GetProperty("presented_offering_identifier").GetString());
		Assert.AreEqual("winback-half-price", root.GetProperty("store_product_discount_identifier").GetString());
		Assert.AreEqual("intro-free-trial", root.GetProperty("discount_identifier").GetString());

		Assert.IsTrue(result.IsSuccess);
		Assert.IsFalse(result.UserCancelled);
		Assert.AreEqual("txn_purchase_001", result.Value!.StoreTransaction!.TransactionIdentifier);
		Assert.IsTrue(result.Value.CustomerInfo!.Subscriber.Entitlements["premium"].IsActive.GetValueOrDefault());
	}

	[TestMethod]
	public async Task PurchaseSubscriptionOption_CancelledResult_ReturnsCancelledAndLegacyPurchaseResult()
	{
		var platform = new RecordingPlatform
		{
			PurchaseSubscriptionOptionJson = ReadFixture("purchase_result_cancelled.json")
		};
		var manager = CreateManager(platform);

		var result = await manager.PurchaseSubscriptionOptionWithOperationResultAsync(null, "pro_monthly", "base-monthly:intro");
		var legacy = await manager.PurchaseSubscriptionOptionAsync(null, "pro_monthly", "base-monthly:intro");

		Assert.AreEqual(2, platform.PurchaseSubscriptionOptionRequests.Count);
		Assert.IsFalse(result.IsSuccess);
		Assert.IsTrue(result.UserCancelled);
		Assert.AreEqual("PurchaseCancelledError", result.Error?.Code);
		Assert.IsNotNull(result.Value);
		Assert.IsTrue(result.Value.UserCancelled);
		Assert.IsNotNull(legacy);
		Assert.IsTrue(legacy.UserCancelled);
	}

	[TestMethod]
	public async Task CustomerInfoLifecycleApis_ForwardAndLegacyWrappersParseCustomerInfo()
	{
		var platform = new RecordingPlatform
		{
			AppUserId = "synthetic-user-001",
			IsAnonymous = false,
			CustomerInfoJson = ReadFixture("customer_info_full.json")
		};
		var manager = CreateManager(platform);

		var logOut = await manager.LogOutAsync();
		var restore = await manager.RestoreWithResultAsync();
		var sync = await manager.SyncPurchasesAsync();
		manager.InvalidateCustomerInfoCache();
		manager.InvalidateVirtualCurrenciesCache();

		Assert.AreEqual("synthetic-user-001", manager.AppUserId);
		Assert.IsFalse(manager.IsAnonymous);
		Assert.AreEqual(1, platform.LogOutCount);
		Assert.AreEqual(1, platform.RestoreCount);
		Assert.AreEqual(1, platform.SyncPurchasesCount);
		Assert.AreEqual(1, platform.InvalidateCustomerInfoCacheCount);
		Assert.AreEqual(1, platform.InvalidateVirtualCurrenciesCacheCount);
		Assert.IsNotNull(logOut);
		Assert.AreEqual("VERIFIED", logOut.VerificationResult);
		Assert.IsTrue(restore.IsSuccess);
		Assert.AreEqual("synthetic-user-001", restore.Value!.Subscriber.OriginalAppUserId);
		Assert.IsNotNull(sync);
		Assert.AreEqual("lifetime_access", sync.Subscriber.Entitlements["lifetime"].ProductIdentifier);
	}

	[TestMethod]
	public async Task AttributeAndAttributionMethods_ForwardValues()
	{
		var platform = new RecordingPlatform();
		var manager = CreateManager(platform);
		var attributes = new Dictionary<string, string>
		{
			["plan"] = "premium",
			["cohort"] = "spring"
		};

		manager.CollectDeviceIdentifiers();
		manager.SetEmail("person@example.test");
		manager.SetPhoneNumber(null);
		manager.SetPushToken("push-token-001");
		manager.SetDisplayName("Synthetic User");
		manager.SetMediaSource(null);
		manager.SetAd("ad-001");
		manager.SetAdGroup("ad-group-001");
		manager.SetCampaign("campaign-001");
		manager.SetCreative("creative-001");
		manager.SetKeyword("keyword-001");
		manager.SetAttribute("favorite_color", null);
		manager.SetAttributes(attributes);
		await manager.SyncOfferingsAndAttributesIfNeeded();
		var syncResult = await manager.SyncOfferingsAndAttributesIfNeededWithResult();

		Assert.AreEqual(1, platform.CollectDeviceIdentifiersCount);
		Assert.AreEqual("person@example.test", platform.Email);
		Assert.IsNull(platform.PhoneNumber);
		Assert.AreEqual("push-token-001", platform.PushToken);
		Assert.AreEqual("Synthetic User", platform.DisplayName);
		Assert.IsNull(platform.MediaSource);
		Assert.AreEqual("ad-001", platform.Ad);
		Assert.AreEqual("ad-group-001", platform.AdGroup);
		Assert.AreEqual("campaign-001", platform.Campaign);
		Assert.AreEqual("creative-001", platform.Creative);
		Assert.AreEqual("keyword-001", platform.Keyword);
		Assert.AreEqual("favorite_color", platform.AttributeKey);
		Assert.IsNull(platform.AttributeValue);
		Assert.IsNotNull(platform.Attributes);
		Assert.AreEqual("premium", platform.Attributes["plan"]);
		Assert.AreEqual("spring", platform.Attributes["cohort"]);
		Assert.AreEqual(2, platform.SyncOfferingsAndAttributesCount);
		Assert.IsTrue(syncResult.IsSuccess);
		Assert.IsTrue(syncResult.Value);
	}

	[TestMethod]
	public async Task CanMakePaymentsAndStorefront_ForwardAndExposeLegacyValues()
	{
		var context = new object();
		var platform = new RecordingPlatform
		{
			CanMakePaymentsValue = false,
			Storefront = StorefrontCountryCode()
		};
		var manager = CreateManager(platform);

		var canMakePayments = await manager.CanMakePaymentsWithResultAsync(context);
		var legacyCanMakePayments = await manager.CanMakePaymentsAsync(context);
		var storefront = await manager.GetStorefrontWithResultAsync();
		var legacyStorefront = await manager.GetStorefrontAsync();

		Assert.AreEqual(2, platform.CanMakePaymentsContexts.Count);
		Assert.AreSame(context, platform.CanMakePaymentsContexts[0]);
		Assert.AreSame(context, platform.CanMakePaymentsContexts[1]);
		Assert.IsTrue(canMakePayments.IsSuccess);
		Assert.IsFalse(canMakePayments.Value);
		Assert.IsFalse(legacyCanMakePayments);
		Assert.AreEqual(2, platform.GetStorefrontCount);
		Assert.IsTrue(storefront.IsSuccess);
		Assert.AreEqual("USA", storefront.Value);
		Assert.AreEqual("USA", legacyStorefront);
	}

	[TestMethod]
	public async Task NullableWrappers_OnErrorsAndNullResponses_ReturnNullOrEmptyDefaults()
	{
		var platform = new RecordingPlatform
		{
			OfferingJson = null,
			OfferingsJson = "{",
			ProductsJson = null,
			StorefrontException = new InvalidOperationException("storefront unavailable")
		};
		var manager = CreateManager(platform);

		var missingOffering = await manager.GetOfferingAsync("missing");
		var missingOfferingResult = await manager.GetOfferingWithResultAsync("missing");
		var invalidOfferings = await manager.GetOfferingsAsync();
		var products = await manager.GetProductsAsync(new[] { "missing" });
		var storefront = await manager.GetStorefrontAsync();
		var storefrontResult = await manager.GetStorefrontWithResultAsync();

		Assert.IsNull(missingOffering);
		Assert.IsFalse(missingOfferingResult.IsSuccess);
		Assert.AreEqual("empty_response", missingOfferingResult.Error?.Code);
		Assert.IsNull(invalidOfferings);
		Assert.AreEqual(0, products.Count);
		Assert.IsNull(storefront);
		Assert.IsFalse(storefrontResult.IsSuccess);
		Assert.AreEqual(nameof(InvalidOperationException), storefrontResult.Error?.Code);
	}

	static RevenueCatManager CreateManager(IRevenueCatPlatformImplementation platform)
	=> new(new RevenueCatOptions(null, null, null, null, false, null, null, null), platform);

	static string ReadFixture(string fileName)
	=> File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "revenuecat", fileName));

	static string CurrentOfferingJson()
	{
		var offerings = JsonSerializer.Deserialize(ReadFixture("offerings_full.json"), ModelSerializerContext.Default.Offerings);
		Assert.IsNotNull(offerings?.Current);
		return JsonSerializer.Serialize(offerings.Current, ModelSerializerContext.Default.Offering);
	}

	static string StorefrontCountryCode()
	{
		using var document = JsonDocument.Parse(ReadFixture("storefront.json"));
		return document.RootElement.GetProperty("country_code").GetString()!;
	}

	sealed class RecordingPlatform : IRevenueCatPlatformImplementation
	{
		public string? ApiKey { get; init; } = "synthetic-api-key";
		public string? AppUserId { get; init; } = "synthetic-user-001";
		public bool IsAnonymous { get; init; } = true;
		public string? CustomerInfoJson { get; set; }
		public string? OfferingJson { get; set; }
		public string? OfferingsJson { get; set; }
		public string? ProductsJson { get; set; }
		public string? PurchaseSubscriptionOptionJson { get; set; }
		public bool CanMakePaymentsValue { get; set; } = true;
		public string? Storefront { get; set; }
		public Exception? StorefrontException { get; set; }
		public List<string> OfferingIdentifiers { get; } = new();
		public List<string> PlacementIdentifiers { get; } = new();
		public List<(string ProductIdentifiersCsv, string? Type)> ProductRequests { get; } = new();
		public List<(object? Context, string ProductIdentifier, string SubscriptionOptionIdentifier, string? Type, string? PurchaseOptionsJson)> PurchaseSubscriptionOptionRequests { get; } = new();
		public List<object?> CanMakePaymentsContexts { get; } = new();
		public int LogOutCount { get; private set; }
		public int RestoreCount { get; private set; }
		public int SyncPurchasesCount { get; private set; }
		public int InvalidateCustomerInfoCacheCount { get; private set; }
		public int InvalidateVirtualCurrenciesCacheCount { get; private set; }
		public int CollectDeviceIdentifiersCount { get; private set; }
		public int SyncOfferingsAndAttributesCount { get; private set; }
		public int GetStorefrontCount { get; private set; }
		public string? Email { get; private set; }
		public string? PhoneNumber { get; private set; }
		public string? PushToken { get; private set; }
		public string? DisplayName { get; private set; }
		public string? MediaSource { get; private set; }
		public string? Ad { get; private set; }
		public string? AdGroup { get; private set; }
		public string? Campaign { get; private set; }
		public string? Creative { get; private set; }
		public string? Keyword { get; private set; }
		public string? AttributeKey { get; private set; }
		public string? AttributeValue { get; private set; }
		public IDictionary<string, string>? Attributes { get; private set; }

		public void SetCustomerInfoUpdatedHandler(Action<string> handler) { }
		public void Initialize(RevenueCatOptions options) { }
		public Task<string?> LoginAsync(string userId) => Task.FromResult(CustomerInfoJson);
		public Task<string?> LogOutAsync()
		{
			LogOutCount++;
			return Task.FromResult(CustomerInfoJson);
		}

		public Task<string?> GetCustomerInfoAsync(bool force) => Task.FromResult(CustomerInfoJson);
		public void InvalidateCustomerInfoCache() => InvalidateCustomerInfoCacheCount++;
		public Task<string?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier) => Task.FromResult(CustomerInfoJson);
		public Task<string?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, string? purchaseOptionsJson) => Task.FromResult(PurchaseSubscriptionOptionJson);
		public Task<string?> PurchaseProductAsync(object? platformContext, string productIdentifier, string? type, string? purchaseOptionsJson) => Task.FromResult(PurchaseSubscriptionOptionJson);

		public Task<string?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, string? type, string? purchaseOptionsJson)
		{
			PurchaseSubscriptionOptionRequests.Add((platformContext, productIdentifier, subscriptionOptionIdentifier, type, purchaseOptionsJson));
			return Task.FromResult(PurchaseSubscriptionOptionJson);
		}

		public Task<string?> GetOfferingAsync(string offeringIdentifier)
		{
			OfferingIdentifiers.Add(offeringIdentifier);
			return Task.FromResult(OfferingJson);
		}

		public Task<string?> GetOfferingsAsync() => Task.FromResult(OfferingsJson);

		public Task<string?> GetOfferingForPlacementAsync(string placementIdentifier)
		{
			PlacementIdentifiers.Add(placementIdentifier);
			return Task.FromResult(OfferingJson);
		}

		public Task<string?> GetProductsAsync(string productIdentifiersCsv, string? type)
		{
			ProductRequests.Add((productIdentifiersCsv, type));
			return Task.FromResult(ProductsJson);
		}

		public Task<string?> RestoreAsync()
		{
			RestoreCount++;
			return Task.FromResult(CustomerInfoJson);
		}

		public Task<string?> SyncPurchasesAsync()
		{
			SyncPurchasesCount++;
			return Task.FromResult(CustomerInfoJson);
		}

		public Task<bool> CanMakePaymentsAsync(object? platformContext)
		{
			CanMakePaymentsContexts.Add(platformContext);
			return Task.FromResult(CanMakePaymentsValue);
		}

		public Task<string?> GetStorefrontAsync()
		{
			GetStorefrontCount++;
			return StorefrontException is null
			? Task.FromResult(Storefront)
			: Task.FromException<string?>(StorefrontException);
		}

		public Task<string?> GetVirtualCurrenciesAsync() => Task.FromResult<string?>(null);
		public void InvalidateVirtualCurrenciesCache() => InvalidateVirtualCurrenciesCacheCount++;
		public Task<string?> RedeemWebPurchaseAsync(string redemptionLink) => Task.FromResult<string?>(null);
		public Task<string?> GetAmazonLwaConsentStatusAsync() => Task.FromResult<string?>(null);

		public Task SyncOfferingsAndAttributesIfNeeded()
		{
			SyncOfferingsAndAttributesCount++;
			return Task.CompletedTask;
		}

		public void CollectDeviceIdentifiers() => CollectDeviceIdentifiersCount++;
		public void SetEmail(string email) => Email = email;
		public void SetPhoneNumber(string? phoneNumber) => PhoneNumber = phoneNumber;
		public void SetPushToken(string? pushToken) => PushToken = pushToken;
		public void SetDisplayName(string displayName) => DisplayName = displayName;
		public void SetMediaSource(string? mediaSource) => MediaSource = mediaSource;
		public void SetAd(string ad) => Ad = ad;
		public void SetAdGroup(string adGroup) => AdGroup = adGroup;
		public void SetCampaign(string campaign) => Campaign = campaign;
		public void SetCreative(string creative) => Creative = creative;
		public void SetKeyword(string keyword) => Keyword = keyword;
		public void SetAttribute(string key, string? value)
		{
			AttributeKey = key;
			AttributeValue = value;
		}

		public void SetAttributes(IDictionary<string, string> attributes)
		=> Attributes = new Dictionary<string, string>(attributes);
	}
}
