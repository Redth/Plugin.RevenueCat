using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public class Package
{
	[JsonPropertyName("identifier")]
	public string Identifier { get; set; }

	[JsonPropertyName("platform_product_identifier")]
	public string PlatformProductIdentifier { get; set; }
}