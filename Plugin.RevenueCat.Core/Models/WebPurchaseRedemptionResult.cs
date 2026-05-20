using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class WebPurchaseRedemptionResult
{
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	[JsonPropertyName("customer_info")]
	public CustomerInfo? CustomerInfo { get; set; }

	[JsonPropertyName("obfuscated_email")]
	public string? ObfuscatedEmail { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
