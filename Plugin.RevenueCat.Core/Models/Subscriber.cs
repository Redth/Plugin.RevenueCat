#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public partial class Subscriber
{
	[JsonPropertyName("entitlements")]
	public IDictionary<string, Entitlement> Entitlements { get; set; } = new Dictionary<string, Entitlement>();

	[JsonPropertyName("first_seen")]
	public DateTimeOffset FirstSeen { get; set; }

	[JsonPropertyName("last_seen")]
	public DateTimeOffset LastSeen { get; set; }

	[JsonPropertyName("management_url")]
	public string ManagementUrl { get; set; }

	[JsonPropertyName("non_subscriptions")]
	public NonSubscriptions NonSubscriptions { get; set; }

	[JsonPropertyName("original_app_user_id")]
	public string OriginalAppUserId { get; set; }

	[JsonPropertyName("original_application_version")]
	public string OriginalApplicationVersion { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset OriginalPurchaseDate { get; set; }

	[JsonPropertyName("other_purchases")]
	public NonSubscriptions OtherPurchases { get; set; }

	[JsonPropertyName("subscriptions")]
	public IDictionary<string, Subscription> Subscriptions { get; set; } = new Dictionary<string, Subscription>();
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
