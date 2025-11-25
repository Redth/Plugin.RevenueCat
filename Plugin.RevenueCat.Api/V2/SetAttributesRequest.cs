using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public class SetAttributesRequest
{
	[JsonPropertyName("attributes")]
	public AttributeItem[] Attributes { get; set; } = Array.Empty<AttributeItem>();

	public class AttributeItem
	{
		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("value")]
		public string Value { get; set; } = string.Empty;
	}
}

