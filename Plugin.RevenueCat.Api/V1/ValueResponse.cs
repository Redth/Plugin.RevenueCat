using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public class OfferingsResponse
{
	[JsonPropertyName("current_offering_id")]
	public string CurrentOfferingId { get; set; }

	[JsonPropertyName("offerings")]
	public List<Models.Offering> Offerings { get; set; } = new();
}
