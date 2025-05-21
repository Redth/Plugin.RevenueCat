#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Core.Converters;

using System;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

	private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

	private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
	private string? _dateTimeFormat;
	private CultureInfo? _culture;

	public DateTimeStyles DateTimeStyles
	{
		get => _dateTimeStyles;
		set => _dateTimeStyles = value;
	}

	public string? DateTimeFormat
	{
		get => _dateTimeFormat ?? string.Empty;
		set => _dateTimeFormat = string.IsNullOrEmpty(value) ? null : value;
	}

	public CultureInfo Culture
	{
		get => _culture ?? CultureInfo.CurrentCulture;
		set => _culture = value;
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		string text;

		if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
				|| (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
		{
			value = value.ToUniversalTime();
		}

		text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

		writer.WriteStringValue(text);
	}

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Try and read MS if it is a number
		if (reader.TokenType == JsonTokenType.Number)
		{
			long milliseconds = reader.GetInt64();
			return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
		}

		string? dateText = reader.GetString();

		if (string.IsNullOrEmpty(dateText) == false)
		{
			if (!string.IsNullOrEmpty(_dateTimeFormat))
			{
				return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
			}
			else
			{
				return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
			}
		}
		else
		{
			return default;
		}
	}
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
