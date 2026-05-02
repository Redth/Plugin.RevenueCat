#nullable enable

using System.Globalization;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

static class PaywallMauiStyleResolver
{
	public static string? GetType(JsonElement? element) =>
		element is { ValueKind: JsonValueKind.Object } objectElement &&
		objectElement.TryGetProperty("type", out var type) &&
		type.ValueKind == JsonValueKind.String
			? type.GetString()
			: null;

	public static Thickness ResolveThickness(JsonElement? element)
	{
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
		{
			return new Thickness(0);
		}

		var value = element.Value;
		if (value.ValueKind != JsonValueKind.Object)
		{
			return new Thickness(0);
		}

		return new Thickness(
			GetDouble(value, "leading"),
			GetDouble(value, "top"),
			GetDouble(value, "trailing"),
			GetDouble(value, "bottom"));
	}

	public static Brush? ResolveBackground(JsonElement? element, PaywallUiConfig? uiConfig)
	{
		var color = ResolveColor(element, uiConfig);
		return color is null ? null : new SolidColorBrush(color);
	}

	public static Color? ResolveColor(JsonElement? element, PaywallUiConfig? uiConfig)
	{
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
		{
			return null;
		}

		var value = element.Value;
		if (value.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		if (value.TryGetProperty("value", out var backgroundValue) && backgroundValue.ValueKind == JsonValueKind.Object)
		{
			return ResolveColor(backgroundValue, uiConfig);
		}

		var preferredTheme = Application.Current?.RequestedTheme == AppTheme.Dark ? "dark" : "light";
		if (value.TryGetProperty(preferredTheme, out var themeColor))
		{
			return ResolveColorInfo(themeColor, uiConfig);
		}

		if (value.TryGetProperty("light", out var lightColor))
		{
			return ResolveColorInfo(lightColor, uiConfig);
		}

		return ResolveColorInfo(value, uiConfig);
	}

	static Color? ResolveColorInfo(JsonElement value, PaywallUiConfig? uiConfig)
	{
		var type = GetType(value);
		if (type == "hex" && value.TryGetProperty("value", out var hex) && hex.ValueKind == JsonValueKind.String)
		{
			return ParseRgba(hex.GetString());
		}

		if (type == "alias" &&
			value.TryGetProperty("value", out var alias) &&
			alias.ValueKind == JsonValueKind.String &&
			uiConfig?.App.Colors.TryGetValue(alias.GetString() ?? string.Empty, out var aliasColor) == true)
		{
			return ResolveColor(aliasColor, uiConfig);
		}

		if ((type == "linear" || type == "radial") &&
			value.TryGetProperty("points", out var points) &&
			points.ValueKind == JsonValueKind.Array)
		{
			var first = points.EnumerateArray().FirstOrDefault();
			if (first.ValueKind == JsonValueKind.Object &&
				first.TryGetProperty("color", out var pointColor) &&
				pointColor.ValueKind == JsonValueKind.String)
			{
				return ParseRgba(pointColor.GetString());
			}
		}

		return null;
	}

	public static TextAlignment ResolveTextAlignment(string? alignment) => alignment switch
	{
		"leading" => TextAlignment.Start,
		"trailing" => TextAlignment.End,
		_ => TextAlignment.Center
	};

	public static double ResolveFontSize(JsonElement? element)
	{
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
		{
			return 15;
		}

		var value = element.Value;
		if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var fontSize))
		{
			return fontSize;
		}

		if (value.ValueKind != JsonValueKind.String)
		{
			return 15;
		}

		return value.GetString() switch
		{
			"heading_xxl" => 40,
			"heading_xl" => 34,
			"heading_l" => 28,
			"heading_m" => 24,
			"heading_s" => 20,
			"heading_xs" => 16,
			"body_xl" => 18,
			"body_l" => 17,
			"body_m" => 15,
			"body_s" => 13,
			_ => 15
		};
	}

	public static FontAttributes ResolveFontAttributes(string? fontWeight, int? fontWeightInt)
	{
		if (fontWeightInt >= 600)
		{
			return FontAttributes.Bold;
		}

		return fontWeight switch
		{
			"bold" or "semibold" or "heavy" or "black" => FontAttributes.Bold,
			"italic" => FontAttributes.Italic,
			_ => FontAttributes.None
		};
	}

	public static Aspect ResolveAspect(string? fitMode) => fitMode switch
	{
		"fill" => Aspect.AspectFill,
		"fit" => Aspect.AspectFit,
		_ => Aspect.AspectFit
	};

	public static string? ResolveImageUrl(JsonElement? element)
	{
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
		{
			return null;
		}

		return ResolveImageUrl(element.Value);
	}

	public static string? ResolveLocalizedImageUrl(PaywallComponentsData? data, string? locale, string localizationId)
	{
		if (data is null)
		{
			return null;
		}

		var resolvedLocale = PaywallLocalizationResolver.ResolveLocale(data, locale);
		if (resolvedLocale is not null &&
			data.ComponentsLocalizations.TryGetValue(resolvedLocale, out var values) &&
			values.TryGetValue(localizationId, out var localization) &&
			localization.Value is { } value)
		{
			return ResolveImageUrl(value);
		}

		return null;
	}

	static string? ResolveImageUrl(JsonElement element)
	{
		if (element.ValueKind == JsonValueKind.String)
		{
			return element.GetString();
		}

		if (element.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		var preferredTheme = Application.Current?.RequestedTheme == AppTheme.Dark ? "dark" : "light";
		if (element.TryGetProperty(preferredTheme, out var themeImage))
		{
			return ResolveImageUrl(themeImage);
		}

		if (element.TryGetProperty("light", out var lightImage))
		{
			return ResolveImageUrl(lightImage);
		}

		foreach (var propertyName in new[] { "url", "original", "webp", "heic", "jpeg", "png" })
		{
			if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
			{
				return property.GetString();
			}
		}

		return null;
	}

	public static void ApplySize(View view, JsonElement? element)
	{
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			element.Value.ValueKind != JsonValueKind.Object)
		{
			return;
		}

		if (element.Value.TryGetProperty("width", out var width))
		{
			ApplySizeConstraint(view, width, isWidth: true);
		}

		if (element.Value.TryGetProperty("height", out var height))
		{
			ApplySizeConstraint(view, height, isWidth: false);
		}
	}

	static void ApplySizeConstraint(View view, JsonElement constraint, bool isWidth)
	{
		var type = GetType(constraint);
		if (type == "fixed" && constraint.TryGetProperty("value", out var value) && value.TryGetDouble(out var fixedSize))
		{
			if (isWidth)
			{
				view.WidthRequest = fixedSize;
			}
			else
			{
				view.HeightRequest = fixedSize;
			}
		}
		else if (type == "fill")
		{
			if (isWidth)
			{
				view.HorizontalOptions = LayoutOptions.Fill;
			}
			else
			{
				view.VerticalOptions = LayoutOptions.Fill;
			}
		}
	}

	static double GetDouble(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.TryGetDouble(out var number) ? number : 0;

	static Color? ParseRgba(string? rgba)
	{
		if (string.IsNullOrWhiteSpace(rgba))
		{
			return null;
		}

		var value = rgba.TrimStart('#');
		if (value.Length == 6)
		{
			value += "ff";
		}

		if (value.Length != 8 ||
			!byte.TryParse(value[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red) ||
			!byte.TryParse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green) ||
			!byte.TryParse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue) ||
			!byte.TryParse(value.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var alpha))
		{
			return null;
		}

		return Color.FromRgba(red, green, blue, alpha);
	}
}
