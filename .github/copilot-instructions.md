# Repository Instructions for Plugin.RevenueCat

## Project Overview

This is a .NET MAUI plugin providing RevenueCat integration using a **slim binding approach**. The library binds RevenueCat's native iOS/macCatalyst and Android SDKs in a way that minimizes the binding surface area and simplifies maintenance.

## Architecture: Slim Bindings Approach

### Philosophy

Rather than creating complete 1:1 bindings of the native RevenueCat SDKs (which would be complex, brittle, and difficult to maintain), this project uses a "slim binding" approach:

1. **Native Wrapper Libraries**: We create small native library projects (Swift for iOS/macCatalyst, Java for Android) that expose a simplified API surface
2. **JSON-Based Data Transfer**: The native wrappers return JSON strings for complex data types, which are then deserialized into shared .NET models
3. **Unified Cross-Platform Models**: The `Plugin.RevenueCat.Core` project contains shared model classes (e.g., `CustomerInfo`, `Offering`, `Subscriber`) that work across all platforms
4. **Minimal Binding Surface**: The bound APIs only deal with simple types (strings, bools, callbacks) making the bindings trivial to maintain

### Key Benefits

- **Easy to Update**: When RevenueCat releases new SDK versions, updating is straightforward
- **Cross-Platform Consistency**: JSON serialization ensures iOS and Android return identical data structures
- **Reduced Complexity**: No need to bind hundreds of native types - just a handful of methods
- **Maintainability**: The native wrapper code is simple and easy to debug

## Project Structure

### Native Projects

- **iOS/macCatalyst**: `macios/native/RevenueCatBinding/` - Xcode project with Swift wrapper
  - `RevenueCatBinding.swift` - The slim binding wrapper that converts RevenueCat SDK calls to JSON responses
  
- **Android**: `android/native/revenuecatbinding/` - Gradle project with Java wrapper
  - `RevenueCatManager.java` - The slim binding wrapper that converts RevenueCat SDK calls to JSON responses

### Binding Projects

- **iOS/macCatalyst Binding**: `macios/RevenueCat.MaciOS.Binding/`
  - `RevenueCat.MaciOS.Binding.csproj` - References the Xcode project and native xcframework
  - `ApiDefinition.cs` - Objective-C binding definitions for the Swift wrapper
  
- **Android Binding**: `android/RevenueCat.Android.Binding/`
  - `RevenueCat.Android.Binding.csproj` - References the Gradle project and Maven dependencies

### Core Libraries

- **Plugin.RevenueCat.Core**: Shared models and converters for JSON deserialization
- **Plugin.RevenueCat**: Main MAUI plugin with `IRevenueCatManager` interface
- **Plugin.RevenueCat.Api**: REST API clients for RevenueCat Web API V1 and V2

### Web API Usage

Some operations are performed via RevenueCat's REST APIs rather than the native SDKs:

- `IRevenueCatApiV1` - V1 API for subscriber and offerings retrieval
- `IRevenueCatApiV2` - V2 API for customer management and attributes

Use these APIs when you need server-side operations or when the native SDK doesn't expose certain functionality.

---

## Updating Native SDK Versions

### Finding Latest Versions

