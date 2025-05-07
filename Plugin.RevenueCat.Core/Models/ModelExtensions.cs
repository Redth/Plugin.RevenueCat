#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using Plugin.RevenueCat.Core.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ModelExtensions
{
	public static T? ToModel<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);
	
	public static string ToJson(this CustomerInfoRequest self) => JsonSerializer.Serialize(self, Settings);

	public static string ToJson(this Offering self) => JsonSerializer.Serialize(self, Settings);

	public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
	{
		Converters =
		{
			new DateOnlyConverter(),
			new TimeOnlyConverter(),
			IsoDateTimeOffsetConverter.Singleton
		},
		NumberHandling = JsonNumberHandling.AllowReadingFromString
	};
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
