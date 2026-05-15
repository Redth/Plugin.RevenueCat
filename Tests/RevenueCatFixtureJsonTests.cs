using System.Text.Json;
using Plugin.RevenueCat.Models;

namespace Tests;

[TestClass]
public sealed class RevenueCatFixtureJsonTests
{
	[TestMethod]
	public void CustomerInfo_FullFixture_DeserializesImportantFields()
	{
		var customerInfo = JsonSerializer.Deserialize(ReadFixture("customer_info_full.json"), ModelSerializerContext.Default.CustomerInfo);

		Assert.IsNotNull(customerInfo);
		Assert.AreEqual(Utc(2025, 1, 15, 12, 34, 56), customerInfo.RequestDate.GetValueOrDefault());
		Assert.AreEqual(1736944496000, customerInfo.RequestDateMs);
		Assert.AreEqual(3, customerInfo.SchemaVersion);
		Assert.AreEqual("VERIFIED", customerInfo.VerificationResult);
		Assert.AreEqual("https://apps.apple.com/account/subscriptions", customerInfo.ManagementUrl);
		Assert.AreEqual(2, customerInfo.ActiveSubscriptions.Count);
		Assert.AreEqual("pro_monthly", customerInfo.ActiveSubscriptions[0]);
		Assert.AreEqual("lifetime_access", customerInfo.ActiveSubscriptions[1]);
		Assert.AreEqual(3, customerInfo.AllPurchasedProductIdentifiers.Count);
		Assert.AreEqual(Utc(2025, 2, 15, 12, 34, 56), customerInfo.LatestExpirationDate.GetValueOrDefault());
		Assert.AreEqual(Utc(2025, 2, 15, 12, 34, 56), customerInfo.AllExpirationDatesByProduct["pro_monthly"].GetValueOrDefault());
		Assert.IsNull(customerInfo.AllExpirationDatesByProduct["lifetime_access"]);
		Assert.AreEqual(Utc(2024, 7, 4, 18, 0, 0), customerInfo.AllPurchaseDatesByProduct["tip_jar"].GetValueOrDefault());

		var subscriber = customerInfo.Subscriber;
		Assert.AreEqual("synthetic-user-001", subscriber.OriginalAppUserId);
		Assert.AreEqual("1.0-test", subscriber.OriginalApplicationVersion);
		Assert.AreEqual("https://play.google.com/store/account/subscriptions", subscriber.ManagementUrl);
		Assert.AreEqual(2, subscriber.Entitlements.Count);
		Assert.AreEqual(2, subscriber.Subscriptions.Count);

		var premium = subscriber.Entitlements["premium"];
		Assert.AreEqual("premium", premium.Identifier);
		Assert.AreEqual("pro_monthly", premium.ProductIdentifier);
		Assert.IsTrue(premium.IsActive.GetValueOrDefault());
		Assert.IsTrue(premium.WillRenew.GetValueOrDefault());
		Assert.AreEqual("play_store", premium.Store);
		Assert.AreEqual("base-monthly", premium.ProductPlanIdentifier);
		Assert.AreEqual(Utc(2025, 2, 15, 12, 34, 56), premium.ExpiresDate.GetValueOrDefault());

		var lifetime = subscriber.Entitlements["lifetime"];
		Assert.AreEqual("lifetime_access", lifetime.ProductIdentifier);
		Assert.IsTrue(lifetime.IsActive.GetValueOrDefault());
		Assert.IsFalse(lifetime.WillRenew.GetValueOrDefault());
		Assert.IsNull(lifetime.ExpiresDate);
		Assert.AreEqual("app_store", lifetime.Store);

		var monthly = subscriber.Subscriptions["pro_monthly"];
		Assert.AreEqual("Pro Monthly", monthly.DisplayName);
		Assert.AreEqual("GPA.synthetic.pro.monthly.001", monthly.StoreTransactionId);
		Assert.AreEqual(9.99d, monthly.Price!.Amount.GetValueOrDefault());
		Assert.AreEqual(9990000, monthly.Price.AmountMicros);
		Assert.AreEqual("$9.99", monthly.Price.Formatted);
		Assert.IsTrue(monthly.IsActive.GetValueOrDefault());
		Assert.IsTrue(monthly.WillRenew.GetValueOrDefault());

		var lifetimeSubscription = subscriber.Subscriptions["lifetime_access"];
		Assert.IsNull(lifetimeSubscription.ExpiresDate);
		Assert.AreEqual(49.99d, lifetimeSubscription.Price!.Amount.GetValueOrDefault());
		Assert.IsFalse(lifetimeSubscription.WillRenew.GetValueOrDefault());

		Assert.AreEqual(1, subscriber.NonSubscriptions["tip_jar"].Count);
		var tip = subscriber.NonSubscriptions["tip_jar"][0];
		Assert.AreEqual("Tip Jar", tip.DisplayName);
		Assert.AreEqual("txn_tip_001", tip.StoreTransactionId);
		Assert.AreEqual(1.99d, tip.Price!.Amount.GetValueOrDefault());
	}

