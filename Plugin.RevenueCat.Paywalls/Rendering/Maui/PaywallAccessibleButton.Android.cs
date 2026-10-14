#if ANDROID
using Android.Content;
using Android.Views.Accessibility;
using Google.Android.Material.Button;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

internal sealed class PaywallAccessibleButtonHandler : ButtonHandler
{
	protected override MaterialButton CreatePlatformView() => new NativePaywallAccessibleButton(Context);
}

internal sealed class NativePaywallAccessibleButton(Context context) : MauiMaterialButton(context)
{
	internal PaywallActionKind Kind { get; set; }
	internal bool IsChoiceSelected { get; set; }
	internal bool IsBusy { get; set; }
	internal string? Locale { get; set; }

	public override void OnInitializeAccessibilityNodeInfo(AccessibilityNodeInfo? info)
	{
		base.OnInitializeAccessibilityNodeInfo(info);
		if (info is null)
		{
			return;
		}

		if (Kind != PaywallActionKind.Button)
		{
			info.ClassName = Kind == PaywallActionKind.Toggle
				? "android.widget.Switch" : "android.widget.RadioButton";
			info.Checkable = true;
			if (OperatingSystem.IsAndroidVersionAtLeast(36))
			{
				info.CheckedState = IsChoiceSelected ? CheckedState.True : CheckedState.False;
			}
			else
			{
				info.Checked = IsChoiceSelected;
			}
		}

		if (IsBusy)
		{
			info.ContentDescription = $"{ContentDescription}, {PaywallAccessibility.Localize(Locale, "Busy")}";
		}
	}
}
#endif
