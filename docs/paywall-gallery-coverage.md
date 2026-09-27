# Paywall gallery coverage

The gallery has **23 examples**: the original 15 visual smoke fixtures and eight new
source-backed behavior cases with **23 selectable input presets**. All examples run offline
with mocked products. No preset queries real eligibility, opens checkout or charges an account.

## What makes the new cases verifiable

Each `verified_*.json` contains a normal runtime offerings envelope plus a **gallery-only**
`gallery` metadata section. That section records immutable upstream source links, mocked
catalog overrides where needed, named input variants and expected visible text/selection.
It is not part of RevenueCat's API schema.

`PaywallGalleryCase` and `PaywallExample` are compiled into both the sample and tests. The
sample's picker and `PaywallGalleryScenarioTests` consume the same inputs. The tests construct
actual managed MAUI controls and invoke their accessible action commands; they check visible
ancestors, exact text, available/selected packages, events, object identity and restored sizes.
Interactive tests additionally assert outcomes independently of the metadata.
The tab examples also check that visible content stays vertically centered inside the native
minimum-height hit target, without changing the fixture padding or package-card alignment.
Best value badge and Premium comparison assert that selection uses the authored card border,
not an extra outline on the accessibility wrapper. Carousel onboarding asserts that page dots
remain visible while native carousel scrollbars are hidden; the main paywall still scrolls.

In the sample, use **Mocked validation input** to choose a preset. The event panel shows the
expected result and the actual action/diagnostic. Preset changes preserve existing selection
when it remains valid; the stated initial selected package in test metadata applies to a
fresh presentation. Window presets simulate **host facts**, not the physical preview size.
Changing the timed-offer preset deliberately creates a new countdown presentation.

## Pinned evidence and adaptations

Sources were inspected at:

