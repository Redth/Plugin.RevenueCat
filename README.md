# RevenueCat for .NET MAUI
RevenueCat integration for .NET MAUI using a slim binding approach.

## About

### Why?
There are a few other community efforts that maintain more complete bindings to RevenueCat SDKs which are commendable efforts!  However, those bindings are complex, difficult to update and maintain, and don't all support all the platforms.

### How
This library takes a much lighter touch approach by creating native library projects which expose a much simpler API to .NET/C# (so it's very easy to bind) and attempts to do most of the heavy lifting in native platform code (java/swift).  The binding API surface only passes very simple types, and returns JSON strings which are deserialized in .NET back into more complex objects.

### What
Generally the idea behind RevenueCat is you mostly have to care about what 'Entitlements' your user has at any given time, so this library doesn't attempt to provide rich data and API's around transaction details, but focuses on the fundamental concept of always raising the `CustomerInfoUpdated` event with the latest `CustomerInfoRequest` instance.

Most methods return this `CustomerInfoRequest` object which contains the user's entitlements.  This includes calls to purchase, restore, login, etc.

### Purchase Types

My current main use case is subscriptions, so I have not done much work around consumables and one-off product purchases.  I'd be happy for any help to round out the API surface and fill in any gaps related to non-subscription purchase types.

## Usage

In your builder, call `.UseRevenueCat("API_KEY")` to register `IRevenueCatManager` with dependency injection and have the initialization methods invoked in the appropriate application lifecycle events:

```csharp
var builder = MauiApp.CreateBuilder();
builder
	.UseMauiApp<App>()
	/* ... */
	.UseRevenueCat(revenueCatApiKey);
```

You can optionally pass in additional parameters to this call:
 - `string? userId` if you know it at this point (you can call `LoginAsync` to set it later too)
 - `string? appStore` if you need to pass a non-default value (according to the RevenueCat SDK docs)
 - `bool debugLog` to enable more debug logging at hte native RevenueCat SDK level 
 - `Action<CustomerInfoRequest> customerInfoUpdatedCallback` to have a custom callback invoked when the customer info is updated or refreshed (resulting from an API call, or from the native platform stores raising events through the RevenueCat SDK).

### CustomerInfoRequest Updates

The `CustomerInfoRequest` object is the main model type which you will use in your app.  This type contains a list of 'Entitlements' and their details which belong to the given user.

There's an event `IRevenueCatManager.CustomerInfoUpdated(CustomerInfoUpdatedEventArgs)` which is called any time relevant methods are called on the native SDK, or any time the native SDK detects a change from the server or platform billing/store API's.

This means the event/callback is called _often_ and may be called more frequently than you expect it.  It's important to gracefully handle this in your code and use the invocations to maintain the state of entitlement and its impact to your app.

Typically you will want to check the entitlements you care about for your app whenever this event/callback is invoked, and adjust the features/state of your UI appropriately.


### Purchases

Use the `IRevenueCatManager` instance (resolved through dependency injection) to interact with the SDK.  This is basically a wrapper/abstraction around the underlying platform specific implementations of `IRevenueCatImpl` (which you do not need to worry about directly).

- `Initialize(...)` - this is called for you if you use the `UseRevenueCat()` builder method and calling it again manually has no effect
- `Task<CustomerInfoRequest?> LoginAsync(string userId)` once you know the user identifier for your purchases, you should call this.  You should avoid calling it unnecessarily as it can cause multiple user account objects to be created if you do.  You should also try to call it as early in the lifecycle of your app as possible.
- `Task<CustomerInfoRequest?> GetCustomerInfoAsync(bool force)` Call this to get the latest customer info.  If you use `false` for `force`, you may have a cached instance returned from the method.  Use `true` to force updating from the server.  The `CustomerInfoUpdated` event and any callback you specify in the initialization will also be called with the updated info regardless of the `force` value you set here.
- `Task<Offering?> GetOfferingAsync(string offeringIdentifier)` Gets the offering (and its packages/products) for a given identifier.  Use this to get the information you need to display to the user when deciding on a purchase, such as the packages, prices, etc.
- `Task<CustomerInfoRequest?> RestoreAsync()` - Restores any purchases and returns the customer info which will have the entitlements relevant to the user.
- `Task<CustomerInfoRequest?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)` - Initiates a purchase.  The `platformContext` should be an `Activity` (likely the current one) on Android, and is currently unused on iOS/MacCatalyst.  There's a version of this method you can call which infers the default 'current' activity of your MAUI app automatically.  You also need to pass the `offeringIdentifier` and the `packageIdentifier` the user wants to purchase (if you only have one package, pick the first one).
- `Task<CustomerInfoRequest?> PurchaseAsync(string offeringIdentifier, string packageIdentifier)` - Same as above, but Initiates a purchase.  The `platformContext` should be an `Activity` (likely the current one) on Android, and is currently unused on iOS/MacCatalyst.  There's a version of this method you can call which infers the default 'current' activity of your MAUI app automatically.  You also need to pass the `offeringIdentifier` and the `packageIdentifier` the user wants to purchase (if you only have one package, pick the first one).
- `Task<CustomerInfoRequest?> PurchaseAsync(string offeringIdentifier, string packageIdentifier)` - Same as above, but automatically infers the default 'current' activity of your MAUI app automatically.

