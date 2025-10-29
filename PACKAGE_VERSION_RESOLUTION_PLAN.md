# Package Version Mismatch Resolution Plan

## Executive Summary

This document outlines a systematic approach to resolving NuGet package version mismatch warnings (NU1608) in the RevenueCat .NET MAUI plugin. These warnings occur when AndroidX binding packages have strict version constraints that conflict with transitive dependencies brought in by .NET MAUI and the RevenueCat Android SDK.

## Current Warnings

The following NU1608 warnings are currently present:

1. **Xamarin.AndroidX.Lifecycle.LiveData.Core 2.8.7.4** requires Xamarin.AndroidX.Lifecycle.Common (>= 2.8.7.4 && < 2.8.8) but version **2.9.1** was resolved
2. **Xamarin.AndroidX.Collection.Ktx 1.4.5.2** requires Xamarin.AndroidX.Collection.Jvm (>= 1.4.5.2 && < 1.4.6) but version **1.5.0.2** was resolved
3. **Xamarin.AndroidX.Lifecycle.Runtime.Ktx 2.8.7.2** requires Xamarin.AndroidX.Lifecycle.Runtime (>= 2.8.7.2 && < 2.8.8) but version **2.9.1** was resolved
4. **Xamarin.AndroidX.Fragment.Ktx 1.8.5.2** requires Xamarin.AndroidX.Fragment (>= 1.8.5.2 && < 1.8.6) but version **1.8.6.2** was resolved
5. **Xamarin.AndroidX.Activity.Ktx 1.9.3.2** requires Xamarin.AndroidX.Activity (>= 1.9.3.2 && < 1.9.4) but version **1.10.1.2** was resolved
6. **Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android 2.8.7.2** requires Xamarin.AndroidX.Lifecycle.Runtime (>= 2.8.7.2 && < 2.8.8) but version **2.9.1** was resolved

## Root Cause Analysis

### Dependency Chain

The version conflicts arise from the following dependency chain:

1. **Microsoft.Maui.Controls 9.0.51** (MAUI framework)
   - Brings in newer versions of AndroidX packages (e.g., Lifecycle.* 2.9.1, Collection.* 1.5.0.2)

2. **RevenueCat Android SDK 9.12.0** (Maven dependency)
   - Depends on older AndroidX library versions from Maven Central
   - Specifically: `androidx.lifecycle:lifecycle-*:2.5.0`, `androidx.activity:activity:1.2.3`

3. **Xamarin.AndroidX Binding Packages** (NuGet)
   - The "-Ktx" variants have strict upper bound constraints (e.g., `< 2.8.8`)
   - These constraints don't accommodate newer versions that MAUI requires

### Key Maven Dependencies from RevenueCat SDK

From `build.gradle.kts` dependency tree:
- `androidx.lifecycle:lifecycle-runtime:2.5.0`
- `androidx.lifecycle:lifecycle-common:2.5.0`
- `androidx.lifecycle:lifecycle-process:2.5.0`
- `androidx.activity:activity:1.2.3` → `androidx.fragment:fragment:1.1.0`
- `androidx.collection:collection:1.1.0`
- `com.android.billingclient:billing:8.0.0` (brings additional AndroidX dependencies)

### Key NuGet Dependencies

**Currently in Directory.packages.props:**
- `Microsoft.Maui.Controls` 9.0.51
- `Xamarin.AndroidX.Lifecycle.Process` 2.9.1
- `Xamarin.AndroidX.Collection.Jvm` 1.5.0.2

**Transitive from MAUI and other sources:**
- `Xamarin.AndroidX.Activity` 1.10.1.2
- `Xamarin.AndroidX.Fragment` 1.8.6.2
- `Xamarin.AndroidX.Lifecycle.Common` 2.9.1
- `Xamarin.AndroidX.Lifecycle.Runtime` 2.9.1
- `Xamarin.AndroidX.Collection.Jvm` 1.5.0.2

## Resolution Strategy

### Guiding Principles

1. **MAUI First**: The .NET MAUI framework version requirements take absolute priority
2. **NuGet Compatibility**: Ensure all NuGet AndroidX binding packages are compatible with each other
3. **Maven Alignment**: Adjust Maven dependencies only if they don't break RevenueCat SDK functionality
4. **Conservative Updates**: Only update packages when necessary to resolve conflicts

