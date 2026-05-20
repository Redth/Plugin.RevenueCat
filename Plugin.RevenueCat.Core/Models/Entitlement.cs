#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class Entitlement
{
	[JsonPropertyName("identifier")]
	public string? Identifier { get; set; }

	[JsonPropertyName("expires_date")]
	public DateTimeOffset? ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("product_identifier")]
	public string? ProductIdentifier { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset? PurchaseDate { get; set; }

	[JsonPropertyName("latest_purchase_date")]
	public DateTimeOffset? LatestPurchaseDate { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset? OriginalPurchaseDate { get; set; }

	[JsonPropertyName("is_active")]
	public bool? IsActive { get; set; }

	[JsonPropertyName("will_renew")]
	public bool? WillRenew { get; set; }

	[JsonPropertyName("period_type")]
	public string? PeriodType { get; set; }

	[JsonPropertyName("store")]
	public string? Store { get; set; }

	[JsonPropertyName("is_sandbox")]
	public bool? IsSandbox { get; set; }

	[JsonPropertyName("unsubscribe_detected_at")]
	public DateTimeOffset? UnsubscribeDetectedAt { get; set; }

	[JsonPropertyName("billing_issue_detected_at")]
	public DateTimeOffset? BillingIssueDetectedAt { get; set; }

	[JsonPropertyName("billing_issues_detected_at")]
	public DateTimeOffset? BillingIssuesDetectedAt { get; set; }

	[JsonPropertyName("ownership_type")]
	public string? OwnershipType { get; set; }

	[JsonPropertyName("product_plan_identifier")]
	public string? ProductPlanIdentifier { get; set; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
