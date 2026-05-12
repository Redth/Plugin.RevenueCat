using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models
{
	public class StoreTransaction
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("transaction_identifier")]
		public string? TransactionIdentifier { get; set; }

		[JsonPropertyName("product_identifier")]
		public string? ProductIdentifier { get; set; }

		[JsonPropertyName("product_identifiers")]
		public List<string> ProductIdentifiers { get; set; } = new();

		[JsonPropertyName("purchase_date")]
		public DateTimeOffset? PurchaseDate { get; set; }

		[JsonPropertyName("quantity")]
		public int? Quantity { get; set; }

		[JsonPropertyName("store")]
		public string? Store { get; set; }

		[JsonExtensionData]
		public IDictionary<string, JsonElement>? ExtensionData { get; set; }
	}
}
