using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public class SubscriptionOption
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("product_id")]
	public string? ProductId { get; set; }

	[JsonPropertyName("base_plan_id")]
	public string? BasePlanId { get; set; }

	[JsonPropertyName("offer_id")]
	public string? OfferId { get; set; }

	[JsonPropertyName("offer_token")]
	public string? OfferToken { get; set; }

	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = new();

	[JsonPropertyName("is_base_plan")]
	public bool? IsBasePlan { get; set; }

	[JsonPropertyName("is_prepaid")]
	public bool? IsPrepaid { get; set; }

	[JsonPropertyName("billing_period")]
	public SubscriptionPeriod? BillingPeriod { get; set; }

	[JsonPropertyName("pricing_phases")]
	public List<PricingPhase> PricingPhases { get; set; } = new();

	[JsonPropertyName("free_phase")]
	public PricingPhase? FreePhase { get; set; }

	[JsonPropertyName("intro_phase")]
	public PricingPhase? IntroPhase { get; set; }

	[JsonPropertyName("full_price_phase")]
	public PricingPhase? FullPricePhase { get; set; }

	[JsonPropertyName("installments_info")]
	public InstallmentsInfo? InstallmentsInfo { get; set; }

	[JsonPropertyName("presented_offering_identifier")]
	public string? PresentedOfferingIdentifier { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
