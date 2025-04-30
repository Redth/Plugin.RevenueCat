#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

using Plugin;

namespace Plugin.RevenueCat.Core.Converters;

using System;

using System.Text.Json;
using System.Text.Json.Serialization;

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
	private readonly string serializationFormat;

	public TimeOnlyConverter() : this(null) { }

	public TimeOnlyConverter(string? serializationFormat)
	{
		this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
	}

	public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return TimeOnly.Parse(value!);
	}

	public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(serializationFormat));
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
