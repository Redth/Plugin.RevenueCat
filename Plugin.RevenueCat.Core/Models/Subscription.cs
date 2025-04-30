#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;
using System.Text.Json.Serialization;

public partial class Subscription
{
	[JsonPropertyName("auto_resume_date")]
	public DateTimeOffset? AutoResumeDate { get; set; }

	[JsonPropertyName("billing_issues_detected_at")]
	public DateTimeOffset? BillingIssuesDetectedAt { get; set; }

	[JsonPropertyName("display_name")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("expires_date")]
	public DateTimeOffset ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("is_sandbox")]
	public bool IsSandbox { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset OriginalPurchaseDate { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("ownership_type")]
	public string OwnershipType { get; set; }

	[JsonPropertyName("period_type")]
	public string PeriodType { get; set; }

	[JsonPropertyName("price")]
	public Price Price { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("product_plan_identifier")]
	public string ProductPlanIdentifier { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset PurchaseDate { get; set; }

	[JsonPropertyName("refunded_at")]
	public DateTimeOffset? RefundedAt { get; set; }

	[JsonPropertyName("store")]
	public string Store { get; set; }

	[JsonPropertyName("store_transaction_id")]
	public string StoreTransactionId { get; set; }

	[JsonPropertyName("unsubscribe_detected_at")]
	public DateTimeOffset? UnsubscribeDetectedAt { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
