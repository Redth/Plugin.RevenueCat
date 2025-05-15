using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class PagedList<T>
{
	[JsonPropertyName("items")]
	public List<T> Items { get; set; } = new();

	[JsonPropertyName("url")]
	public string Url { get; set; }

	[JsonPropertyName("next_page")]
	public string Next { get; set; }
}
