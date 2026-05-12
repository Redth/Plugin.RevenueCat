using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class PurchaseResult
{
	[JsonPropertyName("customer_info")]
	public CustomerInfo? CustomerInfo { get; set; }

	[JsonPropertyName("store_transaction")]
	public StoreTransaction? StoreTransaction { get; set; }

	[JsonPropertyName("user_cancelled")]
	public bool UserCancelled { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
