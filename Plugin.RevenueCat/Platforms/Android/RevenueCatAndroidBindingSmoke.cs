#nullable enable

using Android.App;

namespace Plugin.RevenueCat;

internal static class RevenueCatAndroidBindingSmoke
{
	// This method is intentionally not executed; building net10.0-android type-checks every selector the platform implementation uses.
	internal static void VerifySelectorsCompile()
	{
		var listener = new CustomerInfoUpdatedListener();
		var activity = default(Activity);
		var attributes = new Dictionary<string, string>
		{
			["key"] = "value"
		};

		_ = global::RevenueCat.RevenueCatManager.AppUserId;
		_ = global::RevenueCat.RevenueCatManager.IsAnonymous;
		global::RevenueCat.RevenueCatManager.SetCustomerInfoUpdatedListener(listener);
		global::RevenueCat.RevenueCatManager.Initialize(
			global::Android.App.Application.Context,
			false,
			"google",
			"api_key",
			"user_id",
			"https://example.invalid",
			"revenuecat",
			"informational",
			Java.Lang.Boolean.ValueOf(true),
			Java.Lang.Boolean.ValueOf(false),
			Java.Lang.Boolean.ValueOf(true));

		_ = global::RevenueCat.RevenueCatManager.Login("user_id");
		_ = global::RevenueCat.RevenueCatManager.Logout();
		_ = global::RevenueCat.RevenueCatManager.GetCustomerInfo(true);
		global::RevenueCat.RevenueCatManager.InvalidateCustomerInfoCache();
		_ = global::RevenueCat.RevenueCatManager.Restore();
		_ = global::RevenueCat.RevenueCatManager.SyncPurchases();
		_ = global::RevenueCat.RevenueCatManager.Purchase(activity, "offering", "package");
		_ = global::RevenueCat.RevenueCatManager.PurchaseWithResult(activity, "offering", "package", "{}");
		_ = global::RevenueCat.RevenueCatManager.PurchaseProduct(activity, "product", "subscription", "{}");
		_ = global::RevenueCat.RevenueCatManager.PurchaseSubscriptionOption(activity, "product", "base_plan", "subscription", "{}");
		_ = global::RevenueCat.RevenueCatManager.GetOffering("offering");
		_ = global::RevenueCat.RevenueCatManager.Offerings;
		_ = global::RevenueCat.RevenueCatManager.GetOfferingForPlacement("placement");
		_ = global::RevenueCat.RevenueCatManager.GetProducts("product_a,product_b", "subscription");
		_ = global::RevenueCat.RevenueCatManager.SyncAttributesAndOfferingsIfNeeded();
		_ = global::RevenueCat.RevenueCatManager.CanMakePayments(global::Android.App.Application.Context);
		_ = global::RevenueCat.RevenueCatManager.Storefront;
		_ = global::RevenueCat.RevenueCatManager.VirtualCurrencies;
		global::RevenueCat.RevenueCatManager.InvalidateVirtualCurrenciesCache();
		_ = global::RevenueCat.RevenueCatManager.RedeemWebPurchase("https://example.invalid/redeem");
		_ = global::RevenueCat.RevenueCatManager.AmazonLwaConsentStatus;
		global::RevenueCat.RevenueCatManager.CollectDeviceIdentifiers();
		global::RevenueCat.RevenueCatManager.SetEmail("user@example.invalid");
		global::RevenueCat.RevenueCatManager.SetPhoneNumber("+15555550100");
		global::RevenueCat.RevenueCatManager.SetPushToken("push_token");
		global::RevenueCat.RevenueCatManager.SetDisplayName("Display Name");
		global::RevenueCat.RevenueCatManager.SetMediaSource("media_source");
		global::RevenueCat.RevenueCatManager.SetAd("ad");
		global::RevenueCat.RevenueCatManager.SetAdGroup("ad_group");
		global::RevenueCat.RevenueCatManager.SetCampaign("campaign");
		global::RevenueCat.RevenueCatManager.SetCreative("creative");
		global::RevenueCat.RevenueCatManager.SetKeyword("keyword");
		global::RevenueCat.RevenueCatManager.SetAttribute("key", "value");
		global::RevenueCat.RevenueCatManager.SetAttributes(attributes);
	}

	sealed class CustomerInfoUpdatedListener : Java.Lang.Object, global::RevenueCat.ICustomerInfoUpdatedListener
	{
		public void OnCustomerInfoUpdated(string json)
		{
		}
	}
}