	[TestMethod]
	public void Offerings_FullFixture_DeserializesProductsPackagesMetadataAndAndroidOptions()
	{
		var offerings = JsonSerializer.Deserialize(ReadFixture("offerings_full.json"), ModelSerializerContext.Default.Offerings);

		Assert.IsNotNull(offerings);
		Assert.AreEqual("default", offerings.CurrentOfferingId);
		Assert.AreEqual(2, offerings.All.Count);
		Assert.AreEqual(2, offerings.Items.Count);
		Assert.IsNotNull(offerings.Current);
		Assert.AreEqual("default", offerings.Current.Identifier);
		Assert.AreEqual("Spring Test Offering", offerings.Current.Description);
		Assert.AreEqual("spring-2025", offerings.Current.Metadata["experiment"].GetString());
		Assert.AreEqual(1, offerings.Current.Metadata["rank"].GetInt32());
		Assert.IsTrue(offerings.Current.Metadata["synthetic"].GetBoolean());
		Assert.AreEqual(2, offerings.Current.Packages.Count);

		var monthlyPackage = offerings.Current.Packages.Single(package => package.Identifier == "$rc_monthly");
		Assert.AreEqual("$rc_monthly", monthlyPackage.Id);
		Assert.AreEqual(PackageType.Monthly, monthlyPackage.PackageType);
		Assert.IsNotNull(monthlyPackage.StoreProduct);

		var product = monthlyPackage.StoreProduct;
		Assert.AreEqual("pro_monthly", product.Identifier);
		Assert.AreEqual("pro_monthly:base-monthly", product.Id);
		Assert.IsTrue(product.IsFamilyShareable);
		Assert.AreEqual("subscription", product.ProductType);
		Assert.AreEqual("subscription", product.ProductCategory);
		Assert.AreEqual("base-monthly", product.BasePlanId);
		Assert.AreEqual("$9.99", product.PriceString);
		Assert.AreEqual(9990000, product.Price!.AmountMicros);
		Assert.AreEqual("USD", product.Price.CurrencyCode);
		Assert.AreEqual(1, product.SubscriptionPeriod!.Value);
		Assert.AreEqual(SubscriptionPeriodUnit.Month, product.SubscriptionPeriod.Unit);
		Assert.AreEqual("intro-free-trial", product.IntroductoryDiscount!.Identifier);
		Assert.AreEqual("free_trial", product.IntroductoryDiscount.PaymentMode);
		Assert.AreEqual(7, product.IntroductoryDiscount.SubscriptionPeriod!.Value);
		Assert.AreEqual(1, product.Discounts.Count);
		Assert.AreEqual("winback-half-price", product.Discounts[0].Identifier);
		Assert.AreEqual(4.99d, product.Discounts[0].Price!.Amount.GetValueOrDefault());
		Assert.AreEqual(3, product.Discounts[0].NumberOfPeriods);
		Assert.AreEqual("base-monthly", product.DefaultSubscriptionOption!.Id);
		Assert.IsTrue(product.DefaultSubscriptionOption.IsBasePlan.GetValueOrDefault());

		Assert.AreEqual(1, product.SubscriptionOptions.Count);
		var introOption = product.SubscriptionOptions[0];
		Assert.AreEqual("base-monthly:intro", introOption.Id);
		Assert.AreEqual("token-intro-monthly", introOption.OfferToken);
		Assert.IsFalse(introOption.IsBasePlan.GetValueOrDefault());
		Assert.AreEqual(2, introOption.Tags.Count);
		Assert.AreEqual("intro", introOption.Tags[0]);
		Assert.AreEqual(2, introOption.PricingPhases.Count);
		Assert.AreEqual("FREE_TRIAL", introOption.PricingPhases[0].OfferPaymentMode);
		Assert.AreEqual(0d, introOption.PricingPhases[0].Price!.Amount.GetValueOrDefault());
		Assert.AreEqual("FULL_PRICE", introOption.PricingPhases[1].OfferPaymentMode);
		Assert.AreEqual(119.88d, introOption.PricingPhases[1].PricePerYear!.Amount.GetValueOrDefault());
		Assert.AreEqual("FREE_TRIAL", introOption.FreePhase!.OfferPaymentMode);
		Assert.AreEqual("FULL_PRICE", introOption.FullPricePhase!.OfferPaymentMode);
		Assert.AreEqual(3, introOption.InstallmentsInfo!.CommitmentPaymentsCount);
		Assert.AreEqual(1, introOption.InstallmentsInfo.RenewalCommitmentPaymentsCount);

		var lifetimePackage = offerings.Current.Packages.Single(package => package.Identifier == "$rc_lifetime");
		Assert.AreEqual(PackageType.Lifetime, lifetimePackage.PackageType);
		Assert.AreEqual("lifetime_access", lifetimePackage.StoreProduct!.Identifier);
		Assert.AreEqual("non_subscription", lifetimePackage.StoreProduct.ProductCategory);
	}

