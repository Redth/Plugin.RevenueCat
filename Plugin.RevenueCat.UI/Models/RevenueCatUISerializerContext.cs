using System.Text.Json.Serialization;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.UI;

[JsonSourceGenerationOptions(
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	NumberHandling = JsonNumberHandling.AllowReadingFromString,
	WriteIndented = false)]
[JsonSerializable(typeof(PaywallPresentationResult))]
[JsonSerializable(typeof(CustomerCenterResult))]
[JsonSerializable(typeof(CustomerInfo))]
public partial class RevenueCatUISerializerContext : JsonSerializerContext
{
}
