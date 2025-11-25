﻿#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Plugin.RevenueCat.Models;

using Plugin.RevenueCat.Core.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ModelExtensions
{
	// Generic version kept for compatibility but marked as problematic for trimming/AOT
	[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Generic JSON deserialization may require types that cannot be statically analyzed. Use type-specific overloads instead.")]
	[System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Generic JSON deserialization may require runtime code generation. Use type-specific overloads instead.")]
	public static T? ToModel<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);

	// AOT-safe type-specific overloads
	public static CustomerInfo? ToCustomerInfo(this string? json) => json is null ? default : JsonSerializer.Deserialize(json, ModelSerializerContext.Default.CustomerInfo);

	public static Offering? ToOffering(this string? json) => json is null ? default : JsonSerializer.Deserialize(json, ModelSerializerContext.Default.Offering);

	public static string ToJson(this CustomerInfo self) => JsonSerializer.Serialize(self, ModelSerializerContext.Default.CustomerInfo);

	public static string ToJson(this Offering self) => JsonSerializer.Serialize(self, ModelSerializerContext.Default.Offering);

	public static readonly JsonSerializerOptions Settings = ModelSerializerContext.Default.Options;
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603
