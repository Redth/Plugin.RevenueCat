using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class Entitlement
{
	[JsonPropertyName("entitlement_id")]
	public string Id { get; set; }

	[JsonPropertyName("expires_at")]
	public DateTimeOffset ExpiresAt { get; set; }
}
