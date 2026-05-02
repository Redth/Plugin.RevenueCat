#nullable enable

using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

public sealed class PaywallRenderRequest
{
	public PaywallComponentsData? PaywallData { get; init; }

	public PaywallComponentsConfig? ComponentsConfig { get; init; }

	public PaywallUiConfig? UiConfig { get; init; }

	public IReadOnlyList<Package> Packages { get; init; } = [];

	public string? OfferingIdentifier { get; init; }

	public string? Locale { get; init; }

	public string? ApplicationName { get; init; }

	public string? SelectedPackageIdentifier { get; init; }

	public object? PlatformContext { get; init; }

	public IPaywallActionHandler? ActionHandler { get; init; }

	public IPaywallVariableProvider? VariableProvider { get; init; }

	public Action<string>? PackageSelected { get; init; }

	public PaywallComponentsConfig? GetComponentsConfig() => PaywallData?.ComponentsConfig ?? ComponentsConfig;

	public string? GetDefaultLocale() => PaywallData?.DefaultLocale;
}
