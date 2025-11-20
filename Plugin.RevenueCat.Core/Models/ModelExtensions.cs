#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using Plugin.RevenueCat.Core.Converters;
using Plugin.RevenueCat.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ModelExtensions
{
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeInfoResolver is configured with source-generated context")]
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TypeInfoResolver is configured with source-generated context")]
	public static T? ToModel<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);
	
	public static string ToJson(this CustomerInfo self) => JsonSerializer.Serialize(self, RevenueCatCoreJsonContext.Default.CustomerInfo);

	public static string ToJson(this Offering self) => JsonSerializer.Serialize(self, RevenueCatCoreJsonContext.Default.Offering);

	public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
	{
		TypeInfoResolver = RevenueCatCoreJsonContext.Default,
		Converters =
		{
			new DateOnlyConverter(),
			new TimeOnlyConverter(),
			new IsoDateTimeOffsetConverter()
		},
		NumberHandling = JsonNumberHandling.AllowReadingFromString
	};
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
