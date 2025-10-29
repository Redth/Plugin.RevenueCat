# Updating Android RevenueCat SDK

This guide documents the process for updating the RevenueCat Android SDK to a new version.

## Overview

The Android binding wraps the native RevenueCat Kotlin SDK using the .NET AndroidGradleProject build integration. The SDK is pulled from Maven Central as an AAR dependency, with the version specified in both the binding project file and the Gradle build file.

## Prerequisites

- **.NET 9.0** SDK with `android` workload installed
- **Java Development Kit (JDK)** - Required for Gradle builds
- **Android SDK** - Android API 35 or higher for compilation (minimum runtime is API 24)
- Works on **macOS, Linux, or Windows**

## Update Process

### Phase 1: Preparation

1. **Identify the target version**
   - **Primary source**: Check GitHub releases at: https://github.com/RevenueCat/purchases-android/releases
   - **Authoritative source**: Verify on Maven Central at: https://mvnrepository.com/artifact/com.revenuecat.purchases/purchases
   - Look for releases NOT marked as pre-release or beta
   - Note the version number (e.g., `8.23.0`)

2. **Review release notes**
   - Read the release notes for the target version and all versions since your current version
   - Look for:
     - Breaking changes that might affect the Kotlin wrapper code
     - New APIs that might be useful to expose
     - Deprecations that need to be addressed
     - Bug fixes and improvements
     - Changes to dependencies (Kotlin, AndroidX, Billing Client, etc.)

3. **Verify Maven Central availability**
   - Confirm the version is published at: `https://repo1.maven.org/maven2/com/revenuecat/purchases/purchases/{VERSION}/`
   - Check that both `purchases` and `purchases-store-amazon` artifacts are available
   - The SDK is usually released on GitHub first, then published to Maven Central

### Phase 2: Update Version References

4. **Update the binding project file**
   - Open: `android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj`
   - Locate the `<RevenueCatSdkVersion>` property
   - Update it to the new version number

   Example:
   ```xml
   <RevenueCatSdkVersion>8.23.0</RevenueCatSdkVersion>
   ```

5. **Update AndroidMavenLibrary references**
   - In the same `.csproj` file, update the `Version` attributes for:
     - `com.revenuecat.purchases:purchases`
     - `com.revenuecat.purchases:purchases-store-amazon`

   Example:
   ```xml
   <AndroidMavenLibrary Include="com.revenuecat.purchases:purchases" Version="8.23.0" ... />
   <AndroidMavenLibrary Include="com.revenuecat.purchases:purchases-store-amazon" Version="8.23.0" ... />
   ```

   **Important**: These version numbers should match `<RevenueCatSdkVersion>`.

6. **Update the Gradle build file**
   - Open: `android/native/revenuecatbinding/build.gradle.kts`
   - Locate the `dependencies` block
   - Update the version numbers for:
     - `com.revenuecat.purchases:purchases`
     - `com.revenuecat.purchases:purchases-store-amazon`

   Example:
   ```kotlin
   implementation("com.revenuecat.purchases:purchases:8.23.0")
   implementation("com.revenuecat.purchases:purchases-store-amazon:8.23.0")
   ```

   **Important**: These must match the versions in the `.csproj` file.

### Phase 3: Clean Previous Build Artifacts

7. **Clean Gradle cache (optional but recommended)**
   ```bash
   cd android/native
   ./gradlew clean
   cd ../..
   ```

8. **Clean .NET build outputs**
   ```bash
   dotnet clean ./android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj
   ```

   Or manually delete:
   - `android/RevenueCat.Android.Binding/obj/`
   - `android/RevenueCat.Android.Binding/bin/`
   - `artifacts/grdl/rcbnd/` (Gradle output path)

### Phase 4: Build

9. **Test the Gradle build independently (optional)**
   ```bash
   cd android/native
   ./gradlew build
   cd ../..
   ```

   This verifies the Gradle project can resolve and build with the new SDK version before attempting the .NET binding.

10. **Build the binding project**
    ```bash
    dotnet build -m:1 ./android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj
    ```

    This will:
    - Run the Gradle build via AndroidGradleProject integration
    - Download the new SDK version from Maven Central
    - Build the Kotlin wrapper code in `android/native/revenuecatbinding/`
    - Generate C# bindings for the wrapper
    - Package the AAR and bindings

11. **Build the main plugin project**
    ```bash
    dotnet build -m:1 ./Plugin.RevenueCat/Plugin.RevenueCat.csproj
    ```

    This verifies the main plugin compiles with the updated binding.

### Phase 5: Testing

12. **Run automated tests**
    ```bash
    dotnet test ./Tests/Tests.csproj
    ```

    Ensure all existing tests pass with the new SDK version.

13. **Test with the sample app**
    ```bash
    dotnet build ./sample/MauiSample.csproj
    ```

    Run the sample app on:
    - Android emulator (Google Play Services enabled)
    - Android physical device
    - Amazon Fire device or emulator (to test Amazon AppStore support)

    Verify core functionality:
    - SDK initialization with API key
    - Amazon AppStore auto-detection (if applicable)
    - `CustomerInfo` retrieval
    - `Offerings` and `Package` loading
    - Purchase flow (using sandbox/test accounts)
    - `CustomerInfoUpdated` event fires correctly
    - Restore purchases functionality
    - Error handling