### Approach

#### Phase 1: Inventory and Analysis

**Goal**: Understand the complete dependency graph and identify compatible versions

1. **Document MAUI's AndroidX Requirements**
   ```bash
   # Create a temporary MAUI-only project
   dotnet new maui -n TempMauiProject -f net9.0
   cd TempMauiProject
   dotnet restore
   dotnet list package --include-transitive --framework net9.0-android
   ```

   This will show exactly which AndroidX packages MAUI requires and their versions.

2. **Check NuGet Package Availability**

   For each conflicting package family, search NuGet.org for available versions:
   - `Xamarin.AndroidX.Lifecycle.*` family
   - `Xamarin.AndroidX.Activity.*` family
   - `Xamarin.AndroidX.Fragment.*` family
   - `Xamarin.AndroidX.Collection.*` family

   Specifically look for versions of `-Ktx` and `-Android` packages that:
   - Are compatible with the base package versions MAUI requires
   - Have updated version constraints that don't conflict

3. **Check Maven Central for AndroidX Library Versions**

   For the AndroidX libraries used by RevenueCat SDK, check Maven Central:
   - `androidx.lifecycle:*`
   - `androidx.activity:*`
   - `androidx.fragment:*`
   - `androidx.collection:*`

   Identify which versions are compatible with:
   - BillingClient 8.0.0 (required by RevenueCat SDK)
   - The NuGet AndroidX bindings that MAUI requires

#### Phase 2: Version Resolution

**Goal**: Find the intersection of compatible versions across all three ecosystems

1. **Create a Version Compatibility Matrix**

   Build a table like this:

   | Package Family | MAUI Requires (NuGet) | Current Maven | Compatible Maven Range | Available NuGet Binding |
   |---------------|----------------------|---------------|------------------------|------------------------|
   | lifecycle-* | 2.9.1 | 2.5.0 | 2.5.0 - 2.9.x | ? |
   | activity | 1.10.1.2 | 1.2.3 | 1.2.3 - 1.10.x | ? |
   | fragment | 1.8.6.2 | 1.1.0 | 1.1.0 - 1.8.x | ? |
   | collection | 1.5.0.2 | 1.1.0 | 1.1.0 - 1.5.x | ? |

2. **Identify Update Candidates**

   For each package family:
   - If newer NuGet bindings exist with relaxed constraints → use them
   - If Maven packages need updating → verify they don't break RevenueCat SDK
   - Document any packages that cannot be aligned

#### Phase 3: Implementation

**Goal**: Apply the resolved versions systematically

**Option A: Update NuGet Binding Packages (Preferred)**

If newer versions of the `-Ktx` packages exist that support the newer base versions:

1. Add explicit package references to `Directory.packages.props`:
   ```xml
   <PackageVersion Include="Xamarin.AndroidX.Lifecycle.LiveData.Core" Version="2.9.x" />
   <PackageVersion Include="Xamarin.AndroidX.Collection.Ktx" Version="1.5.x" />
   <PackageVersion Include="Xamarin.AndroidX.Lifecycle.Runtime.Ktx" Version="2.9.x" />
   <PackageVersion Include="Xamarin.AndroidX.Fragment.Ktx" Version="1.8.x" />
   <PackageVersion Include="Xamarin.AndroidX.Activity.Ktx" Version="1.10.x" />
   <PackageVersion Include="Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android" Version="2.9.x" />
   ```

2. These should be the versions that:
   - Are compatible with the base packages MAUI brings in
   - Have updated version constraints

**Option B: Pin Base Package Versions**

If Option A packages don't exist, pin the base packages to versions the `-Ktx` packages can work with:

1. Add to `Directory.packages.props`:
   ```xml
   <!-- Pin to versions compatible with existing -Ktx packages -->
   <PackageVersion Include="Xamarin.AndroidX.Lifecycle.Common" Version="2.8.7.4" />
   <PackageVersion Include="Xamarin.AndroidX.Lifecycle.Runtime" Version="2.8.7.2" />
   <PackageVersion Include="Xamarin.AndroidX.Fragment" Version="1.8.5.2" />
   <PackageVersion Include="Xamarin.AndroidX.Activity" Version="1.9.3.2" />
   ```

   **Risk**: This may conflict with MAUI's requirements and cause runtime issues.

