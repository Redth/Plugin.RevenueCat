using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

internal sealed class PaywallSelectionState
{
	readonly IReadOnlyList<Package> packages;

	public PaywallSelectionState(PaywallComponentsConfig? config, IReadOnlyList<Package> packages, string? selectedIdentifier)
	{
		this.packages = packages;
		IEnumerable<PaywallPackageComponent> Candidates(bool includeHidden) =>
			GetPackages(config?.Base?.Header, includeHidden)
				.Concat(GetPackages(config?.Base?.Stack, includeHidden))
				.Concat(GetPackages(config?.Base?.StickyFooter, includeHidden));

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
	}

	public string? SelectedIdentifier { get; private set; }

	public event Action? Changed;

	public bool Select(string? identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier) ||
			string.Equals(SelectedIdentifier, identifier, StringComparison.Ordinal) ||
			!packages.Any(p => string.Equals(p.Identifier, identifier, StringComparison.Ordinal)))
		{
			return false;
		}

		SelectedIdentifier = identifier;
		Changed?.Invoke();
		return true;
	}

	public string? PreferredPackage(PaywallComponent? component) => FindPreferred(GetPackages(component));

	string? FindPreferred(IEnumerable<PaywallPackageComponent> candidates)
	{
		var available = candidates
			.Where(c => packages.Any(p => string.Equals(p.Identifier, c.PackageId, StringComparison.Ordinal)))
			.ToArray();
		return available.FirstOrDefault(c => c.PackageId == SelectedIdentifier)?.PackageId
			?? available.FirstOrDefault(c => c.IsSelectedByDefault)?.PackageId
			?? available.FirstOrDefault()?.PackageId;
	}

	static IEnumerable<PaywallPackageComponent> GetPackages(PaywallComponent? component, bool includeHidden = false)
	{
		return component switch
		{
			PaywallPackageComponent package when includeHidden || (package.Visible != false && package.Stack?.Visible != false) => [package],
			PaywallStackComponent stack when includeHidden || stack.Visible != false =>
				stack.Components.SelectMany(child => GetPackages(child, includeHidden)),
			PaywallHeaderComponent header => GetPackages(header.Stack, includeHidden),
			PaywallStickyFooterComponent footer => GetPackages(footer.Stack, includeHidden),
			PaywallButtonComponent button => GetPackages(button.Stack, includeHidden),
			PaywallTabsComponent tabs when includeHidden || tabs.Visible != false => GetPackages(
				(tabs.Tabs.FirstOrDefault(t => t.Id == tabs.DefaultTabId) ?? tabs.Tabs.FirstOrDefault())?.Stack, includeHidden),
			PaywallCarouselComponent { Pages.Count: > 0 } carousel when includeHidden || carousel.Visible != false => GetPackages(
				carousel.Pages[Math.Clamp(carousel.InitialPageIndex ?? 0, 0, carousel.Pages.Count - 1)], includeHidden),
			PaywallUnknownComponent unknown => GetPackages(unknown.Fallback, includeHidden),
			PaywallVideoComponent video => GetPackages(video.Fallback, includeHidden),
			_ => []
		};
	}
}
