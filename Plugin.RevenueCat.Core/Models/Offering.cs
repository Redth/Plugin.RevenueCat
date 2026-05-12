#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class Offering
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("identifier")]
	public string Identifier { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("packages")]
	public List<Package> Packages { get; set; } = new();

	[JsonPropertyName("metadata")]
	public Dictionary<string, JsonElement> Metadata { get; set; } = new();

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
