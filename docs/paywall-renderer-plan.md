# RevenueCat MAUI paywall renderer plan

## Reassessment: 2026-09-26

**Verdict: feasible, but still an experimental renderer, not RevenueCatUI parity.**
The gallery is useful visual evidence for our handcrafted fixtures, not proof that arbitrary
dashboard paywalls are safe to publish with this renderer. The earlier "Yes" statuses and
completed-task history overstated coverage: the original rendering tests only exercised
localization, string substitution, and request selection, never MAUI views.

The local branch had been recreated from newer `main`, while PR #24 still contained the
older prototype. This refresh reconciles both histories, retaining the newer core APIs,
structured operation results, native serializers, and tests rather than overwriting them.

### Verified release baseline

| Layer | Selected version | Source / decision |
| --- | --- | --- |
| .NET SDK / workload set | 10.0.401 / 10.0.401 | [SDK release metadata](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json), [Apple workload release](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode26.5-10318) |
| .NET MAUI | 10.0.110 | [Stable release](https://github.com/dotnet/maui/releases/tag/10.0.110); MAUI 11 remains RC and is not the default |
| Apple build tools | Xcode 26.6 | Required by the stable Apple workload; do not silently use the machine's Xcode 27 preview/default |
| Microsoft.Extensions | 10.0.12 | Current .NET 10 servicing packages |
| SkiaSharp MAUI / Svg.Skia | 4.152.1 / 5.2.3 | Compatible .NET 10 assets, checked against NuGet dependency manifests |
| MSTest SDK | 4.4.1 | One SDK-managed test toolchain instead of mismatched 3.6.4/3.9.3 overrides |
| DevFlow Agent / CLI | 0.1.0-preview.12.26421.1 | Aligned Debug tooling; not a reason to switch the app to .NET 11 preview |
| RevenueCat native SDKs | iOS 5.91.0 / Android 10.23.2 | [iOS release](https://github.com/RevenueCat/purchases-ios/releases/tag/5.91.0), [Android release](https://github.com/RevenueCat/purchases-android/releases/tag/10.23.2); native tests, bindings and classic consumers validated after integration |

`MauiVersion` in `Directory.Build.props` also aligns implicit MAUI dependencies with the
central Controls package. `global.json` allows patch roll-forward, not an accidental
feature-band or prerelease migration.

### Material findings and corrections

| Priority | Finding | Refresh outcome |
| --- | --- | --- |
| P1 | Changing tabs could leave purchase targeting the previous tab's package | Tab selection reconciles the selected package with the visible tab and available store catalog; purchase regression test added |
| P1 | Unsupported web-checkout methods silently used in-app purchase | Explicit `ActionFailed` notification instead of changing checkout semantics; actual web checkout remains unsupported |
| P1 | Package selection rebuilt the entire view, resetting tab/scroll state and reloading images | Existing labels and selected borders update in place; the paywall root is retained |
| P1 | Clearing an offering left stale content; draft paywalls were automatically displayed | Null transitions clear the old definition; drafts require explicit `PaywallData` assignment |
| P1 | Async tap failures escaped the event handler | Handler failures are traced and surfaced through `ActionFailed`; duplicate taps on a pending action are gated |
| P2 | Carousel item source reused live views across recycled/looping cells | Templates now render independent views from component models |
| P2 | Timer cleanup relied only on handler disconnection | Loaded/unloaded lifecycle starts/stops timers; app-background behavior still needs device coverage |
| P2 | SVG cancellation could leak a parsed picture and detached SVGs did not reload | Unloaded views cancel/release; loaded views reload; stale/cancelled results dispose; parsing is moved off the UI thread |
| P2 | Locale lookup missed neutral languages and chose unrelated translations for missing keys | Normalized case/separator lookup with per-key language/default fallback, including localized images |
| P2 | Navigation passed `url_lid` itself as the destination | Resolve the localization before dispatching the host navigation event |
| P1 | CI never packed the paywall library or built the gallery | Add paywall packing and Release Android/Mac Catalyst gallery builds |
| P2 | Advertised Windows sample had no matching renderer target; Release included debug tooling | Match the Windows library target, host-conditional sample targets and Android minimum; keep DevFlow Debug-only in the gallery |

Managed view-tree tests compile the **same renderer/control source** against MAUI's
platform-neutral assemblies. They verify object identity, selected borders, package-local
prices, tab purchase routing, hidden/unavailable packages, null/draft handling, checkout
rejection, navigation, failures, duplicate taps, and carousel cell ownership. They are not
native handler, measurement, accessibility, GPU, store-purchase, or AOT tests.

### Revised delivery sequence

1. **Correctness baseline (this refresh):** reconcile main, update stable .NET/MAUI,
   correct selection/action/lifecycle defects, add managed view-tree regressions and CI
   packaging. Preserve the existing visual language; do not redesign fixtures to hide defects.
2. **P1 - Compatibility and purchase safety:** build a versioned corpus of real, sanitized
   dashboard payloads, with provenance and upstream revision; implement conditional overrides,
   offer eligibility, trial state, and subscription-change semantics. Unsupported behavior
   needs explicit diagnostics or rejection, not a claim of forward compatibility.
3. **P1 - State grammar:** evaluate the upstream `state_updates`, contextual values and
   override expressions before implementing them. Retaining unknown JSON does not execute
   those semantics. Integrate `web_view`/web checkout only behind an explicit host contract.
4. **P1 - Accessibility and lifecycle:** VoiceOver/TalkBack and keyboard roles/focus/state for
   custom tappable stacks and toggle; 44pt/48dp hit targets; large text and RTL; theme changes,
   rapid navigation, background/resume, timer termination and repeated SVG load/unload.
5. **P2 - Layout fidelity:** pin screenshots/measurements from native RevenueCatUI and compare
   phone/tablet, portrait/landscape, long translations, missing assets, and safe areas.
   Implement flex distribution, fill/fit/min/max constraints and font metrics before deciding
   whether a custom MAUI layout is actually necessary.
6. **P2 - Performance and completeness:** update countdown labels without rebuilding their
   stack each second; add bounded shared asset caching, remote fonts, proper localized period/
   trial variables, and supported video behavior. Measure allocations and dropped frames.
7. **Release gate:** retain native SDK/dependency compatibility checks on future upgrades;
   the current versions and AndroidX alignment are now integrated. Run live store sandbox
   purchases and restore, verify trimmed/AOT packages and all supported native targets.

No numeric native accessibility/adaptivity score is claimed from source or gallery captures
alone. The current custom controls, fixed-size fixtures and English period strings prevent a
production-ready conformance claim.

### Acceptance criteria for the next implementation increment

The native SDK, paywall semantics and accessibility workstreams must integrate against the
same refreshed baseline. Completing them requires more than three independent builds:

| Workstream | Required evidence |
| --- | --- |
| Native SDK compatibility | Pin verified stable releases; reconcile Maven/NuGet dependencies; compile Java/Swift wrappers and .NET bindings; retain the existing public API signatures, serialized field meanings, callback/error behavior and offering fallback rules; run the classic sample and compatibility tests |
| Conditional UI and state | Pin upstream grammar and precedence; test supported predicates, override ordering, state updates and variable resolution; distinguish unknown eligibility from eligible/ineligible; exercise changes to customer, selected package and available store products without inventing trial eligibility |
| Accessibility | Expose actionable roles, localized names and selected/checked/disabled states; prevent duplicate activation; provide keyboard/focus behavior and platform-sized hit targets; test the actual native properties where possible and label any screen-reader/device coverage that remains unverified |
| Combined behavior | A visible, eligible and available plan must be the same plan that is announced and passed to purchase; hiding/removing the selected plan must reconcile selection; changing context must update both UI and accessibility state without resetting the whole paywall |

Integration must retain the stable non-paywall usage path: an app using only
`Plugin.RevenueCat` must not require the paywall renderer, new eligibility context, or any
RevenueCatUI native library to initialize, fetch products, purchase, restore or receive
customer-info updates. New host context for paywalls must be additive and safe when omitted.

The combined regression pass must cover at least:

- A tab switch changes the announced selection, displayed price/trial text and purchase
  package together; inactive or unavailable plans cannot remain the purchase target.
- Unknown eligibility never becomes a promise of a free trial. Resolving eligibility or
  customer state updates affected content and semantics without replacing unrelated views.
- Pointer, keyboard and native accessibility activation each invoke one guarded action,
  with consistent disabled/busy/error behavior.
- Conditional visibility and state updates do not leave focus on removed content or retain
  listeners to discarded views; package selection preserves existing layout and scroll state.
- The legacy managed/native compatibility suite still passes after all three workstreams
  are combined, alongside paywall tests, platform builds and package checks.

The consolidated plan must distinguish shipped behavior, test-only evidence, unverified
native behavior and deliberately unsupported syntax. In particular, no live-store,
VoiceOver/TalkBack or full RevenueCatUI-parity claim follows from a managed test alone.

### Initial refresh validation, before parallel implementation

- **82 offline managed tests passed**, including construction of all 15 gallery fixtures as
  MAUI view trees. These tests do not require RevenueCat credentials.
- Debug and Release Mac Catalyst gallery builds passed with Xcode 26.6.
- Release Android gallery build passed with profiled AOT enabled. This machine's SDK is under
  a path containing spaces; its Android assembler launcher mishandles that path. Validation
  used the supported `AndroidBinUtilsDirectory` property pointing at a space-free symlink to
  the same toolchain's `bin` directory. No installed SDK scripts were edited and AOT was not disabled.
- `dotnet pack` produced the paywall package with `net10.0`, Android, iOS and Mac Catalyst
  assemblies. The Windows source target is not yet part of the macOS CI-produced package;
  Windows distribution needs a Windows build/pack job before advertising NuGet support.
- Live Mac Catalyst interactions retained the package selector's root object across changes
  and dispatched `$rc_monthly` after switching Tabbed plans from Annual to Monthly. Tabbed
  plans and the SVG-backed Dark gradient surface were inspected under the updated libraries.
- iOS/Android device interaction, Windows UI, accessibility, background/resume and live
  RevenueCat/store sandbox flows were **not** validated in this pass. Existing local NuGet
  source warnings and core-binding AndroidX constraint warnings are not claimed to be fixed.

### Integrated native and semantic work

The native upgrade preserves the previous public API signatures and public-SDK wrapper
mechanisms. Android dependencies now include the required Tink 1.10.0 runtime and aligned
Kotlin/Coroutines/AndroidX packages; the earlier NU1608 constraint warnings are gone from the
integrated classic sample builds. See the README for the exact graph and the separately
documented XAJDV7004 central-package-management verifier limitation.

The optional native introductory-eligibility interface is additive. Existing applications
and implementations of `IRevenueCatManager` do not need to implement or use it. iOS and Mac
Catalyst query the public SDK; Android returns an explicit unsupported-platform result.
Cancellation and customer changes cannot turn into successful eligibility facts.

The semantic engine now supports a bounded, source-verified subset:

| Area | Implemented behavior | Remaining boundary |
| --- | --- | --- |
| Overrides | Ordered matching overrides on text, stacks and tabs; conditions within an entry are ANDed; later matching non-null values win | Not a complete style/layout override engine |
| Conditions | Selected state, compact/medium/expanded, intro/promo eligibility, typed variable/state equality, selected-package membership, window comparisons | Window/screen facts are host-provided; arbitrary expressions are not evaluated |
| Eligibility | Explicit unknown/eligible/ineligible/no-intro states keyed by product and subscription option | Offer metadata alone does not prove eligibility; hosts invalidate customer-scoped context |
| State | Typed declarations and tab-driven literal/`$value` assignments | Other state-producing controls and advanced operations remain unsupported |
| Refresh | Existing text, visibility, container styles, dimensions and package availability update in place; selected purchase target is reconciled | Broader native focus/background/device coverage remains required |
| Offer copy | Explicitly eligible intro price/duration, including known billing-cycle counts | Missing cycles or multi-phase intro durations are not fabricated; regionalized copy remains incomplete |
| Unsupported input | Diagnostics and paywall-wide rule fallback for unsupported conditions/unmodeled overrides | `web_view`, dynamic media overrides, multiple intro offers and non-in-app checkout are not implemented |

Source provenance is pinned to RevenueCat iOS **5.91.0**:
`ComponentOverrides.swift`, `StateDeclaration.swift`, `StateUpdate.swift`,
`PaywallComponentBase.swift`, `PaywallTabsComponent.swift`, and the V2
`PresentedPartials`, `TabsComponentViewModel` and `PaywallStateStore` implementations;
Android **10.23.2** `StateUpdate.kt` is an additional cross-check.
`Tests/data/paywall_semantics.json` is a small sanitized derived fixture, not a claim that
every dashboard-exported paywall is supported.

Integration regressions specifically cover boolean inequality, matching/unmatching size
overrides, parent-assigned row alignment, badged container padding, exact product/offer
identity and native-to-paywall enum mapping. These are behavioral checks beyond the original
gallery parsing tests.

### Integrated accessibility work

The visual paywall still uses authored MAUI stacks, borders and the custom toggle track. A
transparent native activation button supplies focus, keyboard activation and accessibility
semantics, with a 44pt minimum on Apple platforms and 48dp on Android. Register it through
`UseRevenueCatPaywalls()`; Android uses a paywall-specific button handler for radio/switch
checked state rather than changing global MAUI button behavior.

- Android package nodes expose `RadioButton` semantics; the tab toggle exposes `Switch`;
  purchase/navigation actions remain buttons. Apple buttons expose selected/disabled traits
  and localized on/off or busy values.
- Names come from visible localized content. Conditionally hidden trial copy is excluded,
  and name/selected/enabled state follows semantic and catalog updates. Built-in fallback
  labels/hints currently cover English, French and Spanish, not every supported store locale.
- Decorative content is removed from the accessibility-important tree without deleting the
  authored visuals. Async activation shares one guarded command for native and managed test
  paths. Hidden/disabled actions cannot execute; busy actions disable the other surfaces.
- Real tab swaps respect system reduced-motion settings on iOS/Mac Catalyst and Android.
  Selection changes still do not fade or replace the whole paywall.
- Native action surfaces unregister during unloading/handler teardown. Integration exposed
  and fixed an Android crash caused by busy-state updates touching a disposed button from a
  previous tab. Repeated native touch and Tab/Enter switching was rerun after the correction.
- Conditional padding updates target the visual content, not both the outer action container
  and inner decoration; adding accessibility must not double the authored spacing.

**Native evidence:** the integrated app on the dedicated Android API 35 emulator exposed
radio/button/switch roles, checked/selected/enabled state and no duplicate package text in the
compressed accessibility tree. Android Tab focused the switch, Enter changed its state, and
six subsequent native toggles plus a mock purchase preserved the matching monthly package
without a process crash. The custom track's appearance was inspected.

**Not certified:** TalkBack/VoiceOver spoken output, desktop keyboard/focus behavior,
large-text/RTL coverage and Windows accessibility have not been comprehensively exercised.
Mac direct inspection was blocked by missing Accessibility/Screen Recording grants, which
were not bypassed. Focus continuity when whole tab controls are replaced remains a follow-up;
native roles and managed tests alone are not a full accessibility audit.

### Final combined validation

- **117 offline managed tests passed**, including new cross-workstream coverage for native
  eligibility mapping, conditional accessibility names/visibility/disabled state, restored
  layout constraints and visual padding.
- All **2,050 baseline public/protected API signatures and constraints** from the managed
  Core/API/plugin/paywall assemblies remain present. New capabilities are additive.
- Final Release Android gallery (profiled AOT enabled) and Release Mac Catalyst gallery builds
  passed. Classic Android and Mac Catalyst sample consumers also built successfully after all
  workstreams were combined, with no AndroidX NU1608 version-constraint warnings.
- The final paywall package compiled its `net10.0`, Android, iOS and Mac Catalyst assemblies.
  Native workstream evidence additionally includes Java tests/assembly builds, Swift XCTest
  status/selector coverage, and the iOS plugin build.
- Native Android inspection of the **combined** implementation confirmed package radio-button
  checked state, switch state, 48dp hit targets, deduplicated visible text and matching mock
  purchase routing. The keyboard/lifecycle crash was reproduced and then verified absent with
  native Tab/Enter activation followed by six repeated touch toggles.

These results do not replace live App Store/Play sandbox purchases, spoken screen-reader
testing, or broad accessibility conformance verification. Those explicit release gates and
the SDK dependency-verifier limitation remain documented rather than marked as passed.

## Goal

RevenueCat Paywalls V2 are remote UI definitions. The native RevenueCatUI SDKs render those definitions with platform-native UI frameworks, but binding those UI SDKs would significantly increase dependency and maintenance complexity, especially on Android. This repository can instead parse the paywall component payload and render a useful subset with .NET MAUI controls.

The initial goal is an experimental MAUI renderer that can be placed in a `ContentPage` and render a dashboard-created paywall made from common V2 components. Purchase, restore, dismiss, and navigation actions stay host-driven so apps can decide whether to call the existing slim RevenueCat bindings, a server flow, or a mocked handler in samples/tests.

## Data acquisition paths

There are two relevant API shapes:

1. **Documented V2 Paywall API**
   - `GET /v2/projects/{project_id}/paywalls`
   - `GET /v2/projects/{project_id}/paywalls/{paywall_id}?expand=components`
   - This is the canonical OpenAPI-documented Paywall envelope.
   - The `components_config` payload is intentionally schemaless JSON in OpenAPI.
   - These endpoints require project-configuration API permissions, so they are useful for tooling/configuration workflows but are not necessarily a public client-runtime fetch path.

2. **Runtime Offerings API used by SDKs**
   - `GET /v1/subscribers/{app_user_id}/offerings`
   - Uses the public app API key and platform/locale/storefront headers.
   - Native SDK response models include `paywall_components`, `draft_paywall_components`, and `ui_config`.
   - This path is customer-targeted and closer to what app runtime paywalls need.

The renderer should not depend on one acquisition strategy. It should accept parsed component data from either source.

Current runtime support:

- `IRevenueCatManager.AppUserId` exposes the active SDK app user ID, including SDK-generated anonymous IDs. The prototype's `GetAppUserIdAsync()` delegates to that existing property rather than adding a second native API.
- `IRevenueCatApiV1.GetPaywallOfferings(...)` fetches the SDK runtime Offerings endpoint and deserializes the full paywall-aware payload into `PaywallOfferingsResponse`.
- `Offering` and `Package` core models include additional paywall/runtime fields such as metadata, web checkout URLs, legacy `paywall`, `paywall_components`, `draft_paywall_components`, and `ui_config` when those fields are present.
- The reconciled native wrappers retain main's public SDK serialization and purchase behavior, without the prototype's Apple `@_spi(Internal)` paywall dependency. Use the direct V1 runtime Offerings API for the complete UI definition and top-level `ui_config`; combine it with store products obtained through the core SDK. Native-only offering retrieval is not advertised as a complete paywall source.

## OpenAPI vs implementation schema

RevenueCat's V2 OpenAPI is complete for the paywall management envelope, but not for the component grammar. The component grammar must be cross-referenced with the SDK/web implementations:

- Android:
  - `PaywallComponentsData.kt`
  - `PaywallComponent.kt`
  - `components/*.kt`
  - `properties/*.kt`
- iOS:
  - `PaywallComponentsData.swift`
  - `UIConfig.swift`
  - `PaywallComponentBase.swift`
  - `Paywall*Component.swift`
- Web:
  - `@revenuecat/purchases-ui-js`
  - `dist/types/paywall.d.ts`
  - `dist/types/component.d.ts`
  - `dist/types/components/*.d.ts`

The parser preserves unknown component data. Unknown components and unsupported video can
render an author-provided fallback. Other unsupported properties/conditions are not generally
applied, and not every known component falls back. This is parse tolerance, **not** a guarantee
of forward-compatible appearance or purchase behavior.

The 2026-09-26 source comparison was pinned to RevenueCat iOS 5.91.0:
- [Component grammar, including `web_view`](https://github.com/RevenueCat/purchases-ios/blob/5.91.0/Sources/Paywalls/Components/Common/PaywallComponentBase.swift)
- [Tabs and `stateUpdates`](https://github.com/RevenueCat/purchases-ios/blob/5.91.0/Sources/Paywalls/Components/PaywallTabsComponent.swift)
- [Per-tab package selection](https://github.com/RevenueCat/purchases-ios/blob/5.91.0/RevenueCatUI/Templates/V2/Components/Tabs/TabsComponentViewModel.swift)
- [Purchase actions/methods](https://github.com/RevenueCat/purchases-ios/blob/5.91.0/Sources/Paywalls/Components/PaywallPurchaseButtonComponent.swift)
- [Countdown data contract](https://github.com/RevenueCat/purchases-ios/blob/5.91.0/Sources/Paywalls/Components/PaywallCountdownComponent.swift)

## Library shape

The repository now includes the experimental package project:

- `Plugin.RevenueCat.Paywalls`
- MAUI platform targets for rendering controls
- a `net10.0` target for renderer-independent preprocessing and tests

References:

- `Plugin.RevenueCat.Core` for parsed models and JSON serializers.
- No direct dependency on `Plugin.RevenueCat`; host apps bridge action events to RevenueCat purchase/restore APIs when desired.

The core rendering API should be host-driven:

- `RevenueCatPaywallView : ContentView`
- `RcPaywallView : RevenueCatPaywallView` as a short XAML alias
- `IPaywallRenderer`
- `PaywallRenderRequest`
- `IPaywallActionHandler`
- `IPaywallVariableProvider`
- A future asset-resolution abstraction (not implemented yet)

`RevenueCatPaywallView` can be driven at different levels:

- bind `PaywallData`, `UiConfig`, `Packages`, and `OfferingIdentifier` directly.
- bind `PaywallOffering` for a single runtime/offline offering.
- bind `PaywallOfferings` and let the control use the current offering.

If no explicit `IPaywallActionHandler` is supplied, the control raises public events:

- `PurchaseRequested`
- `RestoreRequested`
- `DismissRequested`
- `NavigationRequested`

The renderer currently styles components while constructing controls. A validated, normalized
render model remains planned; it is a prerequisite for full conditional/layout parity.

## Standalone gallery sample

For the expanded source-backed fixture corpus, exact preset expectations, immutable upstream
references and feature coverage boundaries, see [Paywall gallery coverage](paywall-gallery-coverage.md).
The gallery includes 23 examples, with eight behavior cases and 23 selectable mocked-input
presets shared with the automated scenario tests.

The repository now includes `sample-paywalls/PaywallGallerySample.csproj`, a standalone MAUI app that references only `Plugin.RevenueCat.Paywalls`. It does not initialize the RevenueCat SDK, which keeps visual iteration and manual validation isolated from store configuration.

The gallery starts at `PaywallGalleryPage`, lists offline fixtures, and opens `PaywallPreviewPage` with an embedded `RcPaywallView`. The preview page wires the paywall action events to an on-page event log so purchase/restore/navigation behavior can be validated without live credentials.

The current fixture set covers:

- basic subscription flow
- media/header imagery
- package selection and default selected package
- restore/navigation actions and fallback components
- badge overlays
- rounded cards, borders, pills, and shadows
- light/dark gradients and color aliases
- checklist rows, two-column benefit cards, bottom-sheet style layouts, lifetime offers, trial/story fallbacks, and comparison cards
- carousel onboarding with page peek/spacing, page indicators, and auto-advance
- tabbed plan content with tab-control buttons/toggle controls
- timeline rows/connectors, live countdown variables, and video fallback behavior

The sample includes deterministic local SVG assets under `sample-paywalls/Resources/Images/` and raw paywall JSON fixtures under `sample-paywalls/Resources/Raw/paywalls/`. The SVG files are also packaged as raw app assets so the renderer exercises the SkiaSharp SVG path instead of relying on MAUI's generated PNG image assets.

Apps that render paywalls should call `builder.UseRevenueCatPaywalls()` during MAUI startup.
This registers SkiaSharp and the Android accessible activation-button handler.

DevFlow is installed through the repo tool manifest and enabled in the sample under `#if DEBUG` with `builder.AddMauiDevFlowAgent()`. Mac Catalyst debug builds include the local server entitlement needed by the DevFlow agent. Useful validation commands:

```bash
dotnet workload restore sample-paywalls/PaywallGallerySample.csproj
DEVELOPER_DIR=/Applications/Xcode_26.6.app/Contents/Developer dotnet build sample-paywalls/PaywallGallerySample.csproj -f net10.0-maccatalyst -p:RuntimeIdentifier=maccatalyst-arm64 -t:Run
dotnet tool run maui -- devflow wait --project sample-paywalls/PaywallGallerySample.csproj --timeout 45
dotnet tool run maui -- devflow ui screenshot --selector PaywallPreviewPage --output artifacts/paywall-gallery.png --overwrite
```

## Component mapping

| RevenueCat component | MAUI mapping | Status |
| --- | --- | --- |
| root/base | `Grid` with background, scrollable body, optional header, optional sticky footer | Yes |
| `stack` vertical | `VerticalStackLayout`; later `FlexLayout` for advanced distribution | Yes |
| `stack` horizontal | `Grid` for direct text children, otherwise `HorizontalStackLayout`; advanced distribution pending | Partial |
| `stack` zlayer | `Grid` with layered children | Yes |
| `text` | `Label` | Yes |
| `image` | `Image` or SkiaSharp-backed SVG view in optional `Border` | Yes |
| `icon` | `Image` or SkiaSharp-backed SVG view from resolved icon URL | Yes |
| `button` | Authored nested stack plus native accessible activation button; host actions | Partial (broader accessibility validation remains) |
| `package` | Selectable authored stack with native selected/checked action surface | Yes, within supported conditional grammar |
| `purchase_button` | Native accessible button over authored stack; in-app purchase action | Partial (non-in-app checkout is explicitly rejected) |
| `header` | Top row/overlay | Yes |
| `sticky_footer` / `footer` | Bottom row pinned outside body scroll | Yes |
| `carousel` | `CarouselView` with page peek, spacing, loop, initial position, auto-advance, and `IndicatorView` page control | Partial |
| `tabs` / tab-control buttons | selected content view + local tab state | Partial |
| `tab_control_toggle` | Custom pill track/thumb plus native checkable action mapped to first/second tab IDs | Partial (Android switch semantics inspected; full screen-reader coverage pending) |
| `timeline` | vertical rows with icon/title/description and connector lines | Partial |
| `countdown` | timer-refreshed countdown variable resolution and countdown/end stack selection | Partial |
| `video` | renders fallback component or placeholder | Partial |
| unknown/unsupported | Fallback for unknown components; otherwise invisible placeholder | Parse preservation only; not compatibility certification |

## Future custom layout option

The MVP should use standard MAUI layouts first, but a future renderer may need a custom layout to more closely match RevenueCatUI's Compose/SwiftUI behavior. RevenueCat's Android/iOS/Web UI implementations and tests can be used as source material for expected layout behavior. If standard `VerticalStackLayout`, `HorizontalStackLayout`, `Grid`, and `FlexLayout` produce unacceptable differences, investigate a `PaywallLayout` that ports or models the relevant measure/arrange behavior for:

- flex distribution (`space_between`, `space_around`, `space_evenly`).
- z-layer alignment.
- fill/fit/fixed size constraints.
- safe-area/header-media behavior.
- sticky footer/body interactions.

This should be a later phase after the standard-layout MVP renders real fixtures.

## Shared property mapping

The first implementation supports:

- RGBA hex colors and color aliases.
- Light/dark color schemes.
- linear and radial gradient brushes.
- basic `fit`, `fill`, and `fixed` sizing.
- padding/margin with leading/trailing mapping.
- vertical/horizontal/z-layer layout.
- text alignment, font size names, and basic font weight.
- simple image fit modes.
- local file and remote image/icon sources, including SVG via SkiaSharp and `Svg.Skia`.
- border/background/stroke visuals.
- rounded rectangle and pill shapes.
- shadows.
- badge overlays.
- carousel page controls.
- carousel auto-advance.
- tab toggle selection for two-tab controls.
- timeline connector lines.
- live countdown refresh and end-stack swap.
- package-local variable resolution so package cards can show their own prices while standalone purchase buttons use the selected package.

Deferred:

- custom remote fonts.
- override properties/conditions beyond the documented supported subset.
- video backgrounds.
- pixel-perfect safe-area/hero media behavior.
- richer carousel transitions such as fade.
- broader screen-reader, keyboard focus continuity, large-text and RTL verification.

## State and actions

Implemented state:

- selected package ID.
- selected tab ID for the currently rendered tabs component.
- pending gesture operation gating (async `IPaywallActionHandler` only).
- explicit customer/offer eligibility, custom variables and typed tab-updated state.
- current locale.
- current app theme at render time (live theme-change handling remains a gap).

Package selection updates the existing labels/borders and reads the current package when a
purchase is invoked. Only a real tab-content swap crossfades; its incoming view is not reparented
at animation completion. Host event handlers are notifications, not awaitable purchase tasks:
use `IPaywallActionHandler` when the renderer must await purchase/restore completion.
`ActionFailed` reports failed or unsupported actions.
Custom renderers retain the default re-render-on-selection contract unless they explicitly
opt into `IPaywallRenderer.HandlesPackageSelectionUpdates`.

Actions are delegated to the host through either an explicit `IPaywallActionHandler` or the control's default events:

- purchase.
- restore.
- dismiss.
- URL navigation.
- customer center.
- unknown/custom actions.

### Connecting native eligibility to the paywall

The optional `IRevenueCatIntroEligibility` capability returns customer-specific iOS/Mac
Catalyst results keyed by store product ID. The paywall library does not reference the native
plugin. A host using both packages can map a **successful** result explicitly:

```csharp
static IReadOnlyDictionary<PaywallOfferKey, PaywallEligibility> MapIntroEligibility(
    IReadOnlyDictionary<string, IntroEligibilityStatus> statuses) =>
    statuses.ToDictionary(
        pair => new PaywallOfferKey(pair.Key),
        pair => pair.Value switch
        {
            IntroEligibilityStatus.Eligible => PaywallEligibility.Eligible,
            IntroEligibilityStatus.Ineligible => PaywallEligibility.Ineligible,
            IntroEligibilityStatus.NoIntroOfferExists => PaywallEligibility.NoIntroOfferExists,
            _ => PaywallEligibility.Unknown
        });
```

The enums have different numeric ordering: **do not cast between them**. Assign this map to
`PaywallSemanticContext.IntroOfferEligibility`, preserving the host's other semantic facts.
Use the product identifier, not a reusable package alias. Android offer-specific facts, if
independently established, require the exact product and subscription-option identifier.

Eligibility is customer-scoped. Invalidate it immediately on login/logout or customer change;
do not retain a previous user's known-eligible map while a new query is pending. Handle a
failed/unsupported native result as a failure with eligibility still unknown, not as proof of
ineligibility. Unknown introductory eligibility does not itself prevent a normal full-price
purchase. The cross-project regression tests exercise all four native statuses, product-ID
mapping, introductory duration, and stale data after a product replacement.

## Variables

Basic variable support includes:

- `{{ app_name }}`
- `{{ price }}`
- `{{ product_name }}`
- `{{ sub_period }}`
- `{{ sub_duration }}`

The following variables use available product metadata; intro variables additionally require
explicit eligibility:

- `price_per_period`
- `sub_price_per_week`
- `sub_price_per_month`
- `sub_offer_duration`
- `sub_offer_price`

`sub_relative_discount`, complex/multiple intro phases and comprehensive locale-aware offer
copy remain unsupported.

Unresolved variables report through the optional semantic diagnostic callback; full
locale-aware period/trial formatting remains planned. The built-in period names are English;
apps requiring additional languages must supply an appropriate variable provider.

## Test plan

Use offline tests that do not require RevenueCat credentials:

- locale fallback.
- variable substitution.
- unknown component fallback.
- unsupported known component fallback.
- package default selection.
- advanced component fixture parsing.
- basic component-to-view-node mapping.

Run the offline suite without dummy credentials:

```bash
dotnet test Tests/Tests.csproj --filter 'TestCategory!=Integration&TestCategory!=RequiresRevenueCatSecrets'
```

The test project includes the platform-neutral MAUI renderer/control source for real managed
view-tree assertions, and copies gallery fixtures into test output so tests do not rely on the
checkout/output directory depth. The standalone gallery must also build in Release on Mac
Catalyst/Android. Native visual/accessibility, trimming/AOT and live purchase tests remain
separate release gates; a successful parser test does not satisfy them.

## MVP scope

The MVP is successful when sanitized fixtures with stack/text/image/icon/package/purchase-button/header/sticky-footer render in a MAUI page, package selection changes UI state, purchase taps are delegated through the action handler/events, and the offline gallery can be used for manual visual comparison without RevenueCat SDK initialization.
