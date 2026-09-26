using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

internal sealed class PaywallSelectionState
{
	readonly IReadOnlyList<Package> packages;
	readonly PaywallComponentsConfig? config;
	readonly PaywallSemanticSession semantics;
	readonly Dictionary<PaywallTabsComponent, string> activeTabs = new();

	public PaywallSelectionState(PaywallComponentsConfig? config, IReadOnlyList<Package> packages, string? selectedIdentifier,
		PaywallSemanticSession? semantics = null)
	{
		this.config = config;
		this.packages = packages;
		this.semantics = semantics ?? new PaywallSemanticSession(null);
		var candidates = Candidates(false).ToArray();
		var hasPackageComponents = Candidates(true).Any();
		SelectedIdentifier = packages.Any(p => p.Identifier == selectedIdentifier) &&
			(!hasPackageComponents || candidates.Any(c => c.PackageId == selectedIdentifier))
				? selectedIdentifier : null;
		if (SelectedIdentifier is null)
		{
			SelectedIdentifier = FindPreferred(candidates) ??
				(hasPackageComponents ? null : packages.FirstOrDefault()?.Identifier);
		}
		this.semantics.SelectedPackageIdentifier = SelectedIdentifier;
	}

	public string? SelectedIdentifier { get; private set; }

	public event Action? Changed;

	public bool Select(string? identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier) ||
			string.Equals(SelectedIdentifier, identifier, StringComparison.Ordinal) ||
			!packages.Any(p => string.Equals(p.Identifier, identifier, StringComparison.Ordinal)) ||
			(Candidates(true).Any() && !Candidates(false).Any(p => p.PackageId == identifier)))
		{
			return false;
		}

		SelectedIdentifier = identifier;
		semantics.SelectedPackageIdentifier = identifier;
		Changed?.Invoke();
		semantics.SelectionChanged();
		return true;
	}

	public string? PreferredPackage(PaywallComponent? component) => FindPreferred(GetPackages(component));

	public bool SetActiveTab(PaywallTabsComponent tabs, string? tabId)
	{
		if (tabId is null || !tabs.Tabs.Any(t => t.Id == tabId))
		{
			return false;
		}
		activeTabs[tabs] = tabId;
		return Reconcile();
	}

	public bool Reconcile()
	{
		var candidates = Candidates(false).ToArray();
		var identifier = FindPreferred(candidates) ??
			(Candidates(true).Any() ? null : packages.FirstOrDefault()?.Identifier);
		if (identifier == SelectedIdentifier)
		{
			return false;
		}
		SelectedIdentifier = identifier;
		semantics.SelectedPackageIdentifier = identifier;
		Changed?.Invoke();
		semantics.SelectionChanged();
		return true;
	}

	IEnumerable<PaywallPackageComponent> Candidates(bool includeHidden) =>
		GetPackages(config?.Base?.Header, includeHidden)
			.Concat(GetPackages(config?.Base?.Stack, includeHidden))
			.Concat(GetPackages(config?.Base?.StickyFooter, includeHidden));

	string? FindPreferred(IEnumerable<PaywallPackageComponent> candidates)
	{
		var available = candidates
			.Where(c => packages.Any(p => string.Equals(p.Identifier, c.PackageId, StringComparison.Ordinal)))
			.ToArray();
		return available.FirstOrDefault(c => c.PackageId == SelectedIdentifier)?.PackageId
			?? available.FirstOrDefault(c => c.IsSelectedByDefault)?.PackageId
			?? available.FirstOrDefault()?.PackageId;
	}

	IEnumerable<PaywallPackageComponent> GetPackages(PaywallComponent? component, bool includeHidden = false)
	{
		return component switch
		{
			PaywallPackageComponent package when includeHidden ||
				(semantics.IsVisible(package, package.PackageId) &&
				 (package.Stack is null || semantics.IsVisible(package.Stack, package.PackageId))) => [package],
			PaywallStackComponent stack when includeHidden || semantics.IsVisible(stack) =>
				stack.Components.SelectMany(child => GetPackages(child, includeHidden)),
			PaywallHeaderComponent header => GetPackages(header.Stack, includeHidden),
			PaywallStickyFooterComponent footer => GetPackages(footer.Stack, includeHidden),
			PaywallButtonComponent button => GetPackages(button.Stack, includeHidden),
			PaywallTabsComponent tabs when includeHidden || semantics.IsVisible(tabs) => GetPackages(
				(tabs.Tabs.FirstOrDefault(t => t.Id == (activeTabs.GetValueOrDefault(tabs) ?? tabs.DefaultTabId))
				 ?? tabs.Tabs.FirstOrDefault())?.Stack, includeHidden),
			PaywallCarouselComponent { Pages.Count: > 0 } carousel when includeHidden || semantics.IsVisible(carousel) => GetPackages(
				carousel.Pages[Math.Clamp(carousel.InitialPageIndex ?? 0, 0, carousel.Pages.Count - 1)], includeHidden),
			PaywallUnknownComponent unknown => GetPackages(unknown.Fallback, includeHidden),
			PaywallVideoComponent video => GetPackages(video.Fallback, includeHidden),
			_ => []
		};
	}
}
