using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Api.V1;

public static class JsonUtil
{
	/// <summary>
	/// Deserializes using <see cref="Settings"/>. Prefer the <see cref="JsonTypeInfo{T}"/> overload
	/// (e.g. <c>ApiV1SerializerContext.Default.CustomerInfo</c>) for trimming/NativeAOT.
	/// </summary>
	[RequiresUnreferencedCode("Generic JSON deserialization may require types that cannot be statically analyzed. Use the JsonTypeInfo<T> overload instead.")]
	[RequiresDynamicCode("Generic JSON deserialization may require runtime code generation. Use the JsonTypeInfo<T> overload instead.")]
	public static T? Deserialize<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);

	/// <summary>
	/// Serializes using <see cref="Settings"/>. Prefer the <see cref="JsonTypeInfo{T}"/> overload
	/// (e.g. <c>ApiV1SerializerContext.Default.CustomerInfo</c>) for trimming/NativeAOT.
	/// </summary>
	[RequiresUnreferencedCode("Generic JSON serialization may require types that cannot be statically analyzed. Use the JsonTypeInfo<T> overload instead.")]
	[RequiresDynamicCode("Generic JSON serialization may require runtime code generation. Use the JsonTypeInfo<T> overload instead.")]
	public static string Serialize<T>(this T self) => JsonSerializer.Serialize<T>(self, Settings);

	/// <summary>AOT-safe deserialization using source-generated metadata.</summary>
	public static T? Deserialize<T>(this string? json, JsonTypeInfo<T> typeInfo) => json is null ? default : JsonSerializer.Deserialize(json, typeInfo);

	/// <summary>AOT-safe serialization using source-generated metadata.</summary>
	public static string Serialize<T>(this T self, JsonTypeInfo<T> typeInfo) => JsonSerializer.Serialize(self, typeInfo);

	/// <summary>
	/// V1 API options. Resolves V1 API types first, then falls back to every Core model in
	/// <see cref="ModelSerializerContext"/> so any public model can round-trip.
	/// </summary>
	public static readonly JsonSerializerOptions Settings = CreateSettings();

	static JsonSerializerOptions CreateSettings()
	{
		var options = new JsonSerializerOptions(ApiV1SerializerContext.Default.Options)
		{
			TypeInfoResolver = JsonTypeInfoResolver.Combine(ApiV1SerializerContext.Default, ModelSerializerContext.Default),
		};
		options.MakeReadOnly();
		return options;
	}
}