**Option C: Hybrid Approach**

1. Update `-Ktx` packages where newer versions exist
2. For packages without updates, add to `Directory.packages.props` with `NoWarn`:
   ```xml
   <PropertyGroup>
     <NoWarn>$(NoWarn);NU1608</NoWarn>
   </PropertyGroup>
   ```

   Then document:
   - Why these warnings are acceptable
   - Testing performed to verify no runtime issues
   - Plan to update when compatible packages become available

**Option D: Coordinate Maven Package Updates**

If the Maven dependencies can be safely updated:

1. Update `build.gradle.kts` to use newer AndroidX library versions:
   ```kotlin
   dependencies {
       // Align with MAUI's AndroidX versions
       implementation("androidx.lifecycle:lifecycle-runtime:2.9.0")
       implementation("androidx.lifecycle:lifecycle-common:2.9.0")
       implementation("androidx.activity:activity:1.10.0")
       implementation("androidx.fragment:fragment:1.8.6")
   }
   ```

2. Update `RevenueCat.Android.Binding.csproj` to ignore older versions:
   ```xml
   <AndroidIgnoredJavaDependency Include="androidx.lifecycle:lifecycle-common:2.5.0" />
   <AndroidIgnoredJavaDependency Include="androidx.lifecycle:lifecycle-runtime:2.5.0" />
   <AndroidIgnoredJavaDependency Include="androidx.activity:activity:1.2.3" />
   <AndroidIgnoredJavaDependency Include="androidx.fragment:fragment:1.1.0" />
   ```

   **Risk**: Must verify RevenueCat SDK works with newer AndroidX versions.

#### Phase 4: Verification

**Goal**: Ensure the resolution doesn't break functionality

1. **Build Verification**
   ```bash
   dotnet clean
   dotnet build RevenueCat.sln
   ```
   - Verify warnings are resolved
   - Ensure no new errors appear

2. **Runtime Testing**
   - Run the MauiSample app on Android
   - Test all RevenueCat functionality:
     - Initialize SDK
     - Fetch offerings
     - Purchase flows
     - Restore purchases
     - Check entitlements
   - Test on both Google Play and Amazon AppStore configurations

3. **Dependency Audit**
   ```bash
   # Verify no transitive dependency issues
   dotnet list package --include-transitive --framework net9.0-android

   # Check Maven dependencies
   cd android/native
   ./gradlew :revenuecatbinding:dependencies --configuration releaseRuntimeClasspath
   ```

4. **Integration Testing**
   - Create a fresh MAUI app
   - Add Plugin.RevenueCat reference
   - Verify no package conflicts in the consuming app

## Recommended Resolution Path

Based on the analysis, here's the recommended step-by-step resolution:

### Step 1: Research Latest NuGet Bindings

Check NuGet.org for the latest versions of these packages:
- `Xamarin.AndroidX.Lifecycle.LiveData.Core`
- `Xamarin.AndroidX.Collection.Ktx`
- `Xamarin.AndroidX.Lifecycle.Runtime.Ktx`
- `Xamarin.AndroidX.Fragment.Ktx`
- `Xamarin.AndroidX.Activity.Ktx`
- `Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android`

Look for versions released after the MAUI 9.0 timeframe that would support the 2.9.x Lifecycle and 1.5.x Collection versions.

### Step 2: Test with Updated Bindings

If compatible versions exist, update `Directory.packages.props`:
```xml
<!-- Add explicit versions of -Ktx packages compatible with MAUI's requirements -->
<PackageVersion Include="Xamarin.AndroidX.Lifecycle.LiveData.Core" Version="<latest-compatible>" />
<PackageVersion Include="Xamarin.AndroidX.Collection.Ktx" Version="<latest-compatible>" />
<PackageVersion Include="Xamarin.AndroidX.Lifecycle.Runtime.Ktx" Version="<latest-compatible>" />
<PackageVersion Include="Xamarin.AndroidX.Fragment.Ktx" Version="<latest-compatible>" />
<PackageVersion Include="Xamarin.AndroidX.Activity.Ktx" Version="<latest-compatible>" />
<PackageVersion Include="Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android" Version="<latest-compatible>" />
```

