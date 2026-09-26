#nullable enable

using Microsoft.Maui.Controls;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

public interface IPaywallRenderer
{
	/// <summary>Whether package selection updates existing views without another call to Render.</summary>
	bool HandlesPackageSelectionUpdates => false;

	View Render(PaywallRenderRequest request);
}
