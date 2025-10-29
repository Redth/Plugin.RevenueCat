# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a RevenueCat integration for .NET MAUI using a "slim binding" approach. Unlike other bindings that attempt to expose the entire RevenueCat SDK surface, this library creates simplified native library projects (Android/iOS/MacCatalyst) that expose a minimal API to .NET/C#, passing JSON strings between native and managed code.

The core design philosophy focuses on **Entitlements** - the library raises the `CustomerInfoUpdated` event frequently with the latest `CustomerInfo` instance, which contains the user's entitlements. Most methods return `CustomerInfo` objects.

## Architecture

### Project Structure

The solution consists of 7 projects organized in a layered architecture:

1. **Plugin.RevenueCat.Core** - Platform-agnostic models and JSON converters
   - Contains all model types (`CustomerInfo`, `Entitlement`, `Offering`, `Package`, etc.)
   - Pure .NET Standard library with no platform dependencies

2. **Plugin.RevenueCat.Api** - RevenueCat REST API client (uses Refit)
   - Optional REST API access (not required for SDK functionality)

3. **RevenueCat.Android.Binding** (`android/RevenueCat.Android.Binding/`)
   - Android native binding using AndroidGradleProject build integration
   - References the Android Gradle project in `android/native/`
   - The Gradle project (`android/native/revenuecatbinding/`) is a Kotlin library that wraps RevenueCat SDK and exposes a simplified JSON-based API
   - RevenueCat SDK version: 8.23.0 (see `.csproj`)

4. **RevenueCat.MaciOS.Binding** (`macios/RevenueCat.MaciOS.Binding/`)
   - iOS/MacCatalyst binding using XcodeProject build integration
   - References the Xcode project in `macios/native/RevenueCatBinding/`
   - The Xcode project is a Swift framework that wraps RevenueCat SDK
   - RevenueCat SDK version: 5.37.0 (downloaded in `DownloadNativeDependencies` target)
   - Uses a custom MSBuild task to remove unwanted platforms (watchOS, visionOS) from the xcframework

5. **Plugin.RevenueCat** - Main library package
   - Multi-targeted for: `net9.0`, `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`
   - Contains `RevenueCatManager` (the main public API)
   - Platform-specific implementations in `Platforms/` folders:
     - `RevenueCatAndroid` - Android implementation with Amazon AppStore detection
     - `RevenueCatApple` - iOS/MacCatalyst implementation
   - `HostExtensions.cs` provides `.UseRevenueCat()` builder extension

6. **MauiSample** (`sample/`) - Example MAUI application

7. **Tests** - MSTest project testing Core and API functionality

### Key Patterns

**Native Abstraction Layer**: Each platform has native code (Kotlin for Android, Swift for iOS/Mac) that:
- Wraps the native RevenueCat SDK
- Serializes complex objects to JSON strings
- Provides a simple callback-based API

**Platform Implementation Interface**: `IRevenueCatPlatformImplementation` defines the contract that platform-specific implementations must fulfill. All methods return `Task<string?>` (JSON) or void.

**Manager Pattern**: `RevenueCatManager` is the main entry point, which:
- Wraps the platform implementation
- Deserializes JSON responses into strongly-typed models
- Raises the `CustomerInfoUpdated` event
- Handles logging

## Build Commands

### Full Build (macOS only, builds all platforms)
```bash
# Download native iOS/Mac dependencies first (required before first build)
dotnet build -m:1 -t:DownloadNativeDependencies ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj

# Build all projects in order
dotnet build RevenueCat.sln
```

### Individual Project Builds
```bash
# Android binding (works on any platform)
dotnet build ./android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj

# iOS/Mac binding (macOS only)
dotnet build ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj

# Core library (any platform)
dotnet build ./Plugin.RevenueCat.Core/Plugin.RevenueCat.Core.csproj

# Main plugin (macOS only for full multi-targeting)
dotnet build ./Plugin.RevenueCat/Plugin.RevenueCat.csproj
```

### Pack for NuGet
```bash
# Pack all projects (same order as CI)
dotnet pack -m:1 -c Release ./android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj
dotnet pack -m:1 -c Release ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj
dotnet pack -m:1 -c Release ./Plugin.RevenueCat.Core/Plugin.RevenueCat.Core.csproj
dotnet pack -m:1 -c Release ./Plugin.RevenueCat.Api/Plugin.RevenueCat.Api.csproj
dotnet pack -m:1 -c Release ./Plugin.RevenueCat/Plugin.RevenueCat.csproj
```

