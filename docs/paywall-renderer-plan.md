# RevenueCat MAUI paywall renderer plan

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

- `IRevenueCatManager.GetAppUserIdAsync()` exposes the active SDK app user ID, including SDK-generated anonymous IDs.
- `IRevenueCatApiV1.GetPaywallOfferings(...)` fetches the SDK runtime Offerings endpoint and deserializes the full paywall-aware payload into `PaywallOfferingsResponse`.
- `Offering` and `Package` core models include additional paywall/runtime fields such as metadata, web checkout URLs, legacy `paywall`, `paywall_components`, `draft_paywall_components`, and `ui_config` when those fields are present.
- The slim native `GetOfferingAsync` wrappers now include richer runtime fields where safely available. The direct V1 runtime Offerings API remains the preferred source for full top-level `ui_config`, placements, and targeting data.

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

The current parser preserves unknown component data and the renderer uses `fallback` when provided for unknown or currently unsupported known components. This keeps newer RevenueCat component payloads forward-compatible while the MAUI renderer catches up.

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
- `IPaywallAssetResolver`

`RevenueCatPaywallView` can be driven at different levels:

- bind `PaywallData`, `UiConfig`, `Packages`, and `OfferingIdentifier` directly.
- bind `PaywallOffering` for a single runtime/offline offering.
- bind `PaywallOfferings` and let the control use the current offering.

If no explicit `IPaywallActionHandler` is supplied, the control raises public events:

- `PurchaseRequested`
- `RestoreRequested`
- `DismissRequested`
- `NavigationRequested`

The renderer should normalize and style the paywall before creating MAUI controls. This mirrors RevenueCatUI: validate/localize/style first, then render.

## Standalone gallery sample

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
- carousel onboarding with page peek/spacing and page indicators
- tabbed plan content with tab-control buttons
- timeline rows, countdown variables, and video fallback behavior

The sample includes deterministic local SVG assets under `sample-paywalls/Resources/Images/` and raw paywall JSON fixtures under `sample-paywalls/Resources/Raw/paywalls/`. The SVG files are also packaged as raw app assets so the renderer exercises the SkiaSharp SVG path instead of relying on MAUI's generated PNG image assets.

Apps that render paywalls should call `builder.UseRevenueCatPaywalls()` during MAUI startup. This registers SkiaSharp for SVG paywall image/icon rendering.

DevFlow is installed through the repo tool manifest and enabled in the sample under `#if DEBUG` with `builder.AddMauiDevFlowAgent()`. Mac Catalyst debug builds include the local server entitlement needed by the DevFlow agent. Useful validation commands:

```bash
/usr/local/share/dotnet/dotnet build sample-paywalls/PaywallGallerySample.csproj -f net10.0-maccatalyst -p:RuntimeIdentifier=maccatalyst-arm64 -t:Run
/usr/local/share/dotnet/dotnet tool run maui -- devflow wait --project sample-paywalls/PaywallGallerySample.csproj --timeout 45
/usr/local/share/dotnet/dotnet tool run maui -- devflow ui screenshot --output artifacts/paywall-gallery.png --overwrite
```

## Component mapping

| RevenueCat component | MAUI mapping | Status |
| --- | --- | --- |
| root/base | `Grid` with background, scrollable body, optional header, optional sticky footer | Yes |
| `stack` vertical | `VerticalStackLayout`; later `FlexLayout` for advanced distribution | Yes |
| `stack` horizontal | `HorizontalStackLayout`; later `FlexLayout` for advanced distribution | Yes |
| `stack` zlayer | `Grid` with layered children | Yes |
| `text` | `Label` | Yes |
| `image` | `Image` or SkiaSharp-backed SVG view in optional `Border` | Yes |
| `icon` | `Image` or SkiaSharp-backed SVG view from resolved icon URL | Yes |
| `button` | `Border`/`ContentView` with tap gesture and nested stack | Yes |
| `package` | Selectable `Border`/`ContentView` with nested stack | Yes |
| `purchase_button` | Tappable nested stack that calls purchase action | Yes |
| `header` | Top row/overlay | Yes |
| `sticky_footer` / `footer` | Bottom row pinned outside body scroll | Yes |
| `carousel` | `CarouselView` with page peek, spacing, loop, initial position, and `IndicatorView` page control | Partial |
| `tabs` / tab-control buttons | selected content view + local tab state | Partial |
| `tab_control_toggle` | placeholder `Switch` | Partial |
| `timeline` | vertical rows with icon/title/description | Partial |
| `countdown` | one-shot countdown variable resolution and countdown/end stack selection | Partial |
| `video` | renders fallback component or placeholder | Partial |
| unknown/unsupported | fallback component or invisible placeholder | Yes |

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
- package-local variable resolution so package cards can show their own prices while standalone purchase buttons use the selected package.

Deferred:

- custom remote fonts.
- advanced conditional overrides.
- video backgrounds.
- pixel-perfect safe-area/hero media behavior.
- timeline connector drawing.
- live countdown timer updates and disposal.
- carousel auto-advance.
- tab toggle semantics.

## State and actions

The renderer needs local state for:

- selected package ID.
- selected tab ID for the currently rendered tabs component.
- pending purchase/restore state.
- current locale.
- current app theme.

Actions are delegated to the host through either an explicit `IPaywallActionHandler` or the control's default events:

- purchase.
- restore.
- dismiss.
- URL navigation.
- customer center.
- unknown/custom actions.

## Variables

Initial variable support should include:

- `{{ app_name }}`
- `{{ price }}`
- `{{ product_name }}`
- `{{ sub_period }}`
- `{{ sub_duration }}`

Additional variables require richer product and intro-offer metadata:

- `price_per_period`
- `sub_price_per_week`
- `sub_price_per_month`
- `sub_offer_duration`
- `sub_offer_price`
- `sub_relative_discount`

Unresolved recognized variables should not crash rendering. They should produce diagnostics and substitute an empty string unless a host option asks to preserve tokens.

## Test plan

Use offline tests that do not require RevenueCat credentials:

- locale fallback.
- variable substitution.
- unknown component fallback.
- unsupported known component fallback.
- package default selection.
- advanced component fixture parsing.
- basic component-to-view-node mapping.

For MAUI controls, validate at least one platform build and keep most behavior in testable pure preprocessing helpers. The standalone gallery should also validate its raw fixture corpus with JSON parsing and build on Mac Catalyst/Android when renderer or sample XAML changes.

## MVP scope

The MVP is successful when sanitized fixtures with stack/text/image/icon/package/purchase-button/header/sticky-footer render in a MAUI page, package selection changes UI state, purchase taps are delegated through the action handler/events, and the offline gallery can be used for manual visual comparison without RevenueCat SDK initialization.
