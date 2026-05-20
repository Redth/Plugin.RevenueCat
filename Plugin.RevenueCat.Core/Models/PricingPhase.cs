using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class PricingPhase
{
	[JsonPropertyName("billing_period")]
	public SubscriptionPeriod? BillingPeriod { get; set; }

	[JsonPropertyName("recurrence_mode")]
	public string? RecurrenceMode { get; set; }

	[JsonPropertyName("billing_cycle_count")]
	public int? BillingCycleCount { get; set; }

	[JsonPropertyName("offer_payment_mode")]
	public string? OfferPaymentMode { get; set; }

	[JsonPropertyName("price")]
	public Price? Price { get; set; }

	[JsonPropertyName("price_per_day")]
	public Price? PricePerDay { get; set; }

	[JsonPropertyName("price_per_week")]
	public Price? PricePerWeek { get; set; }

	[JsonPropertyName("price_per_month")]
	public Price? PricePerMonth { get; set; }

	[JsonPropertyName("price_per_year")]
	public Price? PricePerYear { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
