#nullable enable

using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

public interface IPaywallActionHandler
{
	Task<CustomerInfo?> PurchaseAsync(PaywallPurchaseRequest request, CancellationToken cancellationToken = default);

	Task<CustomerInfo?> RestoreAsync(CancellationToken cancellationToken = default);

	Task DismissAsync(CancellationToken cancellationToken = default);

	Task NavigateAsync(PaywallNavigationRequest request, CancellationToken cancellationToken = default);
}

public sealed class PaywallPurchaseRequest
{
	public string? OfferingIdentifier { get; init; }

	public string PackageIdentifier { get; init; } = string.Empty;

	public object? PlatformContext { get; init; }
}

public sealed class PaywallNavigationRequest
{
	public string? ActionType { get; init; }

	public string? Destination { get; init; }

	public string? Url { get; init; }
}
