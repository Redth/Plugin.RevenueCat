#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;

using System.Text.Json;
using System.Text.Json.Serialization;

public partial class CustomerInfo
{
	[JsonPropertyName("request_date")]
	public DateTimeOffset? RequestDate { get; set; }

	[JsonPropertyName("request_date_ms")]
	public long? RequestDateMs { get; set; }

	[JsonPropertyName("subscriber")]
	public Subscriber Subscriber { get; set; }

	[JsonPropertyName("schema_version")]
	public int? SchemaVersion { get; set; }

	[JsonPropertyName("verification_result")]
	public string? VerificationResult { get; set; }

	[JsonPropertyName("customer_info_request_date")]
	public JsonElement? CustomerInfoRequestDate { get; set; }

	[JsonPropertyName("active_subscriptions")]
	public List<string> ActiveSubscriptions { get; set; } = new();

	[JsonPropertyName("all_purchased_product_identifiers")]
	public List<string> AllPurchasedProductIdentifiers { get; set; } = new();

	[JsonPropertyName("all_expiration_dates_by_product")]
	public IDictionary<string, DateTimeOffset?> AllExpirationDatesByProduct { get; set; } = new Dictionary<string, DateTimeOffset?>();

	[JsonPropertyName("all_purchase_dates_by_product")]
	public IDictionary<string, DateTimeOffset?> AllPurchaseDatesByProduct { get; set; } = new Dictionary<string, DateTimeOffset?>();

	[JsonPropertyName("latest_expiration_date")]
	public DateTimeOffset? LatestExpirationDate { get; set; }

	[JsonPropertyName("management_url")]
	public string? ManagementUrl { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
