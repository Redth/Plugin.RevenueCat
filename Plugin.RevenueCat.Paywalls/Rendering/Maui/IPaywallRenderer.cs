#nullable enable

using Microsoft.Maui.Controls;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

public interface IPaywallRenderer
{
	View Render(PaywallRenderRequest request);
}
