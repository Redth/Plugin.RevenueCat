using System.Text.Json.Serialization;
using Plugin.RevenueCat.Api.V2.Converters;

namespace Plugin.RevenueCat.Api.V2;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    WriteIndented = false,
    Converters = [
        typeof(EpochMsDateTimeOffsetConverter)
    ])]
[JsonSerializable(typeof(Customer))]
[JsonSerializable(typeof(CustomerAttribute))]
[JsonSerializable(typeof(Entitlement))]
[JsonSerializable(typeof(Offering))]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(PagedList<Offering>))]
[JsonSerializable(typeof(PagedList<CustomerAttribute>))]
[JsonSerializable(typeof(PagedList<Entitlement>))]
[JsonSerializable(typeof(SetAttributesRequest))]
[JsonSerializable(typeof(SetAttributesRequest.AttributeItem))]
public partial class ApiV2SerializerContext : JsonSerializerContext
{
}

