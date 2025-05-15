using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class Customer
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("project_id")]
	public string ProjectId { get; set; }

	[JsonPropertyName("first_seen_at")]
	public DateTimeOffset? FirstSeenAt { get; set; }

	[JsonPropertyName("last_seen_at")]
	public DateTimeOffset? LastSeenAt { get; set; }

	[JsonPropertyName("active_entitlements")]
	public PagedList<Entitlement> ActiveEntitlements { get; set; } = new();

	[JsonPropertyName("attributes")]
	public PagedList<CustomerAttribute> Attributes { get; set; } = new();
}