Note: The `-m:1` flag limits parallelism to avoid build issues with native dependencies.

### Tests
```bash
# Run all tests
dotnet test ./Tests/Tests.csproj

# Run specific test
dotnet test ./Tests/Tests.csproj --filter "FullyQualifiedName~TestMethodName"
```

### Native Project Builds

#### Android (Gradle)
```bash
cd android/native
./gradlew build
```

#### iOS/Mac (Xcode)
The Xcode project builds automatically via the XcodeProject MSBuild integration. Manual build:
```bash
cd macios/native
xcodebuild -project RevenueCatBinding.xcodeproj -scheme RevenueCatBinding -configuration Release
```

## Development Workflow

### Updating RevenueCat SDK Versions

**Android**: Update the version in `android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj`:
```xml
<RevenueCatSdkVersion>8.23.0</RevenueCatSdkVersion>
```
Also update the Gradle dependency in `android/native/revenuecatbinding/build.gradle.kts`.

**iOS/Mac**: Update the version in `macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj`:
```xml
<RevenueCatSdkVersion>5.37.0</RevenueCatSdkVersion>
```

### Checking for SDK Updates

To check for new RevenueCat SDK versions:

**iOS/MacCatalyst**:
- Check releases at: https://github.com/RevenueCat/purchases-ios/releases
- Look for the latest stable release tag (e.g., `5.37.0`)
- The download URL format is: `https://github.com/RevenueCat/purchases-ios/releases/download/{version}/RevenueCat.xcframework.zip`

**Android**:
- Primary source: https://github.com/RevenueCat/purchases-android/releases
- Verify availability on Maven Central: https://mvnrepository.com/search?q=revenuecat
- Search specifically for: `com.revenuecat.purchases:purchases`
- The Maven repository is the authoritative source for actual published versions

When updating, ensure both the binding project `.csproj` file and the native project (Gradle or Xcode) reference the same version number.

### Working with Native Bindings

When modifying the native wrapper code:

1. **Android**: Edit Kotlin code in `android/native/revenuecatbinding/src/`
2. **iOS/Mac**: Edit Swift code in `macios/native/RevenueCatBinding/RevenueCatBinding/`
3. Ensure all complex objects are serialized to JSON before crossing the native/managed boundary
4. Update the binding definitions if new native types are exposed:
   - Android: Binding happens automatically via AndroidGradleProject
   - iOS/Mac: Update `ApiDefinition.cs` and `StructsAndEnums.cs`

### Amazon AppStore Support

The Android implementation automatically detects if the app is running from Amazon AppStore by checking:
- The installer package name (must start with "com.amazon")
- The device manufacturer (must be "amazon")

See `RevenueCatAndroid.IsAmazon()` in `Plugin.RevenueCat/Platforms/Android/RevenueCatAndroid.cs:34`

### CustomerInfo Event Handling

The `CustomerInfoUpdated` event fires frequently - whenever:
- Any SDK method is called that returns CustomerInfo
- The native SDK detects changes from the server
- Platform billing APIs report changes

Design your app to handle frequent invocations of this event gracefully.

## CI/CD

The GitHub Actions workflow (`.github/workflows/build.yaml`) runs on macOS-15 with Xcode 16.0 and:
1. Installs .NET workloads: `maui`, `android`, `ios`, `maccatalyst`
2. Downloads native iOS/Mac dependencies
3. Builds and packs all projects with binlogs
4. Publishes to NuGet on version tags (`v*`)

### Versioning

Uses Nerdbank.GitVersioning:
- Version configured in `version.json`
- Current base version: 0.3
- Public releases triggered by tags matching `^refs/tags/.*`

## Important Platform-Specific Notes

### Android
- Minimum SDK version: 24 (Android 7.0)
- Target framework: `net9.0-android35.0`
- Requires Activity context for purchases (auto-detected from MAUI app)
- Supports both Google Play and Amazon AppStore

### iOS
- Minimum version: 15.0
- Target framework: `net9.0-ios`
- No platform context required for purchases

### MacCatalyst
- Minimum version: 15.0
- Target framework: `net9.0-maccatalyst`
- Shares implementation with iOS (`RevenueCatApple.cs`)

## Configuration

Users configure via the builder extension:
```csharp
builder.UseRevenueCat(o => o
    .WithAndroidApiKey("goog_...")
    .WithAppleApiKey("appl_...")
    .WithAmazonApiKey("amzn_...") // Optional
    .WithDebug(true));
```

Platform-specific API keys are used based on the runtime platform.
