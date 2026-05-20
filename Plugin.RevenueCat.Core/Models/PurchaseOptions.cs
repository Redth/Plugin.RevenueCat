using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RevenueCatReplacementMode>))]
public enum RevenueCatReplacementMode
{
	WithoutProration,
	WithTimeProration,
	ChargeFullPrice,
	ChargeProratedPrice,
	Deferred
}

public class PurchaseOptions
{
	[JsonPropertyName("subscription_option_id")]
	public string? SubscriptionOptionId { get; set; }

	[JsonPropertyName("old_product_identifier")]
	public string? OldProductIdentifier { get; set; }

	[JsonPropertyName("replacement_mode")]
	public RevenueCatReplacementMode? ReplacementMode { get; set; }

	[JsonPropertyName("is_personalized_price")]
	public bool? IsPersonalizedPrice { get; set; }

	[JsonPropertyName("presented_offering_identifier")]
	public string? PresentedOfferingIdentifier { get; set; }

	[JsonPropertyName("store_product_discount_identifier")]
	public string? StoreProductDiscountIdentifier { get; set; }

	[JsonPropertyName("discount_identifier")]
	public string? DiscountIdentifier { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
