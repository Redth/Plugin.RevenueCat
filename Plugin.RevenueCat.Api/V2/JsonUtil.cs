using Plugin.RevenueCat.Api.V2.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V2;

public static class JsonUtil
{
	public static T? Deserialize<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);

	public static string Serialize<T>(this T self) => JsonSerializer.Serialize<T>(self, Settings);

	public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
	{
		Converters =
		{
			new EpochMsDateTimeOffsetConverter(),
		},
		NumberHandling = JsonNumberHandling.AllowReadingFromString
	};
}

