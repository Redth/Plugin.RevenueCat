#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public partial class PaywallOfferingsResponse
{
	[JsonPropertyName("current_offering_id")]
	public string? CurrentOfferingId { get; set; }

	[JsonPropertyName("offerings")]
	public List<PaywallOffering> Offerings { get; set; } = new();

	[JsonPropertyName("placements")]
	public PaywallPlacements? Placements { get; set; }

	[JsonPropertyName("targeting")]
	public PaywallTargeting? Targeting { get; set; }

	[JsonPropertyName("ui_config")]
	public PaywallUiConfig? UiConfig { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }

	[JsonIgnore]
	public PaywallOffering? CurrentOffering => CurrentOfferingId is null
		? null
		: Offerings.FirstOrDefault(o => string.Equals(o.Identifier, CurrentOfferingId, StringComparison.Ordinal));
}

public partial class PaywallOffering
{
	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("metadata")]
	public Dictionary<string, JsonElement> Metadata { get; set; } = new();

	[JsonPropertyName("packages")]
	public List<PaywallPackage> Packages { get; set; } = new();

	[JsonPropertyName("paywall")]
	public LegacyPaywallData? Paywall { get; set; }

	[JsonPropertyName("paywall_components")]
	public PaywallComponentsData? PaywallComponents { get; set; }

	[JsonPropertyName("draft_paywall_components")]
	public PaywallComponentsData? DraftPaywallComponents { get; set; }

	[JsonPropertyName("web_checkout_url")]
	public string? WebCheckoutUrl { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallPackage
{
	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("platform_product_identifier")]
	public string? PlatformProductIdentifier { get; set; }

	[JsonPropertyName("platform_product_plan_identifier")]
	public string? PlatformProductPlanIdentifier { get; set; }

	[JsonPropertyName("web_checkout_url")]
	public string? WebCheckoutUrl { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallPlacements
{
	[JsonPropertyName("fallback_offering_id")]
	public string? FallbackOfferingId { get; set; }

	[JsonPropertyName("offering_ids_by_placement")]
	public Dictionary<string, string?> OfferingIdsByPlacement { get; set; } = new();
}

public partial class PaywallTargeting
{
	[JsonPropertyName("revision")]
	public int? Revision { get; set; }

	[JsonPropertyName("rule_id")]
	public string? RuleId { get; set; }
}

public partial class LegacyPaywallData
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("template_name")]
	public string? TemplateName { get; set; }

	[JsonPropertyName("config")]
	public JsonElement? Config { get; set; }

	[JsonPropertyName("asset_base_url")]
	public string? AssetBaseUrl { get; set; }

	[JsonPropertyName("revision")]
	public int Revision { get; set; }

	[JsonPropertyName("localized_strings")]
	public Dictionary<string, JsonElement> LocalizedStrings { get; set; } = new();

	[JsonPropertyName("localized_strings_by_tier")]
	public Dictionary<string, Dictionary<string, JsonElement>> LocalizedStringsByTier { get; set; } = new();

	[JsonPropertyName("default_locale")]
	public string? DefaultLocale { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
