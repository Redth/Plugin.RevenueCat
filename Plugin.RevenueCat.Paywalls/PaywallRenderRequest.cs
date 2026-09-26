#nullable enable

using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

public sealed class PaywallRenderRequest
{
	PaywallSelectionState? selection;
	PaywallSemanticSession? semantics;
	internal bool ActionInProgress { get; set; }

	internal PaywallSemanticSession Semantics => semantics ??=
		new(PaywallData, SemanticContext, Diagnostic, GetComponentsConfig());

	internal PaywallSelectionState Selection => selection ??= new(GetComponentsConfig(), Packages, SelectedPackageIdentifier, Semantics);

	internal void UpdateSemanticContext(PaywallSemanticContext? context)
	{
		Semantics.UpdateContext(context);
		if (Selection.Reconcile())
		{
			SelectionReconciled?.Invoke(Selection.SelectedIdentifier);
		}
	}

	internal void SelectPackage(string? identifier)
	{
		if (Selection.Select(identifier))
		{
			PackageSelected?.Invoke(identifier!);
		}
	}

	public PaywallComponentsData? PaywallData { get; init; }

	public PaywallComponentsConfig? ComponentsConfig { get; init; }

	public PaywallUiConfig? UiConfig { get; init; }

	public IReadOnlyList<Package> Packages { get; init; } = [];

	public string? OfferingIdentifier { get; init; }

	public string? Locale { get; init; }

	public string? ApplicationName { get; init; }

	public string? SelectedPackageIdentifier { get; init; }

	public PaywallSemanticContext? SemanticContext { get; init; }

	/// <summary>Reports unsupported or malformed semantic rules without interpreting them as matched.</summary>
	public Action<string>? Diagnostic { get; init; }

	internal Action<string?>? SelectionReconciled { get; init; }

	public object? PlatformContext { get; init; }

	public IPaywallActionHandler? ActionHandler { get; init; }

	public IPaywallVariableProvider? VariableProvider { get; init; }

	public Action<string>? PackageSelected { get; init; }

	public Action<Exception>? ActionFailed { get; init; }

	public PaywallComponentsConfig? GetComponentsConfig() => PaywallData?.ComponentsConfig ?? ComponentsConfig;

	public string? GetDefaultLocale() => PaywallData?.DefaultLocale;
}
