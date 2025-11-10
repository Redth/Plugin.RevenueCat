using Plugin.RevenueCat.Api.V1.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public static class JsonUtil
{
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeInfoResolver is configured with source-generated context")]
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TypeInfoResolver is configured with source-generated context")]
	public static T? Deserialize<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);

	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeInfoResolver is configured with source-generated context")]
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TypeInfoResolver is configured with source-generated context")]
	public static string Serialize<T>(this T self) => JsonSerializer.Serialize<T>(self, Settings);

	public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
	{
		TypeInfoResolver = RevenueCatApiV1JsonContext.Default,
		Converters =
		{
			new IsoDateTimeOffsetConverter(),
			new DateOnlyConverter(),
			new TimeOnlyConverter()
		},
		NumberHandling = JsonNumberHandling.AllowReadingFromString
	};
}

