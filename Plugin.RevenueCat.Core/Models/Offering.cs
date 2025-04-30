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
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603

public partial class Offering
{
	public static Offering FromJson(string json) => JsonSerializer.Deserialize<Offering>(json, ModelExtensions.Settings);
}