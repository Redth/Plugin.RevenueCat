using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public class Entitlement
{
	[JsonPropertyName("expires_date")]
	public DateTimeOffset? ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("product_identifier")]
	public string ProductIdentifier { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset? PurchaseDate { get; set; }
}
