using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public class Subscription
{
	[JsonPropertyName("auto_resume_date")]
	public DateTimeOffset? AutoResumeDate { get; set; }

	[JsonPropertyName("billing_issues_detected_at")]
	public DateTimeOffset? BillingIssuesDetectedAt { get; set; }

	[JsonPropertyName("expires_date")]
	public DateTimeOffset? ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("is_sandbox")]
	public bool IsSandbox { get; set; }

	[JsonPropertyName("store")]
	public string Store { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset? OriginalPurchaseDate { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset? PurchaseDate { get; set; }

	[JsonPropertyName("refunded_at")]
	public DateTimeOffset? RefundedAt { get; set; }

	[JsonPropertyName("unsubscribe_detected_at")]
	public DateTimeOffset? UnsubscribeDetectedAt { get; set; }

	[JsonPropertyName("ownership_type")]
	public string OwnershipType { get; set; }

	[JsonPropertyName("period_type")]
	public string PeriodType { get; set; }

	[JsonPropertyName("store_transaction_id")]
	public string StoreTransactionId { get; set; }
}