	[TestMethod]
	public void Products_FullFixture_DeserializesWithSourceGeneratedListContext()
	{
		var products = JsonSerializer.Deserialize(ReadFixture("products_full.json"), ModelSerializerContext.Default.ListStoreProduct);

		Assert.IsNotNull(products);
		Assert.AreEqual(2, products.Count);
		Assert.AreEqual("pro_monthly", products[0].Identifier);
		Assert.AreEqual("base-monthly", products[0].DefaultSubscriptionOption!.Id);
		Assert.AreEqual(2, products[0].SubscriptionOptions.Count);
		Assert.AreEqual("base-monthly:intro", products[0].SubscriptionOptions[1].Id);
		Assert.AreEqual(3, products[0].SubscriptionOptions[1].InstallmentsInfo!.CommitmentPaymentsCount);
		Assert.AreEqual("coins_pack", products[1].Identifier);
		Assert.AreEqual("inapp", products[1].ProductType);
		Assert.AreEqual(1.99d, products[1].Price!.Amount.GetValueOrDefault());
	}

	[TestMethod]
	public void PurchaseResult_SuccessFixture_DeserializesTransactionAndCustomerInfo()
	{
		var result = JsonSerializer.Deserialize(ReadFixture("purchase_result_success.json"), ModelSerializerContext.Default.PurchaseResult);

		Assert.IsNotNull(result);
		Assert.IsFalse(result.UserCancelled);
		Assert.IsNotNull(result.StoreTransaction);
		Assert.AreEqual("transaction-synthetic-001", result.StoreTransaction.Id);
		Assert.AreEqual("txn_purchase_001", result.StoreTransaction.TransactionIdentifier);
		Assert.AreEqual("pro_monthly", result.StoreTransaction.ProductIdentifier);
		Assert.AreEqual(2, result.StoreTransaction.ProductIdentifiers.Count);
		Assert.AreEqual("base-monthly:intro", result.StoreTransaction.ProductIdentifiers[1]);
		Assert.AreEqual(Utc(2025, 1, 15, 12, 35, 0), result.StoreTransaction.PurchaseDate.GetValueOrDefault());
		Assert.AreEqual(1, result.StoreTransaction.Quantity);
		Assert.AreEqual("play_store", result.StoreTransaction.Store);
		Assert.IsNotNull(result.CustomerInfo);
		Assert.AreEqual("VERIFIED", result.CustomerInfo.VerificationResult);
		Assert.IsTrue(result.CustomerInfo.Subscriber.Entitlements["premium"].IsActive.GetValueOrDefault());
	}

