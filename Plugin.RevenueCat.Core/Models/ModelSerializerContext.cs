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
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(Entitlement))]
[JsonSerializable(typeof(Subscriber))]
[JsonSerializable(typeof(Subscription))]
[JsonSerializable(typeof(NonSubscription))]
[JsonSerializable(typeof(StoreProduct))]
[JsonSerializable(typeof(StoreTransaction))]
[JsonSerializable(typeof(SubscriptionPeriod))]
[JsonSerializable(typeof(Price))]
[JsonSerializable(typeof(PackageType))]
[JsonSerializable(typeof(SubscriptionPeriodUnit))]
[JsonSerializable(typeof(List<Package>))]
[JsonSerializable(typeof(List<Offering>))]
[JsonSerializable(typeof(List<NonSubscription>))]
[JsonSerializable(typeof(Dictionary<string, Entitlement>))]
[JsonSerializable(typeof(Dictionary<string, Subscription>))]
[JsonSerializable(typeof(Dictionary<string, NonSubscription>))]
public partial class ModelSerializerContext : JsonSerializerContext
{
}

