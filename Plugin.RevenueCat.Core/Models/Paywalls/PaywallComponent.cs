#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.RevenueCat.Models;

public abstract partial class PaywallComponent
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("fallback")]
	public PaywallComponent? Fallback { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed partial class PaywallUnknownComponent : PaywallComponent
{
	[JsonIgnore]
	public JsonElement Raw { get; set; }
}

public sealed partial class PaywallStackComponent : PaywallComponent
{
	[JsonPropertyName("components")]
	public List<PaywallComponent> Components { get; set; } = new();

	[JsonPropertyName("visible")]
	public bool? Visible { get; set; }

	[JsonPropertyName("dimension")]
	public JsonElement? Dimension { get; set; }

	[JsonPropertyName("size")]
	public JsonElement? Size { get; set; }

	[JsonPropertyName("spacing")]
	public double? Spacing { get; set; }

	[JsonPropertyName("background_color")]
	public JsonElement? BackgroundColor { get; set; }

	[JsonPropertyName("background")]
	public JsonElement? Background { get; set; }

	[JsonPropertyName("padding")]
	public JsonElement? Padding { get; set; }

	[JsonPropertyName("margin")]
	public JsonElement? Margin { get; set; }

	[JsonPropertyName("shape")]
	public JsonElement? Shape { get; set; }

	[JsonPropertyName("border")]
	public JsonElement? Border { get; set; }

	[JsonPropertyName("shadow")]
	public JsonElement? Shadow { get; set; }

	[JsonPropertyName("badge")]
	public JsonElement? Badge { get; set; }

	[JsonPropertyName("overflow")]
	public string? Overflow { get; set; }

	[JsonPropertyName("overrides")]
	public List<PaywallComponentOverride> Overrides { get; set; } = new();
}

public sealed partial class PaywallTextComponent : PaywallComponent
{
	[JsonPropertyName("text_lid")]
	public string? TextLocalizationId { get; set; }

	[JsonPropertyName("color")]
	public JsonElement? Color { get; set; }

	[JsonPropertyName("visible")]
	public bool? Visible { get; set; }

	[JsonPropertyName("background_color")]
	public JsonElement? BackgroundColor { get; set; }

	[JsonPropertyName("font_name")]
	public string? FontName { get; set; }

	[JsonPropertyName("font_weight")]
	public string? FontWeight { get; set; }

	[JsonPropertyName("font_weight_int")]
	public int? FontWeightInt { get; set; }

	[JsonPropertyName("font_size")]
	public JsonElement? FontSize { get; set; }

	[JsonPropertyName("horizontal_alignment")]
	public string? HorizontalAlignment { get; set; }

	[JsonPropertyName("size")]
	public JsonElement? Size { get; set; }

	[JsonPropertyName("padding")]
	public JsonElement? Padding { get; set; }

	[JsonPropertyName("margin")]
	public JsonElement? Margin { get; set; }

	[JsonPropertyName("overrides")]
	public List<PaywallComponentOverride> Overrides { get; set; } = new();
}

public sealed partial class PaywallImageComponent : PaywallComponent
{
	[JsonPropertyName("source")]
	public JsonElement? Source { get; set; }

	[JsonPropertyName("override_source_lid")]
	public string? OverrideSourceLocalizationId { get; set; }

	[JsonPropertyName("visible")]
	public bool? Visible { get; set; }

	[JsonPropertyName("size")]
	public JsonElement? Size { get; set; }

	[JsonPropertyName("fit_mode")]
	public string? FitMode { get; set; }

	[JsonPropertyName("mask_shape")]
	public JsonElement? MaskShape { get; set; }

	[JsonPropertyName("color_overlay")]
	public JsonElement? ColorOverlay { get; set; }

	[JsonPropertyName("padding")]
	public JsonElement? Padding { get; set; }

	[JsonPropertyName("margin")]
	public JsonElement? Margin { get; set; }

	[JsonPropertyName("border")]
	public JsonElement? Border { get; set; }

	[JsonPropertyName("shadow")]
	public JsonElement? Shadow { get; set; }
}

public sealed partial class PaywallIconComponent : PaywallComponent
{
	[JsonPropertyName("base_url")]
	public string? BaseUrl { get; set; }

	[JsonPropertyName("icon_name")]
	public string? IconName { get; set; }

	[JsonPropertyName("formats")]
	public Dictionary<string, string> Formats { get; set; } = new();

	[JsonPropertyName("visible")]
	public bool? Visible { get; set; }

	[JsonPropertyName("size")]
	public JsonElement? Size { get; set; }

	[JsonPropertyName("color")]
	public JsonElement? Color { get; set; }

	[JsonPropertyName("icon_background")]
	public JsonElement? IconBackground { get; set; }

	[JsonPropertyName("padding")]
	public JsonElement? Padding { get; set; }

	[JsonPropertyName("margin")]
	public JsonElement? Margin { get; set; }
}

public sealed partial class PaywallButtonComponent : PaywallComponent
{
	[JsonPropertyName("action")]
	public PaywallButtonAction? Action { get; set; }

	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }

	[JsonPropertyName("transition")]
	public JsonElement? Transition { get; set; }
}

