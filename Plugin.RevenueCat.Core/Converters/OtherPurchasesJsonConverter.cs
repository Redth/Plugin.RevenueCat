using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Core.Converters;

public sealed class OtherPurchasesJsonConverter : JsonConverter<Dictionary<string, List<NonSubscription>>>
{
	public override Dictionary<string, List<NonSubscription>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		if (document.RootElement.ValueKind != JsonValueKind.Object)
			throw new JsonException("Expected a dictionary of other purchases.");

		var purchaseType = (JsonTypeInfo<NonSubscription>)options.GetTypeInfo(typeof(NonSubscription));
		var listType = (JsonTypeInfo<List<NonSubscription>>)options.GetTypeInfo(typeof(List<NonSubscription>));
		var purchases = new Dictionary<string, List<NonSubscription>>();
		foreach (var property in document.RootElement.EnumerateObject())
		{
			purchases[property.Name] = property.Value.ValueKind switch
			{
				JsonValueKind.Object => new() { JsonSerializer.Deserialize(property.Value, purchaseType)! },
				JsonValueKind.Array or JsonValueKind.Null => JsonSerializer.Deserialize(property.Value, listType)!,
				_ => throw new JsonException("Expected a purchase object or array.")
			};
		}
		return purchases;
	}

	public override void Write(Utf8JsonWriter writer, Dictionary<string, List<NonSubscription>> value, JsonSerializerOptions options)
	{
		var listType = (JsonTypeInfo<List<NonSubscription>>)options.GetTypeInfo(typeof(List<NonSubscription>));
		writer.WriteStartObject();
		foreach (var purchase in value)
		{
			writer.WritePropertyName(options.DictionaryKeyPolicy?.ConvertName(purchase.Key) ?? purchase.Key);
			JsonSerializer.Serialize(writer, purchase.Value, listType);
		}
		writer.WriteEndObject();
	}
}