	[TestMethod]
	public void PurchaseResult_CancelledFixture_DeserializesCancellationAndExtensionData()
	{
		var result = JsonSerializer.Deserialize(ReadFixture("purchase_result_cancelled.json"), ModelSerializerContext.Default.PurchaseResult);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.UserCancelled);
		Assert.IsNull(result.StoreTransaction);
		Assert.IsNull(result.CustomerInfo);
		Assert.IsNotNull(result.ExtensionData);
		Assert.AreEqual("user_cancelled", result.ExtensionData["cancellation_reason"].GetString());
	}

	[TestMethod]
	public void VirtualCurrencies_Fixture_DeserializesBalances()
	{
		var virtualCurrencies = JsonSerializer.Deserialize(ReadFixture("virtual_currencies.json"), ModelSerializerContext.Default.VirtualCurrencies);

		Assert.IsNotNull(virtualCurrencies);
		Assert.AreEqual(2, virtualCurrencies.All.Count);
		Assert.AreEqual("Coins", virtualCurrencies.All["coins"].Name);
		Assert.AreEqual(125, virtualCurrencies.All["coins"].Balance);
		Assert.AreEqual("Spendable synthetic coins", virtualCurrencies.All["coins"].ServerDescription);
		Assert.AreEqual("gems", virtualCurrencies.All["gems"].Code);
		Assert.AreEqual(7, virtualCurrencies.All["gems"].Balance);
	}

	[TestMethod]
	[DataRow("web_redemption_success.json", "success", "s***@example.test", true)]
	[DataRow("web_redemption_expired.json", "expired", "e***@example.test", false)]
	[DataRow("web_redemption_purchase_belongs_to_other_user.json", "purchase_belongs_to_other_user", "o***@example.test", false)]
	public void WebRedemption_StatusFixtures_DeserializeExpectedStatus(string fixtureName, string expectedStatus, string expectedEmail, bool hasCustomerInfo)
	{
		var result = JsonSerializer.Deserialize(ReadFixture(fixtureName), ModelSerializerContext.Default.WebPurchaseRedemptionResult);

		Assert.IsNotNull(result);
		Assert.AreEqual(expectedStatus, result.Status);
		Assert.AreEqual(expectedEmail, result.ObfuscatedEmail);
		Assert.AreEqual(hasCustomerInfo, result.CustomerInfo is not null);
	}

	[TestMethod]
	public void Storefront_Fixture_HasExpectedSyntheticCountry()
	{
		using var document = JsonDocument.Parse(ReadFixture("storefront.json"));
		var root = document.RootElement;

		Assert.AreEqual("USA", root.GetProperty("country_code").GetString());
		Assert.AreEqual("app_store", root.GetProperty("store").GetString());
		Assert.AreEqual("synthetic", root.GetProperty("source").GetString());
	}

	[TestMethod]
	public void ModelSerializerContext_PublicManagerApiDtos_HasGeneratedMetadata()
	{
		Assert.IsNotNull(ModelSerializerContext.Default.CustomerInfo);
		Assert.IsNotNull(ModelSerializerContext.Default.Offering);
		Assert.IsNotNull(ModelSerializerContext.Default.Offerings);
		Assert.IsNotNull(ModelSerializerContext.Default.ListStoreProduct);
		Assert.IsNotNull(ModelSerializerContext.Default.StoreProduct);
		Assert.IsNotNull(ModelSerializerContext.Default.Package);
		Assert.IsNotNull(ModelSerializerContext.Default.Entitlement);
		Assert.IsNotNull(ModelSerializerContext.Default.Subscription);
		Assert.IsNotNull(ModelSerializerContext.Default.NonSubscription);
		Assert.IsNotNull(ModelSerializerContext.Default.StoreTransaction);
		Assert.IsNotNull(ModelSerializerContext.Default.PurchaseResult);
		Assert.IsNotNull(ModelSerializerContext.Default.PurchaseOptions);
		Assert.IsNotNull(ModelSerializerContext.Default.VirtualCurrencies);
		Assert.IsNotNull(ModelSerializerContext.Default.VirtualCurrency);
		Assert.IsNotNull(ModelSerializerContext.Default.WebPurchaseRedemptionResult);
		Assert.IsNotNull(ModelSerializerContext.Default.RevenueCatError);
		Assert.IsNotNull(ModelSerializerContext.Default.SubscriptionOption);
		Assert.IsNotNull(ModelSerializerContext.Default.PricingPhase);
		Assert.IsNotNull(ModelSerializerContext.Default.InstallmentsInfo);
		Assert.IsNotNull(ModelSerializerContext.Default.Price);
	}

	static string ReadFixture(string fileName)
	=> File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "revenuecat", fileName));

	static DateTimeOffset Utc(int year, int month, int day, int hour, int minute, int second)
	=> new(year, month, day, hour, minute, second, TimeSpan.Zero);
}
