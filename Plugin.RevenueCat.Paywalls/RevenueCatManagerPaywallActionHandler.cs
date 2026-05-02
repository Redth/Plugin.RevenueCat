#nullable enable

using Plugin.RevenueCat.Models;
using Plugin.RevenueCat;

namespace Plugin.RevenueCat.Paywalls;

public sealed class RevenueCatManagerPaywallActionHandler : IPaywallActionHandler
{
	readonly IRevenueCatManager revenueCatManager;

	public RevenueCatManagerPaywallActionHandler(IRevenueCatManager revenueCatManager)
	{
		this.revenueCatManager = revenueCatManager;
	}

	public Task<CustomerInfo?> PurchaseAsync(PaywallPurchaseRequest request, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(request.OfferingIdentifier))
		{
			throw new InvalidOperationException("An offering identifier is required to purchase from a rendered paywall.");
		}

		return revenueCatManager.PurchaseAsync(request.PlatformContext, request.OfferingIdentifier, request.PackageIdentifier);
	}

	public Task<CustomerInfo?> RestoreAsync(CancellationToken cancellationToken = default) =>
		revenueCatManager.RestoreAsync();

	public Task DismissAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

	public Task NavigateAsync(PaywallNavigationRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
