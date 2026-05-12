# RevenueCat for .NET MAUI
RevenueCat integration for .NET MAUI using a slim binding approach.

## About

### Why?
There are a few other community efforts that maintain more complete bindings to RevenueCat SDKs which are commendable efforts!  However, those bindings are complex, difficult to update and maintain, and don't all support all the platforms.

### How
This library takes a much lighter touch approach by creating native library projects which expose a much simpler API to .NET/C# (so it's very easy to bind) and attempts to do most of the heavy lifting in native platform code (java/swift).  The binding API surface only passes very simple types, and returns JSON strings which are deserialized in .NET back into more complex objects.

### What
Generally the idea behind RevenueCat is you mostly have to care about what 'Entitlements' your user has at any given time. This library focuses on keeping `CustomerInfoUpdated` aligned with the latest `CustomerInfo` instance while also exposing the non-UI catalog, identity, purchase result, attribution, and storefront helpers that most apps need.

Many methods return `CustomerInfo`, which contains the user's entitlements.  This includes calls to purchase, restore, login, logout, and customer-info refresh.

### Purchase Types

The plugin supports subscriptions through offerings/packages and can also retrieve and purchase products directly by product identifier. This is intended to cover one-time products, consumables, non-consumables, and products that are not modeled in a RevenueCat offering. RevenueCat paywall/UI APIs are intentionally out of scope for this package surface.

## Usage

In your builder, call `.UseRevenueCat(...)` to register `IRevenueCatManager` with dependency injection, set configuration options, and have the initialization methods invoked in the appropriate application lifecycle events:

```csharp
var builder = MauiApp.CreateBuilder();
builder
	.UseMauiApp<App>()
	/* ... */
	.UseRevenueCat(o => o
		.WithAndroidApiKey("goog_[YOUR_GOOGLE_CLIENT_KEY]")
		.WithAppleApiKey("appl_[YOUR_APPLE_CLIENT_KEY]")
		.WithDebug(true));
```

You can optionally set additional options:
 - `string? userId` if you know it at this point (you can call `LoginAsync` to set it later too)
 - `string? appStore` if you need to pass a non-default value (according to the RevenueCat SDK docs)
 - `bool debugLog` to enable more debug logging at the native RevenueCat SDK level
 - `WithProxyUrl(...)`, `WithPurchasesAreCompletedBy(...)`, `WithEntitlementVerificationMode(...)`, `WithDiagnosticsEnabled(...)`, `WithAutomaticDeviceIdentifierCollectionEnabled(...)`, `WithStoreKitVersion(...)`, and `WithPendingTransactionsForPrepaidPlansEnabled(...)` for advanced native SDK configuration.
 - `Action<CustomerInfo> customerInfoUpdatedCallback` to have a custom callback invoked when the customer info is updated or refreshed (resulting from an API call, or from the native platform stores raising events through the RevenueCat SDK).

### CustomerInfo Updates

The `CustomerInfo` object is the main model type which you will use in your app.  This type contains a list of 'Entitlements' and their details which belong to the given user.

There's an event `IRevenueCatManager.CustomerInfoUpdated(CustomerInfoUpdatedEventArgs)` which is called any time relevant methods are called on the native SDK, or any time the native SDK detects a change from the server or platform billing/store API's.

This means the event/callback is called _often_ and may be called more frequently than you expect it.  It's important to gracefully handle this in your code and use the invocations to maintain the state of entitlement and its impact to your app.

Typically you will want to check the entitlements you care about for your app whenever this event/callback is invoked, and adjust the features/state of your UI appropriately.


### Purchases

Use the `IRevenueCatManager` instance (resolved through dependency injection) to interact with the SDK.  This is basically a wrapper/abstraction around the underlying platform specific implementations of `IRevenueCatPlatformImplementation` (which you do not need to worry about directly).

