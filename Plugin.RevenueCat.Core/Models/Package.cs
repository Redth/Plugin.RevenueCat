#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;
using System.Text.Json.Serialization;

public partial class Package
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("identifier")]
	public string Identifier { get; set; }

	[JsonPropertyName("package_type")]
	[JsonConverter(typeof(JsonStringEnumConverter<PackageType>))]
	public PackageType PackageType { get; set; } = PackageType.Unknown;
	
	[JsonPropertyName("store_product")]
	public StoreProduct? StoreProduct { get; set; }
}

#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
