using System.Text.Json.Serialization;
using Plugin.RevenueCat.Api.V1.Converters;

namespace Plugin.RevenueCat.Api.V1;

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
[JsonSerializable(typeof(SubscriberResponse))]
[JsonSerializable(typeof(SubscriberValueResponse))]
[JsonSerializable(typeof(OfferingsResponse))]
[JsonSerializable(typeof(Models.Subscriber))]
[JsonSerializable(typeof(Models.Entitlement))]
[JsonSerializable(typeof(Models.Subscription))]
[JsonSerializable(typeof(Models.NonSubscription))]
[JsonSerializable(typeof(Models.Offering))]
[JsonSerializable(typeof(Models.Package))]
[JsonSerializable(typeof(List<Models.Offering>))]
[JsonSerializable(typeof(List<Models.Package>))]
[JsonSerializable(typeof(Dictionary<string, Models.Entitlement>))]
[JsonSerializable(typeof(Dictionary<string, Models.Subscription>))]
[JsonSerializable(typeof(Dictionary<string, Models.NonSubscription[]>))]
public partial class ApiV1SerializerContext : JsonSerializerContext
{
}