1. **iOS/macCatalyst SDK**: Check [RevenueCat purchases-ios releases](https://github.com/RevenueCat/purchases-ios/releases)
2. **Android SDK**: Check [Maven Central for purchases](https://repo1.maven.org/maven2/com/revenuecat/purchases/purchases/) or [RevenueCat purchases-android releases](https://github.com/RevenueCat/purchases-android/releases)

### Updating iOS/macCatalyst Bindings

1. **Update the version** in `macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj`:
   ```xml
   <Target Name="DownloadNativeDependencies" ...>
       <PropertyGroup>
           <RevenueCatSdkVersion>5.48.0</RevenueCatSdkVersion>  <!-- Update this -->
       </PropertyGroup>
   </Target>
   ```

2. **Validate API changes** before proceeding:
   - Check RevenueCat iOS SDK release notes for breaking changes between versions
   - Review `macios/native/RevenueCatBinding/RevenueCatBinding/RevenueCatBinding.swift` to identify which SDK APIs are used
   - Verify those APIs haven't changed signatures or been removed
   - If wrapper uses changed APIs, update the Swift wrapper accordingly

3. **Update Xcode project references** if the RevenueCat SDK has API changes:
   - Open `macios/native/RevenueCatBinding/RevenueCatBinding.xcodeproj` in Xcode
   - Verify the RevenueCat package dependency version
   - Update `RevenueCatBinding.swift` if there are breaking API changes

4. **Download new dependencies**:
   ```bash
   dotnet build -m:1 -t:DownloadNativeDependencies ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj
   ```

5. **Update binding definitions** in `ApiDefinition.cs` only if the wrapper's `@objc` exposed interface changed (new methods, changed signatures, etc.)

### Updating Android Bindings

1. **Update the SDK version** in `android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj`:
   ```xml
   <PropertyGroup>
       <RevenueCatSdkVersion>9.5.0</RevenueCatSdkVersion>  <!-- Update this -->
   </PropertyGroup>
   ```

2. **Update the Gradle project** in `android/native/revenuecatbinding/build.gradle.kts`:
   ```kotlin
   dependencies {
       implementation("com.revenuecat.purchases:purchases:9.5.0")  // Update this
       implementation("com.revenuecat.purchases:purchases-store-amazon:9.5.0")  // Update this
   }
   ```

3. **Validate API changes** before proceeding:
   - Check RevenueCat Android SDK release notes for breaking changes between versions
   - Review `android/native/revenuecatbinding/src/main/java/.../RevenueCatManager.java` to identify which SDK APIs are used
   - Verify those APIs haven't changed signatures or been removed
   - If wrapper uses changed APIs, update the Java wrapper accordingly

4. **Re-evaluate the native dependency graph** (see section below)

5. **Update `RevenueCatManager.java`** if there are breaking API changes in the native SDK

### Re-evaluating the Android Dependency Graph

**Important**: The Android dependency graph can change significantly between RevenueCat SDK releases. You must verify and update dependencies when upgrading.

1. **Check for new transitive dependencies**:
   - Review RevenueCat's release notes for dependency changes
   - Run a Gradle dependency report: `./gradlew dependencies` in the `android/native/` directory

2. **Update `AndroidMavenLibrary` references** in `RevenueCat.Android.Binding.csproj`:
   ```xml
   <ItemGroup>
       <AndroidMavenLibrary Include="com.revenuecat.purchases:purchases" Version="$(RevenueCatSdkVersion)" ... />
       <AndroidMavenLibrary Include="com.revenuecat.purchases:purchases-store-amazon" Version="$(RevenueCatSdkVersion)" ... />
       <!-- Add/update other dependencies as needed -->
   </ItemGroup>
   ```

3. **Update NuGet package references** if Xamarin/AndroidX bindings need updates:
   ```xml
   <ItemGroup>
       <PackageReference Include="Xamarin.Android.Google.BillingClient" />
       <PackageReference Include="Xamarin.KotlinX.Serialization.Json" />
       <!-- etc. -->
   </ItemGroup>
   ```

4. **Update `AndroidIgnoredJavaDependency`** items for any dependencies that should be satisfied by NuGet packages rather than Maven

5. **Version alignment**: Ensure Kotlin, AndroidX, and Google Play Services versions are compatible across all dependencies. Check `Directory.packages.props` for centralized version management.

---

## Build Commands

```bash
# Build everything
dotnet build RevenueCat.sln

# Download iOS native dependencies
dotnet build -m:1 -t:DownloadNativeDependencies ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj

# Run tests
dotnet test ./Tests/Tests.csproj
```

## Testing

- Unit tests are in the `Tests/` directory
- JSON parsing tests validate that model deserialization works correctly across platforms
- Sample app in `sample/` can be used for manual testing

## Key Files to Know

| File | Purpose |
|------|---------|
| `macios/native/RevenueCatBinding/RevenueCatBinding/RevenueCatBinding.swift` | iOS/macCatalyst slim binding wrapper |
| `android/native/revenuecatbinding/src/main/java/.../RevenueCatManager.java` | Android slim binding wrapper |
| `Plugin.RevenueCat.Core/Models/` | Shared cross-platform data models |
| `Plugin.RevenueCat/IRevenueCatManager.cs` | Main plugin interface |
| `Plugin.RevenueCat/Platforms/` | Platform-specific implementations |
