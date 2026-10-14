#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public partial class PaywallUiConfig
{
	[JsonPropertyName("app")]
	public PaywallAppConfig App { get; set; } = new();

	[JsonPropertyName("localizations")]
	public Dictionary<string, Dictionary<string, string>> Localizations { get; set; } = new();

	[JsonPropertyName("variable_config")]
	public PaywallVariableConfig VariableConfig { get; set; } = new();

	[JsonPropertyName("custom_variables")]
	public Dictionary<string, PaywallCustomVariableDefinition> CustomVariables { get; set; } = new();

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallAppConfig
{
	[JsonPropertyName("colors")]
	public Dictionary<string, JsonElement> Colors { get; set; } = new();

	[JsonPropertyName("fonts")]
	public Dictionary<string, PaywallFontConfig> Fonts { get; set; } = new();
}

public partial class PaywallFontConfig
{
	[JsonPropertyName("ios")]
	public PaywallFontInfo? Ios { get; set; }

	[JsonPropertyName("android")]
	public PaywallFontInfo? Android { get; set; }

	[JsonPropertyName("web")]
	public PaywallFontInfo? Web { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallFontInfo
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("value")]
	public string? Value { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("hash")]
	public string? Hash { get; set; }

	[JsonPropertyName("family")]
	public string? Family { get; set; }

	[JsonPropertyName("weight")]
	public int? Weight { get; set; }

	[JsonPropertyName("style")]
	public string? Style { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallVariableConfig
{
	[JsonPropertyName("variable_compatibility_map")]
	public Dictionary<string, JsonElement> VariableCompatibilityMap { get; set; } = new();

	[JsonPropertyName("function_compatibility_map")]
	public Dictionary<string, JsonElement> FunctionCompatibilityMap { get; set; } = new();
}

public partial class PaywallCustomVariableDefinition
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("default_value")]
	public JsonElement? DefaultValue { get; set; }
}