- `Initialize(...)` - this is called for you if you use the `UseRevenueCat()` builder method and calling it again manually has no effect
- `Task<CustomerInfo?> LoginAsync(string userId)` once you know the user identifier for your purchases, you should call this.  Avoid calling it unnecessarily as it can cause multiple user account objects to be created if you do.  You should also try to call it as early in the lifecycle of your app as possible.
- `Task<CustomerInfo?> LogOutAsync()` logs out the current app user and returns the anonymous user's customer info.
- `string? AppUserId` and `bool IsAnonymous` expose the native SDK identity state after initialization.
- `Task<CustomerInfo?> GetCustomerInfoAsync(bool force)` gets the latest customer info.  If you use `false` for `force`, a cached instance may be returned.  Use `true` to force updating from the server.  The `CustomerInfoUpdated` event and any callback you specify in initialization will also be called with updated info.
- `void InvalidateCustomerInfoCache()` clears the native SDK customer-info cache.
- `Task<Offerings?> GetOfferingsAsync()`, `Task<Offering?> GetCurrentOfferingAsync()`, `Task<Offering?> GetOfferingAsync(string offeringIdentifier)`, and `Task<Offering?> GetOfferingForPlacementAsync(string placementIdentifier)` expose all/current/custom/placement RevenueCat offering lookup flows.
- `Task<IReadOnlyList<StoreProduct>> GetProductsAsync(IEnumerable<string> productIdentifiers, RevenueCatProductType? type = null)` retrieves products directly from the store through RevenueCat.
- `Task<CustomerInfo?> RestoreAsync()` restores purchases and returns the customer info with relevant entitlements.
- `Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)` initiates a package purchase and returns customer info for compatibility.
- `Task<PurchaseResult?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)` initiates a package purchase and preserves the store transaction plus user-cancelled flag.
- `Task<PurchaseResult?> PurchaseProductAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type = null)` purchases a product directly by identifier.
- `PurchaseOptions` overloads support advanced purchase parameters, including Android subscription-option/base-plan purchase, replacement mode and old product ID for upgrades/downgrades, personalized price indication, presented offering context, and iOS StoreProductDiscount promotional/win-back purchases by discount identifier.
- `Task<bool> CanMakePaymentsAsync(object? platformContext = null)` and `Task<string?> GetStorefrontAsync()` expose native payment/storefront helpers where supported.
- `Task<VirtualCurrencies?> GetVirtualCurrenciesAsync()` and `InvalidateVirtualCurrenciesCache()` expose RevenueCat virtual currency balances for consumables/credits scenarios.
- `Task<WebPurchaseRedemptionResult?> RedeemWebPurchaseAsync(string redemptionLink)` supports native SDK web purchase redemption flows; inspect `Status`, `CustomerInfo`, and `ObfuscatedEmail`.
- `Task<string?> GetAmazonLwaConsentStatusAsync()` exposes the Android Amazon LWA consent status when using the Amazon store.
- `CollectDeviceIdentifiers()`, `SetPhoneNumber(...)`, `SetPushToken(...)`, `SetMediaSource(...)`, and `SetAttribute(...)` cover RevenueCat attribution and reserved/custom attributes. Call device identifier collection only when appropriate for your app's privacy flow.

For non-throwing error handling, use the matching `*WithResultAsync` / `*WithOperationResultAsync` methods. These return `RevenueCatOperationResult<T>` with `IsSuccess`, `Value`, `Error`, and `UserCancelled` so you can inspect native RevenueCat error codes/messages and distinguish user-cancelled purchases while the existing nullable-returning methods remain source-compatible.

Configuration options are available through `RevenueCatOptionsBuilder` for explicit store selection (`WithAppStore("google" | "amazon" | "test")`), proxy URL, purchases-completed-by mode, entitlement verification mode, diagnostics, automatic device identifier collection, iOS StoreKit version, and Android pending prepaid-plan transactions.

### Android 15 / 16 KB page-size notes

This binding package does not currently ship native RevenueCat `.so` libraries on Android, so 16 KB Play Console warnings are usually caused by the consuming app's packaged native dependencies or Android build toolchain rather than the RevenueCat wrapper itself.

Current .NET for Android toolchains align Android packages for 16 KB pages by default. If warnings persist in a consuming app, update to a current .NET for Android / .NET MAUI workload, inspect the final APK or AAB, and identify which packaged native library needs an update. For APKs, Android's `zipalign` verification can check shared-library alignment:

```bash
zipalign -c -P 16 -v 4 path/to/app.apk
```
