# Plugin.RevenueCat Agent Guide

## Project summary

`Plugin.RevenueCat` is a .NET MAUI RevenueCat integration that uses a **slim binding** approach rather than maintaining large 1:1 bindings to the native iOS and Android SDKs.

The important design rule is: **keep the native binding surface minimal**. Push platform-specific work into the native wrapper libraries, return simple values and JSON payloads, and deserialize those into shared .NET models.

## Repository layout

| Path | Purpose |
| --- | --- |
| `Plugin.RevenueCat/` | Main MAUI-facing plugin and platform implementations |
| `Plugin.RevenueCat.Core/` | Shared models and JSON converters used across platforms |
| `Plugin.RevenueCat.Api/` | RevenueCat Web API v1/v2 clients |
| `android/RevenueCat.Android.Binding/` | Android binding project |
| `android/native/revenuecatbinding/` | Gradle/Java native wrapper for Android |
| `macios/RevenueCat.MaciOS.Binding/` | iOS/macCatalyst binding project |
| `macios/native/RevenueCatBinding/` | Xcode/Swift native wrapper for iOS/macCatalyst |
| `Tests/` | MSTest project for shared/API behavior |
| `sample/` | Sample app for manual verification |

## Architecture guidance

When making changes, prefer the existing repo pattern:

1. Keep the public .NET API small and cross-platform.
2. Put native SDK interaction details in:
   - `macios/native/RevenueCatBinding/RevenueCatBinding/RevenueCatBinding.swift`
   - `android/native/revenuecatbinding/src/main/java/com/revenuecat/revenuecatbinding/RevenueCatManager.java`
3. Return complex native data as JSON and deserialize it into shared models in `Plugin.RevenueCat.Core`.
4. Avoid introducing wide, brittle bindings to native SDK types unless there is a strong reason.

If a feature touches platform behavior, check whether it should be implemented for **both** Android and Apple platforms to keep the plugin consistent.

## Common commands

```bash
dotnet build RevenueCat.sln
dotnet test ./Tests/Tests.csproj
dotnet build -m:1 -t:DownloadNativeDependencies ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj
```

## Change guidelines

- Follow existing C# conventions: nullable enabled, `LangVersion=latest`, concise APIs, and shared models where possible.
- Add or update tests in `Tests/` when changing shared JSON models, API clients, or parsing behavior.
- Keep documentation in sync when the public API or setup flow changes.
- Do not add broad error swallowing; surface failures explicitly and follow current patterns.

## RevenueCat SDK updates

When upgrading native RevenueCat SDKs, update the wrapper projects and verify the APIs they call still exist:

- **iOS/macCatalyst**
  - Update the version in `macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj`
  - Verify the Xcode package dependency and `RevenueCatBinding.swift`
  - Re-run the native dependency download target

- **Android**
  - Update the version in `android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj`
  - Update the Gradle dependency in `android/native/revenuecatbinding/build.gradle.kts`
  - Re-check the Android dependency graph and any `AndroidMavenLibrary` / ignored dependency entries

## Key files to inspect first

- `Plugin.RevenueCat/IRevenueCatManager.cs`
- `Plugin.RevenueCat/Platforms/Android/RevenueCatAndroid.cs`
- `Plugin.RevenueCat/Platforms/iOS/RevenueCatApple.cs`
- `Plugin.RevenueCat/Platforms/MacCatalyst/RevenueCatApple.cs`
- `Plugin.RevenueCat.Core/Models/`
- `macios/native/RevenueCatBinding/RevenueCatBinding/RevenueCatBinding.swift`
- `android/native/revenuecatbinding/src/main/java/com/revenuecat/revenuecatbinding/RevenueCatManager.java`
- `README.md`
