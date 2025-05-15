using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class Product
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("store_identifier")]
	public string StoreIdentifier { get; set; }

	[JsonPropertyName("created_at")]
	public DateTimeOffset CreatedAt { get; set; }

	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }

	[JsonPropertyName("app_id")]
	public string AppId { get; set; }

	//public Subscription? Subscription { get; set; }

	//public OneTime? OneTime { get; set; }

}
