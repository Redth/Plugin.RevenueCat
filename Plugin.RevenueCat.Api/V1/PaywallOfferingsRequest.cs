namespace Plugin.RevenueCat.Api.V1;

/// <summary>
/// Optional SDK-like metadata sent when retrieving runtime offerings for paywall rendering.
/// </summary>
public sealed class PaywallOfferingsRequest
{
	/// <summary>
	/// Platform identifier such as <c>android</c>, <c>ios</c>, or <c>macos</c>.
	/// </summary>
	public string? Platform { get; set; }

	/// <summary>
	/// OS or platform version.
	/// </summary>
	public string? PlatformVersion { get; set; }

	/// <summary>
	/// Client SDK version to report to RevenueCat.
	/// </summary>
	public string? ClientVersion { get; set; }

	/// <summary>
	/// App version to report to RevenueCat.
	/// </summary>
	public string? AppVersion { get; set; }

	/// <summary>
	/// Storefront/country code when available.
	/// </summary>
	public string? Storefront { get; set; }

	/// <summary>
	/// Preferred locale identifiers in priority order, such as <c>en-US</c> or <c>fr-CA</c>.
	/// </summary>
	public IReadOnlyList<string>? PreferredLocales { get; set; }
}
