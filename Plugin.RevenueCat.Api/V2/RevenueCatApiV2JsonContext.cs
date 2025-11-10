using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(Customer))]
[JsonSerializable(typeof(CustomerAttribute))]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(Entitlement))]
[JsonSerializable(typeof(Offering))]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(PagedList<Package>))]
[JsonSerializable(typeof(PagedList<Entitlement>))]
[JsonSerializable(typeof(PagedList<CustomerAttribute>))]
[JsonSerializable(typeof(PagedList<Offering>))]
[JsonSerializable(typeof(PagedList<Product>))]
public partial class RevenueCatApiV2JsonContext : JsonSerializerContext
{
}
