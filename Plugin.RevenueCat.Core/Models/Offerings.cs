using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class Offerings
{
	[JsonPropertyName("current_offering_id")]
	public string? CurrentOfferingId { get; set; }

	[JsonPropertyName("current")]
	public Offering? Current { get; set; }

	[JsonPropertyName("all")]
	public Dictionary<string, Offering> All { get; set; } = new();

	[JsonPropertyName("offerings")]
	public List<Offering> Items { get; set; } = new();

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
