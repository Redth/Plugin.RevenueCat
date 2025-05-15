using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2.Converters;

public class EpochMsDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

	
	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		var ms = value.ToUnixTimeMilliseconds();
		writer.WriteNumberValue(ms);
	}

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TryGetInt64(out var ms))
		{
			return DateTimeOffset.FromUnixTimeMilliseconds(ms);
		}

		return default;
	}
}
