using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class StoreProductDiscount
{
	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("offer_identifier")]
	public string? OfferIdentifier { get; set; }

	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("payment_mode")]
	public string? PaymentMode { get; set; }

	[JsonPropertyName("price")]
	public Price? Price { get; set; }

	[JsonPropertyName("price_string")]
	public string? PriceString { get; set; }

	[JsonPropertyName("currency_code")]
	public string? CurrencyCode { get; set; }

	[JsonPropertyName("subscription_period")]
	public SubscriptionPeriod? SubscriptionPeriod { get; set; }

	[JsonPropertyName("number_of_periods")]
	public int? NumberOfPeriods { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
