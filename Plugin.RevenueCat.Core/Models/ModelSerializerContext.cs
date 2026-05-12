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
        typeof(TimeOnlyConverter)
    ])]
[JsonSerializable(typeof(CustomerInfo))]
[JsonSerializable(typeof(Offering))]
[JsonSerializable(typeof(Offerings))]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(Entitlement))]
[JsonSerializable(typeof(Subscriber))]
[JsonSerializable(typeof(Subscription))]
[JsonSerializable(typeof(NonSubscription))]
[JsonSerializable(typeof(StoreProduct))]
[JsonSerializable(typeof(StoreProductDiscount))]
[JsonSerializable(typeof(StoreTransaction))]
[JsonSerializable(typeof(PurchaseResult))]
[JsonSerializable(typeof(PurchaseOptions))]
[JsonSerializable(typeof(RevenueCatReplacementMode))]
[JsonSerializable(typeof(WebPurchaseRedemptionResult))]
[JsonSerializable(typeof(RevenueCatError))]
[JsonSerializable(typeof(SubscriptionPeriod))]
[JsonSerializable(typeof(SubscriptionOption))]
[JsonSerializable(typeof(PricingPhase))]
[JsonSerializable(typeof(InstallmentsInfo))]
[JsonSerializable(typeof(Price))]
[JsonSerializable(typeof(PackageType))]
[JsonSerializable(typeof(RevenueCatProductType))]
[JsonSerializable(typeof(SubscriptionPeriodUnit))]
[JsonSerializable(typeof(VirtualCurrencies))]
[JsonSerializable(typeof(VirtualCurrency))]
[JsonSerializable(typeof(List<Package>))]
[JsonSerializable(typeof(List<Offering>))]
[JsonSerializable(typeof(List<StoreProduct>))]
[JsonSerializable(typeof(List<StoreProductDiscount>))]
[JsonSerializable(typeof(List<SubscriptionOption>))]
[JsonSerializable(typeof(List<PricingPhase>))]
[JsonSerializable(typeof(List<NonSubscription>))]
[JsonSerializable(typeof(Dictionary<string, Entitlement>))]
[JsonSerializable(typeof(Dictionary<string, Subscription>))]
[JsonSerializable(typeof(Dictionary<string, NonSubscription>))]
[JsonSerializable(typeof(Dictionary<string, Offering>))]
[JsonSerializable(typeof(Dictionary<string, VirtualCurrency>))]
public partial class ModelSerializerContext : JsonSerializerContext
{
}
