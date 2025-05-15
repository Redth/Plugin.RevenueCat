using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Plugin.RevenueCat.Api.V1;

public class Subscriber
{
	[JsonPropertyName("original_app_user_id")]
	public string? OriginalAppUserId { get; set; }

	[JsonPropertyName("original_application_version")]
	public string? OriginalApplicationVersion { get; set; }

	[JsonPropertyName("first_seen")]
	public DateTimeOffset? FirstSeen { get; set; }

	[JsonPropertyName("original_purchase_date")]
	public DateTimeOffset? OriginalPurchaseDate { get; set; }

	[JsonPropertyName("management_url")]
	public string? ManagementUrl { get; set; }

	[JsonPropertyName("entitlements")]
	public Dictionary<string, Entitlement> Entitlements { get; set; } = new();

	[JsonPropertyName("subscriptions")]
	public Dictionary<string, Subscription> Subscriptions { get; set; } = new();

	[JsonPropertyName("non_subscriptions")]
	public Dictionary<string, NonSubscription> NonSubscriptions { get; set; } = new();
}

public class SubscriberAttributes
{
	[JsonPropertyName("attributes")]
	public Dictionary<string, SubscriberAttribute> Attributes { get; set; } = new();
}

public class SubscriberAttribute
{
	[JsonPropertyName("value")]
	public string Value { get; set; }

	[JsonPropertyName("updated_at_ms")]
	public long UpdatedAtMs { get; set; }

	[JsonIgnore]
	public DateTimeOffset? UpdatedAt
	{
		get => DateTimeOffset.FromUnixTimeMilliseconds(UpdatedAtMs);
		set => UpdatedAtMs = value?.ToUnixTimeMilliseconds() ?? 0;
	}
}