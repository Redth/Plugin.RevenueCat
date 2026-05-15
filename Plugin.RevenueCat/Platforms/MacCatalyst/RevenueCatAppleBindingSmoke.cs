#nullable enable

using Foundation;

namespace Plugin.RevenueCat;

internal static class RevenueCatAppleBindingSmoke
{
	// This method is intentionally not executed; building net10.0-maccatalyst type-checks every selector the platform implementation uses.
	internal static void VerifySelectorsCompile()
	{
		var manager = new global::RevenueCat.RevenueCatManager();
		var text = new NSString("value");

		_ = manager.AppUserID();
		_ = manager.IsAnonymous();
		manager.SetCustomerInfoChangedHandler(_ => { });
		manager.Initialize(
			false,
			"api_key",
			"user_id",
			"https://example.invalid",
			"revenuecat",
			"storekit2",
			"informational",
			NSNumber.FromBoolean(true),
			NSNumber.FromBoolean(false));

		_ = manager.LoginAsync("user_id");
		_ = manager.LogOutAsync();
		_ = manager.RestoreAsync();
		_ = manager.SyncPurchasesAsync();
		_ = manager.PurchaseAsync(text, text);
		_ = manager.PurchaseWithResultAsync(text, text, text);
		_ = manager.PurchaseProductAsync(text, text, text);
		_ = manager.GetOfferingAsync(text);
		_ = manager.GetOfferingsAsync();
		_ = manager.GetOfferingForPlacementAsync(text);
		_ = manager.GetProductsAsync(text, text);
		_ = manager.GetCustomerInfoAsync(true);
		manager.InvalidateCustomerInfoCache();
		_ = manager.SyncOfferingsAndAttributesIfNeededAsync();
		_ = manager.CanMakePayments();
		_ = manager.GetStorefrontAsync();
		_ = manager.GetVirtualCurrenciesAsync();
		manager.InvalidateVirtualCurrenciesCache();
		_ = manager.RedeemWebPurchaseAsync(text);
		manager.CollectDeviceIdentifiers();
		manager.SetEmail(text);
		manager.SetPhoneNumber(text);
		manager.SetPushToken(text);
		manager.SetDisplayName(text);
		manager.SetMediaSource(text);
		manager.SetAd(text);
		manager.SetAdGroup(text);
		manager.SetCampaign(text);
		manager.SetCreative(text);
		manager.SetKeyword(text);
		manager.SetAttribute(text, text);
		manager.SetAttributes(null!);
	}
}