### Step 3: If Bindings Don't Exist, Suppress with Documentation

If compatible NuGet packages don't exist yet:

1. Add to `Directory.Build.props` (create if doesn't exist):
   ```xml
   <Project>
     <PropertyGroup>
       <!-- Suppress NU1608 for AndroidX version constraints -->
       <!-- These are warnings only - the newer versions are backward compatible -->
       <!-- Will be resolved when Microsoft releases updated Xamarin.AndroidX bindings -->
       <NoWarn>$(NoWarn);NU1608</NoWarn>
     </PropertyGroup>
   </Project>
   ```

2. Document in this file and in comments why suppression is acceptable
3. Add tests to verify runtime compatibility

### Step 4: Consider Maven Updates (Low Priority)

Only if necessary and after thorough testing:
- Update AndroidX library versions in `build.gradle.kts`
- Test extensively with RevenueCat SDK
- Document any behavior changes

## Monitoring and Maintenance

1. **Track Microsoft Releases**
   - Monitor Xamarin.AndroidX NuGet packages for updates
   - Check release notes for MAUI updates
   - Subscribe to .NET MAUI release notifications

2. **RevenueCat SDK Updates**
   - When updating RevenueCat SDK versions, re-evaluate AndroidX dependencies
   - Check if newer versions have updated AndroidX requirements

3. **Periodic Audits**
   - Run `dotnet list package --outdated` monthly
   - Review transitive dependencies quarterly
   - Re-run the verification process after major MAUI updates

## Risk Assessment

### Low Risk (Recommended)
- **Option A**: Update NuGet bindings to compatible versions
- **Option C**: Suppress warnings with thorough testing

**Rationale**: AndroidX libraries maintain backward compatibility within major versions. The newer versions (2.9.x) include the functionality that 2.8.x packages expect.

### Medium Risk
- **Option D**: Update Maven dependencies in Gradle

**Rationale**: RevenueCat SDK is tested with specific AndroidX versions. While newer versions should work, extensive testing is required.

### High Risk (Not Recommended)
- **Option B**: Pin base packages to older versions

**Rationale**: This conflicts with MAUI's requirements and may cause runtime issues or prevent access to MAUI features that depend on newer AndroidX functionality.

## Success Criteria

The resolution is successful when:

1. ✅ All NU1608 warnings are resolved or documented as acceptable
2. ✅ The solution builds without errors
3. ✅ All RevenueCat SDK functionality works on Android
4. ✅ No runtime crashes or unexpected behavior in the sample app
5. ✅ The plugin works correctly when consumed by external MAUI apps
6. ✅ Both Google Play and Amazon AppStore configurations work
7. ✅ No new dependency conflicts are introduced

## References

- [Microsoft Docs: .NET for Android error/warning codes](https://learn.microsoft.com/en-us/dotnet/android/messages/)
- [NuGet Warning NU1608](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1608)
- [AndroidX Releases](https://developer.android.com/jetpack/androidx/versions)
- [Maven Central: AndroidX Lifecycle](https://mvnrepository.com/artifact/androidx.lifecycle)
- [Xamarin.AndroidX NuGet Packages](https://www.nuget.org/packages?q=Xamarin.AndroidX)
- [RevenueCat Android SDK Releases](https://github.com/RevenueCat/purchases-android/releases)

## Appendix: Command Reference

### Useful Commands for Investigation

```bash
# List all NuGet packages with transitive dependencies
dotnet list <project>.csproj package --include-transitive

# Check for outdated packages
dotnet list package --outdated

# Show Maven dependency tree
cd android/native
./gradlew :revenuecatbinding:dependencies --configuration releaseRuntimeClasspath

# Restore with detailed logging
dotnet restore -v detailed

# Build with binary logs for analysis
dotnet build -bl

# Search NuGet for packages
dotnet package search Xamarin.AndroidX.Lifecycle --take 50
```

### Testing Commands

```bash
# Clean build
dotnet clean && dotnet build

# Run tests
dotnet test

# Pack for local testing
dotnet pack -c Release

# Test in a new MAUI app
dotnet new maui -n TestApp
cd TestApp
dotnet add package Plugin.RevenueCat --source /path/to/nupkg/folder
dotnet build
```
