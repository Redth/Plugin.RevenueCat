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

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("localized_introductory_price_string")]
	public string LocalizedIntroductoryPriceString { get; set; }

	[JsonPropertyName("localized_price_string")]
	public string LocalizedPriceString { get; set; }

	[JsonPropertyName("offering_identifier")]
	public string OfferingIdentifier { get; set; }

	[JsonPropertyName("package_type")]
	public string PackageType { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
