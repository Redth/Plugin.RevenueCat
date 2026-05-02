# RevenueCat MAUI paywall renderer plan

## Goal

RevenueCat Paywalls V2 are remote UI definitions. The native RevenueCatUI SDKs render those definitions with platform-native UI frameworks, but binding those UI SDKs would significantly increase dependency and maintenance complexity, especially on Android. This repository can instead parse the paywall component payload and render a useful subset with .NET MAUI controls.

The initial goal is an experimental MAUI renderer that can be placed in a `ContentPage` and render a dashboard-created paywall made from common V2 components, while continuing to use the existing slim RevenueCat bindings for purchase execution.

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

The current parser should preserve unknown component data and render a `fallback` component when provided.

## Library shape

Add a new package project:

- `Plugin.RevenueCat.Paywalls`
- MAUI platform targets for rendering controls
- a `net10.0` target for renderer-independent preprocessing and tests

References:

- `Plugin.RevenueCat.Core` for parsed models and JSON serializers.
- `Plugin.RevenueCat` only for an optional action-handler adapter to the existing `IRevenueCatManager`.

The core rendering API should be host-driven:

- `RevenueCatPaywallView : ContentView`
- `IPaywallRenderer`
- `PaywallRenderRequest`
- `IPaywallActionHandler`
- `IPaywallVariableProvider`
- `IPaywallAssetResolver`

The renderer should normalize and style the paywall before creating MAUI controls. This mirrors RevenueCatUI: validate/localize/style first, then render.

## Component mapping

| RevenueCat component | MAUI mapping | MVP |
| --- | --- | --- |
| root/base | `Grid` with background, scrollable body, optional header, optional sticky footer | Yes |
| `stack` vertical | `VerticalStackLayout`; later `FlexLayout` for advanced distribution | Yes |
| `stack` horizontal | `HorizontalStackLayout`; later `FlexLayout` for advanced distribution | Yes |
| `stack` zlayer | `Grid` with layered children | Yes |
| `text` | `Label` | Yes |
| `image` | `Image` in optional `Border` | Yes |
| `icon` | `Image` from resolved icon URL | Yes |
| `button` | `Border`/`ContentView` with tap gesture and nested stack | Yes |
| `package` | Selectable `Border`/`ContentView` with nested stack | Yes |
| `purchase_button` | Tappable nested stack that calls purchase action | Yes |
| `header` | Top row/overlay | Yes |
| `sticky_footer` / `footer` | Bottom row pinned outside body scroll | Yes |
| `carousel` | `CarouselView` | Later |
| `tabs` / tab controls | selected content view + state | Later |
| `timeline` | custom `Grid`/`GraphicsView` connector rendering | Later |
| `countdown` | timer-driven labels | Later |
| `video` | media control dependency | Later |
| unknown | fallback component or invisible placeholder | Yes |

## Future custom layout option

The MVP should use standard MAUI layouts first, but a future renderer may need a custom layout to more closely match RevenueCatUI's Compose/SwiftUI behavior. RevenueCat's Android/iOS/Web UI implementations and tests can be used as source material for expected layout behavior. If standard `VerticalStackLayout`, `HorizontalStackLayout`, `Grid`, and `FlexLayout` produce unacceptable differences, investigate a `PaywallLayout` that ports or models the relevant measure/arrange behavior for:

- flex distribution (`space_between`, `space_around`, `space_evenly`).
- z-layer alignment.
- fill/fit/fixed size constraints.
- safe-area/header-media behavior.
- sticky footer/body interactions.

This should be a later phase after the standard-layout MVP renders real fixtures.

## Shared property mapping

The first implementation should support:

- RGBA hex colors and color aliases.
- Light/dark color schemes.
- basic `fit`, `fill`, and `fixed` sizing.
- padding/margin with leading/trailing mapping.
- vertical/horizontal/z-layer layout.
- text alignment, font size names, and basic font weight.
- simple image fit modes.
- basic border/background/stroke visuals.

Deferred:

- custom remote fonts.
- radial/linear gradients beyond a reasonable fallback.
- complex shadows and badges.
- advanced conditional overrides.
- video backgrounds.
- pixel-perfect safe-area/hero media behavior.

## State and actions

The renderer needs local state for:

- selected package ID.
- pending purchase/restore state.
- current locale.
- current app theme.

Actions should be delegated to the host:

- purchase.
- restore.
- dismiss.
- URL navigation.
- customer center.
- unknown/custom actions.

The library can include an optional `RevenueCatManagerPaywallActionHandler` adapter that calls `IRevenueCatManager.PurchaseAsync` and `RestoreAsync`.

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
- package default selection.
- basic component-to-view-node mapping.

For MAUI controls, validate at least one platform build and keep most behavior in testable pure preprocessing helpers.

## MVP scope

The MVP is successful when a sanitized fixture with stack/text/image/icon/package/purchase-button/header/sticky-footer renders in a MAUI page, package selection changes UI state, and purchase taps are delegated through the action handler.
