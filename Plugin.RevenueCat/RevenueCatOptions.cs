using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public record RevenueCatOptions(
	string? AndroidApiKey,
	string? AmazonApiKey,
	string? iOSApiKey,
	string? MacCatalystApiKey,
	bool Debug,
	string? UserId,
	string? AppStore,
	Action<CustomerInfo>? CustomerInfoUpdatedCallback)
{
	public string? ProxyUrl { get; init; }

	public RevenueCatPurchasesAreCompletedBy? PurchasesAreCompletedBy { get; init; }

	public RevenueCatEntitlementVerificationMode? EntitlementVerificationMode { get; init; }

	public bool? DiagnosticsEnabled { get; init; }

	public bool? AutomaticDeviceIdentifierCollectionEnabled { get; init; }

	public RevenueCatStoreKitVersion? StoreKitVersion { get; init; }

	public bool? PendingTransactionsForPrepaidPlansEnabled { get; init; }
}

public enum RevenueCatPurchasesAreCompletedBy
{
	RevenueCat,
	MyApp
}

public enum RevenueCatEntitlementVerificationMode
{
	Disabled,
	Informational
}

public enum RevenueCatStoreKitVersion
{
	StoreKit1 = 1,
	StoreKit2 = 2
}
