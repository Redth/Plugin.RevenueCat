using System.Text.Json.Serialization;
using Plugin.RevenueCat.Core.Converters;

namespace Plugin.RevenueCat.Models;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    WriteIndented = false,
    Converters = [
        typeof(IsoDateTimeOffsetConverter),
        typeof(DateOnlyConverter),
        typeof(TimeOnlyConverter),
        typeof(PaywallComponentConverter)
    ])]
[JsonSerializable(typeof(CustomerInfo))]
[JsonSerializable(typeof(Offering))]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(Entitlement))]
[JsonSerializable(typeof(Subscriber))]
[JsonSerializable(typeof(Subscription))]
[JsonSerializable(typeof(NonSubscription))]
[JsonSerializable(typeof(StoreProduct))]
[JsonSerializable(typeof(StoreTransaction))]
[JsonSerializable(typeof(SubscriptionPeriod))]
[JsonSerializable(typeof(Price))]
[JsonSerializable(typeof(PaywallOfferingsResponse))]
[JsonSerializable(typeof(PaywallOffering))]
[JsonSerializable(typeof(PaywallComponentsData))]
[JsonSerializable(typeof(PaywallComponentsConfig))]
[JsonSerializable(typeof(PaywallRootComponents))]
[JsonSerializable(typeof(PaywallLocalizationData))]
[JsonSerializable(typeof(PaywallUiConfig))]
[JsonSerializable(typeof(PaywallComponent))]
[JsonSerializable(typeof(PaywallButtonComponent))]
[JsonSerializable(typeof(PaywallCarouselComponent))]
[JsonSerializable(typeof(PaywallCountdownComponent))]
[JsonSerializable(typeof(PaywallFallbackHeaderComponent))]
[JsonSerializable(typeof(PaywallHeaderComponent))]
[JsonSerializable(typeof(PaywallIconComponent))]
[JsonSerializable(typeof(PaywallImageComponent))]
[JsonSerializable(typeof(PaywallPackageComponent))]
[JsonSerializable(typeof(PaywallPurchaseButtonComponent))]
[JsonSerializable(typeof(PaywallStackComponent))]
[JsonSerializable(typeof(PaywallStickyFooterComponent))]
[JsonSerializable(typeof(PaywallTabComponent))]
[JsonSerializable(typeof(PaywallTabControlButtonComponent))]
[JsonSerializable(typeof(PaywallTabControlComponent))]
[JsonSerializable(typeof(PaywallTabControlToggleComponent))]
[JsonSerializable(typeof(PaywallTabsComponent))]
[JsonSerializable(typeof(PaywallTextComponent))]
[JsonSerializable(typeof(PaywallTimelineComponent))]
[JsonSerializable(typeof(PaywallUnknownComponent))]
[JsonSerializable(typeof(PaywallVideoComponent))]
[JsonSerializable(typeof(PackageType))]
[JsonSerializable(typeof(SubscriptionPeriodUnit))]
[JsonSerializable(typeof(List<Package>))]
[JsonSerializable(typeof(List<Offering>))]
[JsonSerializable(typeof(List<PaywallComponent>))]
[JsonSerializable(typeof(List<NonSubscription>))]
[JsonSerializable(typeof(Dictionary<string, Entitlement>))]
[JsonSerializable(typeof(Dictionary<string, Subscription>))]
[JsonSerializable(typeof(Dictionary<string, NonSubscription>))]
public partial class ModelSerializerContext : JsonSerializerContext
{
}
