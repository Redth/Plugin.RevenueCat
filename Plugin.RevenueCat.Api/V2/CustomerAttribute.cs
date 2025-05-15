using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class CustomerAttribute
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("value")]
	public string Value { get; set; }

	[JsonPropertyName("updated_at")]
	public DateTimeOffset UpdatedAt { get; set; }
}
