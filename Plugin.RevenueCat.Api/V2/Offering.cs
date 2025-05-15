using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class Offering
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("lookup_key")]
	public string LookupKey { get; set; }

	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }

	[JsonPropertyName("is_current")]
	public bool IsCurrent { get; set; }

	[JsonPropertyName("created_at")]
	public DateTimeOffset CreatedAt { get; set; }

	[JsonPropertyName("metadata")]
	public Dictionary<string, string> Metadata { get; set; } = new();

	[JsonPropertyName("project_id")]
	public string ProjectId { get; set; }

	[JsonPropertyName("packages")]
	public PagedList<Package> Packages { get; set; } = new();
}
