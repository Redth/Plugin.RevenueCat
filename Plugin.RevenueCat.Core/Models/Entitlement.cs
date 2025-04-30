#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;
using System.Text.Json.Serialization;

public partial class Entitlement
{
	[JsonPropertyName("expires_date")]
	public DateTimeOffset ExpiresDate { get; set; }

	[JsonPropertyName("grace_period_expires_date")]
	public DateTimeOffset? GracePeriodExpiresDate { get; set; }

	[JsonPropertyName("product_identifier")]
	public string ProductIdentifier { get; set; }

	[JsonPropertyName("purchase_date")]
	public DateTimeOffset PurchaseDate { get; set; }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