- RevenueCat iOS **5.91.0**, commit
  [`d654b8b85143c29e5bb73d0ff92f5f5766fbbedc`](https://github.com/RevenueCat/purchases-ios/tree/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc).
- RevenueCat Android **10.23.2**, commit
  [`1c44a44177559fd90a769af66745bb17bff5e095`](https://github.com/RevenueCat/purchases-android/tree/1c44a44177559fd90a769af66745bb17bff5e095).

These are **small, independently authored adaptations of behavioral representations**, not
verbatim dashboard exports or pixel-identical reproductions. Layout, copy, identifiers and
store prices are local demo content. Upstream source is copyright RevenueCat Inc.,
distributed under the [MIT license](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/LICENSE).
No remote artwork or customer data is copied into the fixtures.

| Gallery case / fixture | Upstream representation | Expected outcomes |
| --- | --- | --- |
| Trial eligibility / `verified_eligibility.json` | [`PresentedPartialsTests.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Tests/RevenueCatUITests/PaywallsV2/PresentedPartialsTests.swift), intro-offer condition tests | Eligible shows the one-week, zero-price intro; unknown, ineligible and no-offer do not announce a trial. Full-price purchase remains available. Our explicit unknown state is intentionally more conservative than a boolean eligibility input. |
| Audience rules / `verified_rules.json` | Same file: `testMultipleConditions_AllMustMatch`, `testMultipleConditions_OneFailsDoesNotMatch`, `testVariableCondition_BoolTrueDoesNotMatchStringTrue`, `testMultipleMatchingOverrides_LaterPropertiesWin` | VIP + annual selection selects the later message; either nonmatching condition prevents it. String `"true"` is not boolean `true`. Package prices remain local to their cards. |
| Stateful plan tabs / `verified_tab_state.json` | Android tester [`TabsWithButtons.kt`](https://github.com/RevenueCat/purchases-android/blob/1c44a44177559fd90a769af66745bb17bff5e095/examples/paywall-tester/src/main/java/com/revenuecat/paywallstester/paywalls/TabsWithButtons.kt), iOS [`PaywallStateStoreTests.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Tests/RevenueCatUITests/PaywallsV2/PaywallStateStoreTests.swift) | Three button tabs write `$value` into declared string state. State caption, price, accessible selected state and purchase package agree. The adaptation omits unsupported nested tab-control placement and advanced upstream styling. |
| Mixed package visibility / `verified_mixed_packages.json` | [`MixedTabsDefaultPackageVisibilityTests.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Tests/RevenueCatUITests/PaywallsV2/MixedTabsDefaultPackageVisibilityTests.swift) | A hidden annual default outside tabs cannot remain selected. Entering the weekly tab selects weekly; leaving it restores an available page plan, not the hidden default. Removing annual from the mocked catalog disables its action. |
| Window rule boundaries / `verified_window_rules.json` | [`WindowSizeConditionTests.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Tests/RevenueCatUITests/PaywallsV2/WindowSizeConditionTests.swift) | No dimensions match no window rules. Width 699 does not match `>= 700`; 700 does. Wide-to-narrow restores the original type size and natural panel height. Aspect ratio `>= 1.2` reveals a separate note. |
| Localized legal actions / `verified_localization.json` | [`LocaleFinderTests.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Tests/RevenueCatUITests/PaywallsV2/LocaleFinderTests.swift), [`PaywallButtonComponent.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Sources/Paywalls/Components/PaywallButtonComponent.swift) | Exact and neutral-language matches, default English when absent, long German text, and localized `url_lid` events. Tests cover our supported fallback subset, not upstream's full script/region matching algorithm. Mock USD prices intentionally do not change with locale. |
| Forward compatibility / `verified_fallbacks.json` | [`FallbackComponentTests.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Tests/UnitTests/Paywalls/Components/FallbackComponentTests.swift), [`PaywallPurchaseButtonComponent.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Sources/Paywalls/Components/PaywallPurchaseButtonComponent.swift) | Future component, video and web-view authored fallbacks appear without network loads. A web-checkout method reports `NotSupportedException` and emits **zero** in-app purchase events. This tests safe rejection, not web-checkout support. |
| Timed offer expiry / `verified_countdown.json` | [`PaywallCountdownComponent.swift`](https://github.com/RevenueCat/purchases-ios/blob/d654b8b85143c29e5bb73d0ff92f5f5766fbbedc/Sources/Paywalls/Components/PaywallCountdownComponent.swift), `date` style, countdown/end stacks | A fresh ten-second demo deadline switches to the end stack. Already-expired preset renders the ending immediately. Date construction is tested at a fixed instant without mutating the shared fixture; timer scheduling itself requires a running native app. |

## Feature coverage map

| Feature | Gallery coverage | Assertion / limitation |
| --- | --- | --- |
| Stack/text/header/sticky footer | Basic subscription, Media header, all source-backed cases | Managed construction; source-backed text and purchase events asserted. Complex measure/arrange parity is not certified. |
| Images/icons/SVG, gradient, shapes, shadows, badges | Media header, Dark gradient, Best value badge, Feature checklist | Existing visual smoke examples and platform builds; not a complete image pixel oracle. |
| Horizontal wrapping/columns | Feature checklist, Two-column benefits, Premium comparison | Existing rendering/layout regression tests; broad device/font-scale checks remain separate. |
| Package-local variables and selection | Package selector, Premium comparison, Audience rules, Mixed package visibility | Exact card/CTA prices, selection preservation, catalog invalidation and purchase targets asserted. |
| Overrides and typed variables | Audience rules, Window rule boundaries | AND semantics, order, type mismatch, threshold boundaries and reversal asserted. Arbitrary expressions and unsupported style properties remain outside scope. |
| Intro eligibility/trial variables | Trial eligibility | Four mocked states, visible/audible copy and full-price purchase asserted; not a live store-eligibility test. |
| Tabs/toggle/state | Tabbed plans, Stateful plan tabs, Mixed package visibility | Button and toggle examples; state transitions/purchase routing tested. Selected-tab contextual styling remains a separate parity gap. |
| Carousel | Carousel onboarding | Existing managed cell-ownership test and gallery smoke; native swipe/auto-advance timing is not asserted by the new corpus. |
| Timeline/countdown | Timeline countdown, Timed offer expiry | Construction plus deterministic countdown branches/deadline creation; live expiry inspected separately. |
| Locale and legal actions | Localized legal actions | Exact text and URL events, including neutral/default fallback. RTL, script expansion and actual navigation are not certified. |
| Accessibility | All actions; Trial eligibility and Mixed package visibility specifically | Selected/disabled state and visible-content names tested. Spoken VoiceOver/TalkBack, large text and focus continuity still need device coverage. |
| Unknown/media fallback and unsupported checkout | Actions and fallback, Forward compatibility | Positive fallback assertions and negative purchase/network assertions; unsupported features are not marked implemented. |

## Run the checks

```bash
dotnet test Tests/Tests.csproj --filter 'FullyQualifiedName~PaywallGalleryScenarioTests'
dotnet test Tests/Tests.csproj --filter 'TestCategory!=Integration&TestCategory!=RequiresRevenueCatSecrets'
dotnet build sample-paywalls/PaywallGallerySample.csproj -f net10.0-android -p:RuntimeIdentifier=android-arm64
DEVELOPER_DIR=/Applications/Xcode_26.6.app/Contents/Developer dotnet build sample-paywalls/PaywallGallerySample.csproj -f net10.0-maccatalyst -p:RuntimeIdentifier=maccatalyst-arm64
```

For native validation, select the named presets, tap package/tab/action controls, compare the
expected panel with the displayed content and host event log, and inspect native accessibility
nodes. For Timed offer expiry, allow ten seconds after selecting the running preset and verify
the end stack. Never infer timer, screen-reader, store or pixel-parity coverage from a
managed construction test alone.

## Validation recorded for this increment

The source-backed increment introduced **37 gallery checks**: 23 preset cases plus catalog,
interaction, locale-link, fallback and deadline assertions. Mac Catalyst inspection confirmed
eligible-to-ineligible copy changes, weekly tab state/price updates, the Spanish neutral-locale
legal URL event, and the live transition from a nine-second remaining value to `Offer ended`.
Android and Mac Catalyst gallery builds are checked separately.

Android native interaction was not repeated in this increment: the dedicated emulator did not
become ready and its boot request timed out. The existing Android accessibility evidence in the
project plan predates these new examples; it is not counted as a native pass for this corpus.
