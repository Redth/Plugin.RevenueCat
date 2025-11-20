using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(SubscriberResponse))]
[JsonSerializable(typeof(SubscriberValueResponse))]
[JsonSerializable(typeof(Subscriber), TypeInfoPropertyName = "ApiV1Subscriber")]
[JsonSerializable(typeof(SubscriberAttribute))]
[JsonSerializable(typeof(SubscriberAttributes))]
[JsonSerializable(typeof(Package), TypeInfoPropertyName = "ApiV1Package")]
[JsonSerializable(typeof(List<Package>), TypeInfoPropertyName = "ApiV1ListPackage")]
[JsonSerializable(typeof(Entitlement), TypeInfoPropertyName = "ApiV1Entitlement")]
[JsonSerializable(typeof(Subscription), TypeInfoPropertyName = "ApiV1Subscription")]
[JsonSerializable(typeof(NonSubscription), TypeInfoPropertyName = "ApiV1NonSubscription")]
[JsonSerializable(typeof(Offering), TypeInfoPropertyName = "ApiV1Offering")]
[JsonSerializable(typeof(OfferingsResponse))]
public partial class RevenueCatApiV1JsonContext : JsonSerializerContext
{
}
