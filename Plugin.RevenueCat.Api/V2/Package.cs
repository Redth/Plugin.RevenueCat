using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class Package
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("lookup_key")]
	public string LookupKey { get; set; }

	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }

	[JsonPropertyName("position")]
	public int IsCurrent { get; set; }

	[JsonPropertyName("created_at")]
	public DateTimeOffset CreatedAt { get; set; }


	[JsonPropertyName("products")]
	public PagedList<Product> Products { get; set; }
}
