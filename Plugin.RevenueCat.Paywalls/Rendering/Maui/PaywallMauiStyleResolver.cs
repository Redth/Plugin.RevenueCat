#nullable enable

using System.Globalization;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
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
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			element.Value.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		if (element.Value.TryGetProperty("value", out var backgroundValue) && backgroundValue.ValueKind == JsonValueKind.Object)
		{
			return ResolveBackground(backgroundValue, uiConfig);
		}

		var preferredTheme = Application.Current?.RequestedTheme == AppTheme.Dark ? "dark" : "light";
		var colorInfo = element.Value.TryGetProperty(preferredTheme, out var themeColor)
			? themeColor
			: element.Value.TryGetProperty("light", out var lightColor)
				? lightColor
				: element.Value;

		var brush = ResolveBrushInfo(colorInfo, uiConfig);
		if (brush is not null)
		{
			return brush;
		}

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

	static Brush? ResolveBrushInfo(JsonElement value, PaywallUiConfig? uiConfig)
	{
		var type = GetType(value);
		if (type == "hex" || type == "alias")
		{
			var color = ResolveColorInfo(value, uiConfig);
			return color is null ? null : new SolidColorBrush(color);
		}

		if (type == "linear" &&
			value.TryGetProperty("points", out var linearPoints) &&
			linearPoints.ValueKind == JsonValueKind.Array)
		{
			var brush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = ResolveLinearGradientEndPoint(value)
			};

			AddGradientStops(brush.GradientStops, linearPoints, uiConfig);
			return brush.GradientStops.Count == 0 ? null : brush;
		}

		if (type == "radial" &&
			value.TryGetProperty("points", out var radialPoints) &&
			radialPoints.ValueKind == JsonValueKind.Array)
		{
			var brush = new RadialGradientBrush
			{
				Center = new Point(0.5, 0.5),
				Radius = 0.8
			};

			AddGradientStops(brush.GradientStops, radialPoints, uiConfig);
			return brush.GradientStops.Count == 0 ? null : brush;
		}

		return null;
	}

	static Point ResolveLinearGradientEndPoint(JsonElement value)
	{
		if (!value.TryGetProperty("degrees", out var degreesElement) ||
			!degreesElement.TryGetDouble(out var degrees))
		{
			return new Point(1, 1);
		}

		var radians = degrees * Math.PI / 180;
		return new Point(
			0.5 + Math.Cos(radians) / 2,
			0.5 + Math.Sin(radians) / 2);
	}

	static void AddGradientStops(GradientStopCollection stops, JsonElement points, PaywallUiConfig? uiConfig)
	{
		foreach (var point in points.EnumerateArray())
		{
			if (point.ValueKind != JsonValueKind.Object ||
				!point.TryGetProperty("color", out var colorElement))
			{
				continue;
			}

			var offset = point.TryGetProperty("percent", out var percentElement) &&
				percentElement.TryGetDouble(out var percent)
					? Math.Clamp(percent / 100d, 0d, 1d)
					: stops.Count == 0 ? 0d : 1d;
			var color = colorElement.ValueKind == JsonValueKind.String
				? ParseRgba(colorElement.GetString())
				: ResolveColorInfo(colorElement, uiConfig);

			if (color is not null)
			{
				stops.Add(new GradientStop(color, (float)offset));
			}
		}
	}

	public static TextAlignment ResolveTextAlignment(string? alignment) => alignment switch
	{
		"leading" => TextAlignment.Start,
		"trailing" => TextAlignment.End,
		_ => TextAlignment.Center
	};

	public static void ApplyStackLayoutOptions(View view, JsonElement? dimension)
	{
		if (dimension is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			dimension.Value.ValueKind != JsonValueKind.Object)
		{
			return;
		}

		var dimensionType = GetType(dimension.Value);
		var alignment = GetString(dimension.Value, "alignment");
		var distribution = GetString(dimension.Value, "distribution");

		if (dimensionType == "horizontal")
		{
			ApplyHorizontalOption(view, distribution);
			ApplyVerticalOption(view, alignment);
			return;
		}

		ApplyHorizontalOption(view, alignment);
		ApplyVerticalOption(view, distribution);
	}

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

	public static double? ResolveFixedHeight(JsonElement? element) => ResolveFixedSize(element, "height");

	public static double? ResolveFixedWidth(JsonElement? element) => ResolveFixedSize(element, "width");

	public static bool HasContainerDecoration(JsonElement? shape, JsonElement? border, JsonElement? shadow) =>
		shape is { ValueKind: JsonValueKind.Object } ||
		border is { ValueKind: JsonValueKind.Object } ||
		shadow is { ValueKind: JsonValueKind.Object };

	public static IShape ResolveStrokeShape(JsonElement? shape)
	{
		if (shape is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			shape.Value.ValueKind != JsonValueKind.Object)
		{
			return new Rectangle();
		}

		var type = GetType(shape.Value);
		if (type == "pill")
		{
			return new RoundRectangle { CornerRadius = new CornerRadius(999) };
		}

		var cornerRadius = ResolveCornerRadius(shape.Value);
		return cornerRadius == default
			? new Rectangle()
			: new RoundRectangle { CornerRadius = cornerRadius };
	}

	public static CornerRadius ResolveCornerRadius(JsonElement? shape)
	{
		if (shape is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			shape.Value.ValueKind != JsonValueKind.Object)
		{
			return default;
		}

		if (GetType(shape.Value) == "pill")
		{
			return new CornerRadius(999);
		}

		return ResolveCornerRadius(shape.Value);
	}

	public static Brush? ResolveBorderBrush(JsonElement? border, PaywallUiConfig? uiConfig)
	{
		if (border is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			border.Value.ValueKind != JsonValueKind.Object ||
			!border.Value.TryGetProperty("color", out var color))
		{
			return null;
		}

		var borderColor = ResolveColor(color, uiConfig);
		return borderColor is null ? null : new SolidColorBrush(borderColor);
	}

	public static double ResolveBorderWidth(JsonElement? border)
	{
		if (border is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			border.Value.ValueKind != JsonValueKind.Object ||
			!border.Value.TryGetProperty("width", out var width) ||
			!width.TryGetDouble(out var value))
		{
			return 0;
		}

		return value;
	}

	public static Shadow? ResolveShadow(JsonElement? shadow, PaywallUiConfig? uiConfig)
	{
		if (shadow is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			shadow.Value.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		var color = shadow.Value.TryGetProperty("color", out var colorElement)
			? ResolveColor(colorElement, uiConfig)
			: Colors.Black;

		return new Shadow
		{
			Brush = new SolidColorBrush(color ?? Colors.Black),
			Radius = (float)GetDouble(shadow.Value, "radius"),
			Offset = new Point(GetDouble(shadow.Value, "x"), GetDouble(shadow.Value, "y")),
			Opacity = 0.24f
		};
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

	static double? ResolveFixedSize(JsonElement? element, string propertyName)
	{
		if (element is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			element.Value.ValueKind != JsonValueKind.Object ||
			!element.Value.TryGetProperty(propertyName, out var constraint) ||
			GetType(constraint) != "fixed" ||
			!constraint.TryGetProperty("value", out var value) ||
			!value.TryGetDouble(out var fixedSize))
		{
			return null;
		}

		return fixedSize;
	}

	static double GetDouble(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.TryGetDouble(out var number) ? number : 0;

	static string? GetString(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
			? value.GetString()
			: null;

	static void ApplyHorizontalOption(View view, string? value)
	{
		var option = ResolveLayoutOption(value);
		if (option is not null)
		{
			view.HorizontalOptions = option.Value;
		}
	}

	static void ApplyVerticalOption(View view, string? value)
	{
		var option = ResolveLayoutOption(value);
		if (option is not null)
		{
			view.VerticalOptions = option.Value;
		}
	}

	static LayoutOptions? ResolveLayoutOption(string? value) => value switch
	{
		"center" => LayoutOptions.Center,
		"end" or "trailing" or "bottom" => LayoutOptions.End,
		"fill" => LayoutOptions.Fill,
		"start" or "leading" or "top" => LayoutOptions.Start,
		_ => null
	};

	static CornerRadius ResolveCornerRadius(JsonElement shape)
	{
		if (!shape.TryGetProperty("corners", out var corners) || corners.ValueKind != JsonValueKind.Object)
		{
			return default;
		}

		if (corners.TryGetProperty("topLeading", out _) ||
			corners.TryGetProperty("top_leading", out _))
		{
			return new CornerRadius(
				GetCornerRadius(corners, "topLeading", "top_leading"),
				GetCornerRadius(corners, "topTrailing", "top_trailing"),
				GetCornerRadius(corners, "bottomLeading", "bottom_leading"),
				GetCornerRadius(corners, "bottomTrailing", "bottom_trailing"));
		}

		if (corners.TryGetProperty("all", out var all) && all.TryGetDouble(out var allValue))
		{
			return new CornerRadius(allValue);
		}

		return default;
	}

	static double GetCornerRadius(JsonElement corners, string camelCaseName, string snakeCaseName)
	{
		if (corners.TryGetProperty(camelCaseName, out var camelCase) && camelCase.TryGetDouble(out var camelCaseValue))
		{
			return camelCaseValue;
		}

		return corners.TryGetProperty(snakeCaseName, out var snakeCase) && snakeCase.TryGetDouble(out var snakeCaseValue)
			? snakeCaseValue
			: 0;
	}

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
