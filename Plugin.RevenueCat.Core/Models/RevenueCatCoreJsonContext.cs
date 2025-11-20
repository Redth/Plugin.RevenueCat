using System.Text.Json.Serialization;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Core.Models;

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(CustomerInfo))]
[JsonSerializable(typeof(Offering))]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(List<Package>))]
[JsonSerializable(typeof(Entitlement))]
[JsonSerializable(typeof(Subscription))]
[JsonSerializable(typeof(NonSubscription))]
[JsonSerializable(typeof(List<NonSubscription>))]
[JsonSerializable(typeof(Dictionary<string, List<NonSubscription>>))]
[JsonSerializable(typeof(StoreProduct))]
[JsonSerializable(typeof(StoreTransaction))]
[JsonSerializable(typeof(Price))]
[JsonSerializable(typeof(SubscriptionPeriod))]
[JsonSerializable(typeof(Subscriber))]
public partial class RevenueCatCoreJsonContext : JsonSerializerContext
{
}
