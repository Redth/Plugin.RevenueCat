#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public sealed class PaywallComponentConverter : JsonConverter<PaywallComponent>
{
	public override PaywallComponent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}

		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object)
		{
			return new PaywallUnknownComponent { Raw = root.Clone() };
		}

		var componentType = root.TryGetProperty("type", out var typeProperty)
			? typeProperty.GetString()
			: null;

		var component = DeserializeComponent(root, componentType);
		if (component is null)
		{
			var unknown = new PaywallUnknownComponent
			{
				Type = componentType,
				Raw = root.Clone()
			};

			if (root.TryGetProperty("fallback", out var fallback) && fallback.ValueKind == JsonValueKind.Object)
			{
				unknown.Fallback = JsonSerializer.Deserialize(fallback.GetRawText(), ModelSerializerContext.Default.PaywallComponent);
			}

			return unknown;
		}

		return component;
	}

	public override void Write(Utf8JsonWriter writer, PaywallComponent value, JsonSerializerOptions options)
	{
		if (value is PaywallUnknownComponent { Raw.ValueKind: not JsonValueKind.Undefined } unknownComponent)
		{
			unknownComponent.Raw.WriteTo(writer);
			return;
		}

		switch (value)
		{
			case PaywallButtonComponent button:
				JsonSerializer.Serialize(writer, button, ModelSerializerContext.Default.PaywallButtonComponent);
				break;
			case PaywallCarouselComponent carousel:
				JsonSerializer.Serialize(writer, carousel, ModelSerializerContext.Default.PaywallCarouselComponent);
				break;
			case PaywallCountdownComponent countdown:
				JsonSerializer.Serialize(writer, countdown, ModelSerializerContext.Default.PaywallCountdownComponent);
				break;
			case PaywallFallbackHeaderComponent fallbackHeader:
				JsonSerializer.Serialize(writer, fallbackHeader, ModelSerializerContext.Default.PaywallFallbackHeaderComponent);
				break;
			case PaywallHeaderComponent header:
				JsonSerializer.Serialize(writer, header, ModelSerializerContext.Default.PaywallHeaderComponent);
				break;
			case PaywallIconComponent icon:
				JsonSerializer.Serialize(writer, icon, ModelSerializerContext.Default.PaywallIconComponent);
				break;
			case PaywallImageComponent image:
				JsonSerializer.Serialize(writer, image, ModelSerializerContext.Default.PaywallImageComponent);
				break;
			case PaywallPackageComponent package:
				JsonSerializer.Serialize(writer, package, ModelSerializerContext.Default.PaywallPackageComponent);
				break;
			case PaywallPurchaseButtonComponent purchaseButton:
				JsonSerializer.Serialize(writer, purchaseButton, ModelSerializerContext.Default.PaywallPurchaseButtonComponent);
				break;
			case PaywallStackComponent stack:
				JsonSerializer.Serialize(writer, stack, ModelSerializerContext.Default.PaywallStackComponent);
				break;
			case PaywallStickyFooterComponent stickyFooter:
				JsonSerializer.Serialize(writer, stickyFooter, ModelSerializerContext.Default.PaywallStickyFooterComponent);
				break;
			case PaywallTabComponent tab:
				JsonSerializer.Serialize(writer, tab, ModelSerializerContext.Default.PaywallTabComponent);
				break;
			case PaywallTabControlButtonComponent tabControlButton:
				JsonSerializer.Serialize(writer, tabControlButton, ModelSerializerContext.Default.PaywallTabControlButtonComponent);
				break;
			case PaywallTabControlComponent tabControl:
				JsonSerializer.Serialize(writer, tabControl, ModelSerializerContext.Default.PaywallTabControlComponent);
				break;
			case PaywallTabControlToggleComponent tabControlToggle:
				JsonSerializer.Serialize(writer, tabControlToggle, ModelSerializerContext.Default.PaywallTabControlToggleComponent);
				break;
			case PaywallTabsComponent tabs:
				JsonSerializer.Serialize(writer, tabs, ModelSerializerContext.Default.PaywallTabsComponent);
				break;
			case PaywallTextComponent text:
				JsonSerializer.Serialize(writer, text, ModelSerializerContext.Default.PaywallTextComponent);
				break;
			case PaywallTimelineComponent timeline:
				JsonSerializer.Serialize(writer, timeline, ModelSerializerContext.Default.PaywallTimelineComponent);
				break;
			case PaywallUnknownComponent unknownForSerialization:
				JsonSerializer.Serialize(writer, unknownForSerialization, ModelSerializerContext.Default.PaywallUnknownComponent);
				break;
			case PaywallVideoComponent video:
				JsonSerializer.Serialize(writer, video, ModelSerializerContext.Default.PaywallVideoComponent);
				break;
			default:
				writer.WriteNullValue();
				break;
		}
	}

	static PaywallComponent? DeserializeComponent(JsonElement root, string? componentType)
	{
		var json = root.GetRawText();

		return componentType switch
		{
			"button" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallButtonComponent),
			"carousel" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallCarouselComponent),
			"countdown" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallCountdownComponent),
			"fallback_header" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallFallbackHeaderComponent),
			"footer" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallStickyFooterComponent),
			"header" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallHeaderComponent),
			"icon" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallIconComponent),
			"image" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallImageComponent),
			"package" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallPackageComponent),
			"purchase_button" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallPurchaseButtonComponent),
			"stack" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallStackComponent),
			"sticky_footer" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallStickyFooterComponent),
			"tab" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTabComponent),
			"tab_control" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTabControlComponent),
			"tab_control_button" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTabControlButtonComponent),
			"tab_control_toggle" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTabControlToggleComponent),
			"tabs" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTabsComponent),
			"text" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTextComponent),
			"timeline" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallTimelineComponent),
			"video" => JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallVideoComponent),
			_ => null
		};
	}
}
