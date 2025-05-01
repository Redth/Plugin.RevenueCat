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