### Phase 6: Review Native Wrapper (If Needed)

14. **Check for API changes**

    If the release notes indicate API changes, you may need to update the Kotlin wrapper:
    - Review: `android/native/revenuecatbinding/src/main/kotlin/` (Kotlin wrapper code)
    - Update method signatures if the native SDK API changed
    - Add new functionality if useful APIs were added
    - Handle deprecations
    - Update serialization code if model classes changed

15. **Update dependency versions (If Needed)**

    If the new RevenueCat SDK requires updated dependencies:
    - Update Kotlin version in `android/native/gradle.properties` or `build.gradle.kts`
    - Update AndroidX library versions
    - Update Google Play Billing Client version
    - Update Kotlin serialization version

    Then update corresponding `AndroidIgnoredJavaDependency` items in the `.csproj` if needed.

### Phase 7: Verification

16. **Verify the update**
    - Check Gradle build logs to confirm the correct version was resolved
    - Confirm no build warnings related to the binding
    - Review binlog files if any errors occurred
    - Test that the AAR is properly packaged in the NuGet package
    - Verify all platforms (arm64-v8a, armeabi-v7a, x86, x86_64) are included

### Phase 8: Documentation

17. **Update documentation**
    - Update this file if the process has changed
    - Update `CLAUDE.md` if the SDK version is mentioned in examples
    - Update `README.md` if there are user-facing changes
    - Document any breaking changes or new features in release notes

### Phase 9: Commit and CI

18. **Commit changes**
    ```bash
    git add android/RevenueCat.Android.Binding/RevenueCat.Android.Binding.csproj
    git add android/native/revenuecatbinding/build.gradle.kts
    git commit -m "Update Android RevenueCat SDK to {VERSION}"
    git push
    ```

    **Note**: Only commit the project files. The AAR artifacts are downloaded from Maven Central during the build.

19. **Monitor CI pipeline**
    - Check the GitHub Actions workflow at: `.github/workflows/build.yaml`
    - Verify the Android binding build succeeds
    - Ensure all pack steps complete successfully
    - Check for any warnings or errors in the workflow output

## Important Notes

### Build Flags
- **Use `-m:1` for consistency** - This flag limits build parallelism and is recommended for native builds
- Gradle builds may work without this flag, but it ensures consistent behavior with iOS builds

### Platform Requirements
- Android bindings can be built on **any platform** (macOS, Linux, Windows)
- This is different from iOS bindings which require macOS
- Requires Java/JDK for Gradle execution

### Version Synchronization
- The `.csproj` version and `build.gradle.kts` version **must match**
- Mismatched versions will cause runtime issues or binding errors
- Always update both files together

### Maven Central vs GitHub Releases
- GitHub releases may appear before Maven Central publication
- Always verify the version is available on Maven Central before updating
- Maven Central is the authoritative source for actual availability

### Amazon AppStore Support
- The binding includes `purchases-store-amazon` for Amazon AppStore support
- Amazon's AppStore SDK is also included as a dependency
- Amazon support is automatically detected at runtime (no configuration needed)
- See: `Plugin.RevenueCat/Platforms/Android/RevenueCatAndroid.cs` for detection logic

### Dependency Management
- The binding uses `AndroidIgnoredJavaDependency` to prevent duplicate dependencies
- These are dependencies already provided by other NuGet packages (AndroidX, Kotlin, etc.)
- If you see dependency conflicts, you may need to add more items to this list
- Check the Gradle dependency tree: `./gradlew :revenuecatbinding:dependencies`

### Troubleshooting

**Gradle build fails with "Could not resolve dependency"**
- Verify the version exists on Maven Central
- Check your internet connection
- Clear Gradle cache: `rm -rf ~/.gradle/caches/`
- Try running `./gradlew build --refresh-dependencies`

**Binding generation errors**
- Check that the Gradle build succeeded
- Verify the AAR is in the correct output location
- Look for Kotlin/Java version mismatches
- Review the binlog for detailed error messages

**Runtime errors about missing classes or methods**
- Ensure `.csproj` and `build.gradle.kts` versions match exactly
- Verify all required dependencies are included or ignored appropriately
- Check for ProGuard/R8 issues if using release builds

**Version mismatch warnings**
- Double-check all three version locations:
  1. `<RevenueCatSdkVersion>` in `.csproj`
  2. `<AndroidMavenLibrary Version="...">` in `.csproj`
  3. `implementation("com.revenuecat.purchases:purchases:...")` in `build.gradle.kts`

**CI build succeeds but local build fails**
- Clear local caches (Gradle, NuGet, obj/bin directories)
- Verify your Java/JDK version matches CI
- Check Android SDK installation and environment variables

## Reference Links

- **Android SDK Releases**: https://github.com/RevenueCat/purchases-android/releases
- **Maven Central Repository**: https://mvnrepository.com/artifact/com.revenuecat.purchases/purchases
- **Android SDK Documentation**: https://docs.revenuecat.com/docs/android
- **AndroidGradleProject Integration**: https://learn.microsoft.com/en-us/dotnet/android/binding-libs/android-gradle-project
