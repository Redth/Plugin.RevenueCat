#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Api.V2;

public class Paywall
{
	[JsonPropertyName("object")]
	public string Object { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("offering_id")]
	public string OfferingId { get; set; } = string.Empty;

	[JsonPropertyName("created_at")]
	public DateTimeOffset CreatedAt { get; set; }

	[JsonPropertyName("published_at")]
	public DateTimeOffset? PublishedAt { get; set; }

	[JsonPropertyName("offering")]
	public Offering? Offering { get; set; }

	[JsonPropertyName("components")]
	public PaywallComponents? Components { get; set; }
}

public class PaywallComponents
{
	[JsonPropertyName("published")]
	public PaywallComponentsVersion? Published { get; set; }

	[JsonPropertyName("draft")]
	public PaywallComponentsVersion? Draft { get; set; }
}

public class PaywallComponentsVersion
{
	[JsonPropertyName("revision")]
	public int? Revision { get; set; }

	[JsonPropertyName("components_config")]
	public JsonElement? ComponentsConfig { get; set; }

	[JsonPropertyName("default_locale")]
	public string? DefaultLocale { get; set; }

	[JsonPropertyName("components_localizations")]
	[JsonConverter(typeof(PaywallComponentsLocalizationsConverter))]
	public Dictionary<string, Dictionary<string, PaywallLocalizationData>> ComponentsLocalizations { get; set; } = new();

	[JsonPropertyName("fonts")]
	public Dictionary<string, JsonElement>? Fonts { get; set; }

	[JsonIgnore]
	public PaywallComponentsConfig? ParsedComponentsConfig =>
		ComponentsConfig is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } componentsConfig
			? JsonSerializer.Deserialize(componentsConfig.GetRawText(), ModelSerializerContext.Default.PaywallComponentsConfig)
			: null;
}
