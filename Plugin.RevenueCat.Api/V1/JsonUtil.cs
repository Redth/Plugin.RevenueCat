﻿using Plugin.RevenueCat.Api.V1.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Api.V1;

public static class JsonUtil
{
	[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Generic JSON deserialization may require types that cannot be statically analyzed.")]
	[System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Generic JSON deserialization may require runtime code generation.")]
	public static T? Deserialize<T>(this string? json) => json is null ? default : JsonSerializer.Deserialize<T>(json, Settings);

	[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Generic JSON serialization may require types that cannot be statically analyzed.")]
	[System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Generic JSON serialization may require runtime code generation.")]
	public static string Serialize<T>(this T self) => JsonSerializer.Serialize<T>(self, Settings);

	public static readonly JsonSerializerOptions Settings = ApiV1SerializerContext.Default.Options;
}

