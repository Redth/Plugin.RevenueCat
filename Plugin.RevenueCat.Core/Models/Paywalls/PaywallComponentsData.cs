#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public partial class PaywallComponentsData
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("template_name")]
	public string? TemplateName { get; set; }

	[JsonPropertyName("asset_base_url")]
	public string? AssetBaseUrl { get; set; }

	[JsonPropertyName("revision")]
	public int Revision { get; set; }

	[JsonPropertyName("components_config")]
	public PaywallComponentsConfig? ComponentsConfig { get; set; }

	[JsonPropertyName("components_localizations")]
	[JsonConverter(typeof(PaywallComponentsLocalizationsConverter))]
	public Dictionary<string, Dictionary<string, PaywallLocalizationData>> ComponentsLocalizations { get; set; } = new();

	[JsonPropertyName("default_locale")]
	public string? DefaultLocale { get; set; }

	[JsonPropertyName("zero_decimal_place_countries")]
	public PaywallZeroDecimalPlaceCountries? ZeroDecimalPlaceCountries { get; set; }

	[JsonPropertyName("exit_offers")]
	public JsonElement? ExitOffers { get; set; }

	[JsonPropertyName("play_store_product_change_mode")]
	public JsonElement? PlayStoreProductChangeMode { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallComponentsConfig
{
	[JsonPropertyName("base")]
	public PaywallRootComponents? Base { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallRootComponents
{
	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }

	[JsonPropertyName("header")]
	public PaywallComponent? Header { get; set; }

	[JsonPropertyName("sticky_footer")]
	public PaywallComponent? StickyFooter { get; set; }

	[JsonPropertyName("background")]
	public JsonElement? Background { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallZeroDecimalPlaceCountries
{
	[JsonPropertyName("apple")]
	public List<string> Apple { get; set; } = new();

	[JsonPropertyName("google")]
	public List<string> Google { get; set; } = new();
}

[JsonConverter(typeof(PaywallLocalizationDataConverter))]
public sealed class PaywallLocalizationData
{
	public string? Text { get; set; }

	public JsonElement? Value { get; set; }

	[JsonIgnore]
	public bool IsText => Text is not null;
}

public sealed class PaywallLocalizationDataConverter : JsonConverter<PaywallLocalizationData>
{
	public override PaywallLocalizationData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			return new PaywallLocalizationData { Text = reader.GetString() };
		}

		using var document = JsonDocument.ParseValue(ref reader);
		return new PaywallLocalizationData { Value = document.RootElement.Clone() };
	}

	public override void Write(Utf8JsonWriter writer, PaywallLocalizationData value, JsonSerializerOptions options)
	{
		if (value.Text is not null)
		{
			writer.WriteStringValue(value.Text);
			return;
		}

		if (value.Value is { } element)
		{
			element.WriteTo(writer);
			return;
		}

		writer.WriteNullValue();
	}
}

public sealed class PaywallComponentsLocalizationsConverter : JsonConverter<Dictionary<string, Dictionary<string, PaywallLocalizationData>>>
{
	public override Dictionary<string, Dictionary<string, PaywallLocalizationData>> Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return new Dictionary<string, Dictionary<string, PaywallLocalizationData>>();
		}

		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind == JsonValueKind.Array)
		{
			return new Dictionary<string, Dictionary<string, PaywallLocalizationData>>();
		}

		if (root.ValueKind != JsonValueKind.Object)
		{
			throw new JsonException("Expected paywall component localizations to be a JSON object or an empty array.");
		}

		var localizations = new Dictionary<string, Dictionary<string, PaywallLocalizationData>>(StringComparer.Ordinal);
		foreach (var localeProperty in root.EnumerateObject())
		{
			var localeValues = new Dictionary<string, PaywallLocalizationData>(StringComparer.Ordinal);

			if (localeProperty.Value.ValueKind == JsonValueKind.Object)
			{
				foreach (var valueProperty in localeProperty.Value.EnumerateObject())
				{
					localeValues[valueProperty.Name] = valueProperty.Value.ValueKind == JsonValueKind.String
						? new PaywallLocalizationData { Text = valueProperty.Value.GetString() }
						: new PaywallLocalizationData { Value = valueProperty.Value.Clone() };
				}
			}

			localizations[localeProperty.Name] = localeValues;
		}

		return localizations;
	}

	public override void Write(
		Utf8JsonWriter writer,
		Dictionary<string, Dictionary<string, PaywallLocalizationData>> value,
		JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		foreach (var locale in value)
		{
			writer.WritePropertyName(locale.Key);
			writer.WriteStartObject();

			foreach (var localization in locale.Value)
			{
				writer.WritePropertyName(localization.Key);
				if (localization.Value.Text is not null)
				{
					writer.WriteStringValue(localization.Value.Text);
				}
				else if (localization.Value.Value is { } element)
				{
					element.WriteTo(writer);
				}
				else
				{
					writer.WriteNullValue();
				}
			}

			writer.WriteEndObject();
		}

		writer.WriteEndObject();
	}
}
