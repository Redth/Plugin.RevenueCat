using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class Subscription
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("customer_id")]
	public string CustomerId { get; set; }

	[JsonPropertyName("product_id")]
	public string ProductId { get; set; }

	[JsonPropertyName("starts_at")]
	public DateTimeOffset? StartsAt { get; set; }

	[JsonPropertyName("current_period_starts_at")]
	public DateTimeOffset? CurrentPeriodStartsAt { get; set; }

	[JsonPropertyName("current_period_ends_at")]
	public DateTimeOffset? CurrentPeriodEndsAt { get; set; }

	[JsonPropertyName("gives_access")]
	public bool GivesAccess { get; set; }

	[JsonPropertyName("pending_payment")]
	public bool PendingPayment { get; set; }

	[JsonPropertyName("auto_renewal_status")]
	public string AutoRenewalStatus { get; set; }

	[JsonPropertyName("status")]
	public string Status { get; set; }

	[JsonPropertyName("store")]
	public string Store { get; set; }

	[JsonPropertyName("environment")]
	public string Environment { get; set; }

	[JsonPropertyName("management_url")]
	public string? ManagementUrl { get; set; }
}
