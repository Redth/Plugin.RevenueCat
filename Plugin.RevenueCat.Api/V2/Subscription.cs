using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

/// <summary>
/// Represents a subscription purchased by a customer.
/// </summary>
public class Subscription
{
	/// <summary>
	/// The unique identifier for this subscription.
	/// </summary>
	[JsonPropertyName("id")]
	public string Id { get; set; }

	/// <summary>
	/// The object type. Always "subscription".
	/// </summary>
	[JsonPropertyName("object")]
	public string Object { get; set; }

	/// <summary>
	/// The app user ID of the customer who owns this subscription.
	/// </summary>
	[JsonPropertyName("customer_id")]
	public string CustomerId { get; set; }

	/// <summary>
	/// The product identifier for this subscription.
	/// </summary>
	[JsonPropertyName("product_id")]
	public string ProductId { get; set; }

	/// <summary>
	/// When the subscription originally started.
	/// </summary>
	[JsonPropertyName("starts_at")]
	public DateTimeOffset? StartsAt { get; set; }

	/// <summary>
	/// When the subscription was originally purchased.
	/// </summary>
	[JsonPropertyName("purchased_at")]
	public DateTimeOffset? PurchasedAt { get; set; }

	/// <summary>
	/// When the current billing period started.
	/// </summary>
	[JsonPropertyName("current_period_starts_at")]
	public DateTimeOffset? CurrentPeriodStartsAt { get; set; }

	/// <summary>
	/// When the current billing period ends.
	/// </summary>
	[JsonPropertyName("current_period_ends_at")]
	public DateTimeOffset? CurrentPeriodEndsAt { get; set; }

	/// <summary>
	/// When the subscription expires, or <c>null</c> if not applicable.
	/// </summary>
	[JsonPropertyName("expires_at")]
	public DateTimeOffset? ExpiresAt { get; set; }

	/// <summary>
	/// Whether this subscription currently gives access to entitled content.
	/// </summary>
	[JsonPropertyName("gives_access")]
	public bool GivesAccess { get; set; }

	/// <summary>
	/// Whether there is a pending payment for this subscription.
	/// </summary>
	[JsonPropertyName("pending_payment")]
	public bool PendingPayment { get; set; }

	/// <summary>
	/// The auto-renewal status of the subscription (e.g., "will_renew", "will_not_renew").
	/// </summary>
	[JsonPropertyName("auto_renewal_status")]
	public string AutoRenewalStatus { get; set; }

	/// <summary>
	/// The current status of the subscription (e.g., "active", "expired", "in_grace_period").
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; }

	/// <summary>
	/// The store where this subscription was purchased (e.g., "app_store", "play_store", "stripe").
	/// </summary>
	[JsonPropertyName("store")]
	public string Store { get; set; }

	/// <summary>
	/// The environment where this subscription exists (e.g., "production", "sandbox").
	/// </summary>
	[JsonPropertyName("environment")]
	public string Environment { get; set; }

	/// <summary>
	/// Whether this is a sandbox/test subscription.
	/// </summary>
	[JsonPropertyName("is_sandbox")]
	public bool IsSandbox { get; set; }

	/// <summary>
	/// When cancellation was requested, or <c>null</c> if not cancelled.
	/// </summary>
	[JsonPropertyName("cancellation_at")]
	public DateTimeOffset? CancellationAt { get; set; }

	/// <summary>
	/// The reason for cancellation, or <c>null</c> if not cancelled or reason not provided.
	/// </summary>
	[JsonPropertyName("cancel_reason")]
	public string? CancelReason { get; set; }

	/// <summary>
	/// The entitlement IDs unlocked by this subscription.
	/// </summary>
	[JsonPropertyName("entitlement_ids")]
	public List<string> EntitlementIds { get; set; } = new();

	/// <summary>
	/// The URL where the customer can manage this subscription, or <c>null</c> if not available.
	/// </summary>
	/// <remarks>
	/// For App Store subscriptions, this links to the iOS subscription management page.
	/// For Play Store subscriptions, this links to the Google Play subscription center.
	/// Some stores may not provide a management URL.
	/// </remarks>
	[JsonPropertyName("management_url")]
	public string? ManagementUrl { get; set; }
}
