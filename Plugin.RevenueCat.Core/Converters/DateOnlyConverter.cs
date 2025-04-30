#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

using Plugin;

namespace Plugin.RevenueCat.Core.Converters;

using System;

using System.Text.Json;
using System.Text.Json.Serialization;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
	private readonly string serializationFormat;
	public DateOnlyConverter() : this(null) { }

	public DateOnlyConverter(string? serializationFormat)
	{
		this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
	}

	public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return DateOnly.Parse(value!);
	}

	public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(serializationFormat));
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
