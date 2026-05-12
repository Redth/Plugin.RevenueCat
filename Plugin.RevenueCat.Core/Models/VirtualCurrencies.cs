using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class VirtualCurrencies
{
	[JsonPropertyName("all")]
	public Dictionary<string, VirtualCurrency> All { get; set; } = new();

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class VirtualCurrency
{
	[JsonPropertyName("code")]
	public string? Code { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("balance")]
	public int? Balance { get; set; }

	[JsonPropertyName("server_description")]
	public string? ServerDescription { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
