using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Plugin.RevenueCat.Api.V2;

public static class JsonUtil
{
	/// <summary>
	/// Deserializes using <see cref="Settings"/>. Prefer the <see cref="JsonTypeInfo{T}"/> overload
	/// (e.g. <c>ApiV2SerializerContext.Default.Customer</c>) for trimming/NativeAOT.
	/// </summary>
	[RequiresUnreferencedCode("Generic JSON deserialization may require types that cannot be statically analyzed. Use the JsonTypeInfo<T> overload instead.")]
	[RequiresDynamicCode("Generic JSON deserialization may require runtime code generation. Use the JsonTypeInfo<T> overload instead.")]
	public static T? Deserialize<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);

	/// <summary>
	/// Serializes using <see cref="Settings"/>. Prefer the <see cref="JsonTypeInfo{T}"/> overload
	/// (e.g. <c>ApiV2SerializerContext.Default.Customer</c>) for trimming/NativeAOT.
	/// </summary>
	[RequiresUnreferencedCode("Generic JSON serialization may require types that cannot be statically analyzed. Use the JsonTypeInfo<T> overload instead.")]
	[RequiresDynamicCode("Generic JSON serialization may require runtime code generation. Use the JsonTypeInfo<T> overload instead.")]
	public static string Serialize<T>(this T self) => JsonSerializer.Serialize<T>(self, Settings);

	/// <summary>AOT-safe deserialization using source-generated metadata.</summary>
	public static T? Deserialize<T>(this string? json, JsonTypeInfo<T> typeInfo) => json is null ? default : JsonSerializer.Deserialize(json, typeInfo);

	/// <summary>AOT-safe serialization using source-generated metadata.</summary>
	public static string Serialize<T>(this T self, JsonTypeInfo<T> typeInfo) => JsonSerializer.Serialize(self, typeInfo);

	public static readonly JsonSerializerOptions Settings = ApiV2SerializerContext.Default.Options;
}
