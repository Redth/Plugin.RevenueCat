#nullable enable

namespace Plugin.RevenueCat.Paywalls;

public sealed class PaywallPurchaseRequestedEventArgs : EventArgs
{
	public PaywallPurchaseRequestedEventArgs(PaywallPurchaseRequest request)
	{
		Request = request;
	}

	public PaywallPurchaseRequest Request { get; }
}

public sealed class PaywallRestoreRequestedEventArgs : EventArgs
{
}

public sealed class PaywallDismissRequestedEventArgs : EventArgs
{
}

public sealed class PaywallNavigationRequestedEventArgs : EventArgs
{
	public PaywallNavigationRequestedEventArgs(PaywallNavigationRequest request)
	{
		Request = request;
	}

	public PaywallNavigationRequest Request { get; }
}
