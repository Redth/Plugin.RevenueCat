using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class NonSubscription
{
	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("is_sandbox")]
	public bool IsSandbox { get; set; }

	[JsonPropertyName("price")]
	public Price Price { get; set; }

	[JsonPropertyName("store")]
	public string Store { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset? PurchaseDate { get; set; }
}
