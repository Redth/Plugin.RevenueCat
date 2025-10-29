# Updating iOS/MacCatalyst RevenueCat SDK

This guide documents the process for updating the RevenueCat iOS/MacCatalyst SDK to a new version.

## Overview

The iOS/MacCatalyst binding wraps the native RevenueCat Swift SDK using the .NET XcodeProject build integration. The SDK version is specified in the binding project file and automatically downloaded during the build process.

## Prerequisites

- **macOS** is required (XcodeProject integration cannot run on other platforms)
- **Xcode** command-line tools must be installed
- **.NET 9.0** SDK with `ios` and `maccatalyst` workloads installed

## Update Process

### Phase 1: Preparation

1. **Identify the target version**
   - Check for the latest stable release at: https://github.com/RevenueCat/purchases-ios/releases
   - Look for releases NOT marked as pre-release or beta
   - Note the version number (e.g., `5.45.0`)

2. **Review release notes**
   - Read the release notes for the target version and all versions since your current version
   - Look for:
     - Breaking changes that might affect the Swift wrapper code
     - New APIs that might be useful to expose
     - Deprecations that need to be addressed
     - Bug fixes and improvements

3. **Verify download URL**
   - Confirm the xcframework is available at: `https://github.com/RevenueCat/purchases-ios/releases/download/{VERSION}/RevenueCat.xcframework.zip`
   - Replace `{VERSION}` with the target version number

### Phase 2: Update Version Reference

4. **Update the binding project file**
   - Open: `macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj`
   - Locate the `<RevenueCatSdkVersion>` property in the `DownloadNativeDependencies` target
   - Update it to the new version number

   Example:
   ```xml
   <RevenueCatSdkVersion>5.45.0</RevenueCatSdkVersion>
   ```

### Phase 3: Clean Previous Build Artifacts

5. **Remove cached dependencies**
   - Delete the directory: `macios/native/bin/deps/`
   - This ensures a fresh download of the new xcframework version
   - Old cached versions can cause build issues

### Phase 4: Download and Build

6. **Download new native dependencies**
   ```bash
   dotnet build -m:1 -t:DownloadNativeDependencies ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj
   ```

   This target will:
   - Download the `RevenueCat.xcframework.zip` from GitHub releases
   - Extract it to `macios/native/bin/deps/`
   - Remove unwanted platforms (watchOS, visionOS) to reduce package size
   - Strip code signatures
   - Update the xcframework's `Info.plist` to remove references to unwanted platforms

7. **Build the binding project**
   ```bash
   dotnet build -m:1 ./macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj
   ```

   This will:
   - Build the Swift wrapper code in `macios/native/RevenueCatBinding/`
   - Generate C# bindings from `ApiDefinition.cs` and `StructsAndEnums.cs`
   - Link the native RevenueCat xcframework

8. **Build the main plugin project**
   ```bash
   dotnet build -m:1 ./Plugin.RevenueCat/Plugin.RevenueCat.csproj
   ```

   This verifies the main plugin compiles with the updated binding.

### Phase 5: Testing

9. **Run automated tests**
   ```bash
   dotnet test ./Tests/Tests.csproj
   ```

   Ensure all existing tests pass with the new SDK version.

10. **Test with the sample app**
    ```bash
    dotnet build ./sample/MauiSample.csproj
    ```

    Run the sample app on:
    - iOS simulator
    - iOS physical device
    - macOS (MacCatalyst target)

    Verify core functionality:
    - SDK initialization with API key
    - `CustomerInfo` retrieval
    - `Offerings` and `Package` loading
    - Purchase flow (using sandbox/test accounts)
    - `CustomerInfoUpdated` event fires correctly
    - Restore purchases functionality
    - Error handling

### Phase 6: Review Native Wrapper (If Needed)

11. **Check for API changes**

    If the release notes indicate API changes, you may need to update the Swift wrapper:
    - Review: `macios/native/RevenueCatBinding/RevenueCatBinding/RevenueCatBinding.swift`
    - Update method signatures if the native SDK API changed
    - Add new functionality if useful APIs were added
    - Handle deprecations

12. **Update C# bindings (If Needed)**

    If you modified the Swift wrapper's exposed API:
    - Update: `macios/RevenueCat.MaciOS.Binding/ApiDefinition.cs`
    - Update: `macios/RevenueCat.MaciOS.Binding/StructsAndEnums.cs`
    - Rebuild and retest

### Phase 7: Verification

13. **Verify the update**
    - Check the xcframework version in `macios/native/bin/deps/RevenueCat/RevenueCat.xcframework/`
    - Confirm no build warnings related to the binding
    - Review binlog files if any errors occurred
    - Ensure package size is reasonable (watchOS/visionOS should be removed)

### Phase 8: Documentation

14. **Update documentation**
    - Update this file if the process has changed
    - Update `CLAUDE.md` if the SDK version is mentioned in examples
    - Update `README.md` if there are user-facing changes
    - Document any breaking changes or new features in release notes

### Phase 9: Commit and CI

15. **Commit changes**
    ```bash
    git add macios/RevenueCat.MaciOS.Binding/RevenueCat.MaciOS.Binding.csproj
    git commit -m "Update iOS/MacCatalyst RevenueCat SDK to {VERSION}"
    git push
    ```

    **Note**: Only commit the `.csproj` change. Do NOT commit the downloaded xcframework in `macios/native/bin/deps/` - it's downloaded during the build.

16. **Monitor CI pipeline**
    - Check the GitHub Actions workflow at: `.github/workflows/build.yaml`
    - Verify the build succeeds on `macos-15` with Xcode 16.0
    - Ensure all pack steps complete successfully
    - Check for any warnings or errors in the workflow output

## Important Notes

### Build Flags
- **Always use `-m:1`** - This flag limits build parallelism to prevent issues with native dependency downloads and builds
- Without this flag, you may encounter race conditions or file locking issues

### Platform Requirements
- These builds **must run on macOS** - the XcodeProject integration requires Xcode
- Attempting to build on Linux or Windows will fail

### Downloaded Artifacts
- The xcframework is downloaded to `macios/native/bin/deps/` during the build
- This directory should be in `.gitignore` - never commit it
- Each developer's machine and the CI server will download it independently

### Version Synchronization
- The iOS/MacCatalyst SDK version is independent from the Android SDK version
- Each platform can be updated separately
- See `UPDATE_REVENUECAT_ANDROID.md` for Android update procedures

### Troubleshooting

**Build fails with "framework not found"**
- Delete `macios/native/bin/deps/` and run the `DownloadNativeDependencies` target again

**"Code signature invalid" errors**
- The build process removes code signatures - if you see this error, check that the `RemoveDir` and `codesign --remove-signature` commands in the `.csproj` are executing

**Unwanted platforms (watchOS, visionOS) in the xcframework**
- Check that the `RevenueCatRemoveLibraryIdentifier` custom task is executing
- Verify the platform removal logic in the `DownloadNativeDependencies` target

**CI build fails but local build succeeds**
- Check that the CI Xcode version matches your local Xcode version
- Verify all SDK downloads are successful in the CI logs
- Ensure no local artifacts are being used that aren't available in CI

## Reference Links

- **iOS SDK Releases**: https://github.com/RevenueCat/purchases-ios/releases
- **iOS SDK Documentation**: https://docs.revenuecat.com/docs/ios
- **XcodeProject Build Integration**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/native-embedding
