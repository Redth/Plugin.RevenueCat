using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public class SubscriberValueResponse
{
	[JsonPropertyName("value")]
	public SubscriberResponse? Value { get; set; }
}

public class SubscriberResponse
{
	[JsonPropertyName("request_date")]
	public DateTimeOffset? RequestDate { get; set; }

	[JsonPropertyName("subscriber")]
	public Models.Subscriber? Subscriber { get; set; }
}