public sealed partial class PaywallPackageComponent : PaywallComponent
{
	[JsonPropertyName("package_id")]
	public string? PackageId { get; set; }

	[JsonPropertyName("is_selected_by_default")]
	public bool IsSelectedByDefault { get; set; }

	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }

	[JsonPropertyName("visible")]
	public bool? Visible { get; set; }

	[JsonPropertyName("play_store_offer")]
	public JsonElement? PlayStoreOffer { get; set; }
}

public sealed partial class PaywallPurchaseButtonComponent : PaywallComponent
{
	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }

	[JsonPropertyName("action")]
	public string? Action { get; set; }

	[JsonPropertyName("method")]
	public JsonElement? Method { get; set; }
}

public sealed partial class PaywallHeaderComponent : PaywallComponent
{
	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }
}

public sealed partial class PaywallStickyFooterComponent : PaywallComponent
{
	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }
}

public sealed partial class PaywallCarouselComponent : PaywallComponent
{
	[JsonPropertyName("pages")]
	public List<PaywallStackComponent> Pages { get; set; } = new();
}

public sealed partial class PaywallTabsComponent : PaywallComponent
{
	[JsonPropertyName("tabs")]
	public List<PaywallTabComponent> Tabs { get; set; } = new();

	[JsonPropertyName("control")]
	public JsonElement? Control { get; set; }

	[JsonPropertyName("default_tab_id")]
	public string? DefaultTabId { get; set; }
}

public sealed partial class PaywallTabComponent : PaywallComponent
{
	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }
}

public sealed partial class PaywallTabControlComponent : PaywallComponent
{
}

public sealed partial class PaywallTabControlButtonComponent : PaywallComponent
{
	[JsonPropertyName("tab_id")]
	public string? TabId { get; set; }

	[JsonPropertyName("stack")]
	public PaywallStackComponent? Stack { get; set; }
}

public sealed partial class PaywallTabControlToggleComponent : PaywallComponent
{
	[JsonPropertyName("default_value")]
	public bool? DefaultValue { get; set; }
}

public sealed partial class PaywallTimelineComponent : PaywallComponent
{
	[JsonPropertyName("items")]
	public List<JsonElement> Items { get; set; } = new();
}

public sealed partial class PaywallVideoComponent : PaywallComponent
{
	[JsonPropertyName("source")]
	public JsonElement? Source { get; set; }
}

public sealed partial class PaywallCountdownComponent : PaywallComponent
{
}

public sealed partial class PaywallFallbackHeaderComponent : PaywallComponent
{
}

public partial class PaywallButtonAction
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("destination")]
	public string? Destination { get; set; }

	[JsonPropertyName("url")]
	public JsonElement? Url { get; set; }

	[JsonPropertyName("sheet")]
	public JsonElement? Sheet { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public partial class PaywallComponentOverride
{
	[JsonPropertyName("conditions")]
	public List<PaywallComponentOverrideCondition> Conditions { get; set; } = new();

	[JsonPropertyName("properties")]
	public JsonElement? Properties { get; set; }
}

public partial class PaywallComponentOverrideCondition
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("operator")]
	public string? Operator { get; set; }

	[JsonPropertyName("value")]
	public JsonElement? Value { get; set; }

	[JsonPropertyName("variable")]
	public string? Variable { get; set; }

	[JsonPropertyName("packages")]
	public List<string> Packages { get; set; } = new();

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
