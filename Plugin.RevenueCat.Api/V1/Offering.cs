using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public class Offering
{
	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("identifier")]
	public string Identifier { get; set; }

	[JsonPropertyName("packages")]
	public List<Package> Packages { get; set;} = new();
}
