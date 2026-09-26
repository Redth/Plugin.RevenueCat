#nullable enable

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

internal enum PaywallActionKind
{
	Button,
	Choice,
	Toggle
}

internal static class PaywallAccessibility
{
	static readonly ConditionalWeakTable<PaywallRenderRequest, List<WeakReference<Button>>> Actions = new();
	static readonly ConditionalWeakTable<View, Button> Surfaces = new();
	static readonly ConditionalWeakTable<Button, ActionState> States = new();

	internal static View Wrap(View content) => content is Border ? content : new Border
	{
		Content = content,
		Padding = 0,
		StrokeThickness = 0
	};

	internal static Button AddSurface(View target, PaywallRenderRequest request, string? fallback, PaywallActionKind kind, ICommand command)
	{
		if (target is not Border border)
		{
			throw new ArgumentException("Accessible actions require a border container.", nameof(target));
		}

		var visual = border.Content as View;
		var name = Describe(visual) ?? fallback ?? Localize(request.Locale, "Action");
		var button = new PaywallAccessibleButton
		{
			BackgroundColor = Colors.Transparent,
			BorderWidth = 0,
			CornerRadius = 0,
			Padding = 0,
#if IOS || MACCATALYST
			MinimumWidthRequest = 44,
			MinimumHeightRequest = 44,
#else
			MinimumWidthRequest = 48,
			MinimumHeightRequest = 48,
#endif
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Command = command
		};
		SemanticProperties.SetDescription(button, name);
		SemanticProperties.SetHint(button, kind switch
		{
			PaywallActionKind.Choice => Localize(request.Locale, "Select option"),
			PaywallActionKind.Toggle => Localize(request.Locale, "Change plan"),
			_ => Localize(request.Locale, "Activate")
		});
		AutomationProperties.SetIsInAccessibleTree(button, true);
		var hasNestedActions = visual is not null && Descendants(visual).Any(child => Surfaces.TryGetValue(child, out _));
		if (visual is not null)
		{
			border.Content = null;
			if (!hasNestedActions)
			{
				AutomationProperties.SetExcludedWithChildren(visual, true);
				foreach (var child in Descendants(visual))
				{
					AutomationProperties.SetIsInAccessibleTree(child, false);
					child.HandlerChanged += (_, _) => HideVisualFromAccessibility(child);
					child.Loaded += (_, _) => HideVisualFromAccessibility(child);
					HideVisualFromAccessibility(child);
				}
			}
		}
		var layers = new Grid { ColumnSpacing = 0, RowSpacing = 0 };
		if (visual is not null)
		{
			var padding = border.Padding;
			border.Padding = 0;
			var decoration = new ContentView { Content = visual, Padding = padding };
			if (!hasNestedActions)
			{
				AutomationProperties.SetExcludedWithChildren(decoration, true);
				decoration.HandlerChanged += (_, _) => HideVisualFromAccessibility(decoration);
				decoration.Loaded += (_, _) => HideVisualFromAccessibility(decoration);
			}
			layers.Children.Add(decoration);
		}
		layers.Children.Add(button);
		border.Content = layers;
		Surfaces.Add(target, button);
		States.Add(button, new ActionState(visual, fallback, request.Locale, kind));
		Actions.GetOrCreateValue(request).Add(new WeakReference<Button>(button));
		button.HandlerChanged += (_, _) => UpdateNative(button, kind);
		button.Loaded += (_, _) => UpdateNative(button, kind);
		if (visual is not null)
		{
			foreach (var label in Descendants(visual).OfType<Label>())
			{
				label.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == nameof(Label.Text))
					{
						RefreshName(target, fallback);
					}
				};
			}
		}
		button.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(Button.IsEnabled))
			{
				UpdateNative(button, kind);
			}
		};
		return button;
	}

	internal static void SetSelected(View target, bool selected)
	{
		if (Surfaces.TryGetValue(target, out var button))
		{
			button.SetValue(Selected, selected);
		}
	}

	internal static void SetBusy(PaywallRenderRequest request, bool busy)
	{
		foreach (var reference in Actions.GetOrCreateValue(request).ToArray())
		{
			if (reference.TryGetTarget(out var button))
			{
				button.IsEnabled = !busy && States.GetValue(button, _ => throw new InvalidOperationException()).Enabled;
				button.SetValue(Busy, busy);
				UpdateNative(button, States.GetValue(button, _ => throw new InvalidOperationException()).Kind);
			}
			else
			{
				Actions.GetOrCreateValue(request).Remove(reference);
			}
		}
	}

	internal static void SetEnabled(View target, bool enabled)
	{
		target.IsEnabled = enabled;
		if (Surfaces.TryGetValue(target, out var button))
		{
			States.GetValue(button, _ => throw new InvalidOperationException()).Enabled = enabled;
			button.IsEnabled = enabled && !(bool)button.GetValue(Busy);
		}
	}

	internal static Button? GetSurface(View target) => Surfaces.TryGetValue(target, out var button) ? button : null;

	internal static bool GetSelected(View target) =>
		Surfaces.TryGetValue(target, out var button) && (bool)button.GetValue(Selected);

	internal static bool ReduceMotionRequested()
	{
#if IOS || MACCATALYST
		return UIKit.UIAccessibility.IsReduceMotionEnabled;
#elif ANDROID
		return OperatingSystem.IsAndroidVersionAtLeast(26) &&
			!Android.Animation.ValueAnimator.AreAnimatorsEnabled();
#else
		return false;
#endif
	}

	static void HideVisualFromAccessibility(View visual)
	{
#if ANDROID
		if (visual.Handler?.PlatformView is Android.Views.View native)
		{
			native.ImportantForAccessibility = Android.Views.ImportantForAccessibility.NoHideDescendants;
		}
#elif IOS || MACCATALYST
		if (visual.Handler?.PlatformView is UIKit.UIView native)
		{
			native.IsAccessibilityElement = false;
			native.AccessibilityElementsHidden = true;
		}
#endif
	}

	internal static void RefreshName(View target, string? fallback = null)
	{
		if (Surfaces.TryGetValue(target, out var button))
		{
			var state = States.GetValue(button, _ => throw new InvalidOperationException());
			SemanticProperties.SetDescription(button, Describe(state.Visual) ?? fallback ?? state.Fallback ?? Localize(state.Locale, "Action"));
			UpdateNative(button, state.Kind);
		}
	}

	static string? Describe(View? view)
	{
		if (view is null)
		{
			return null;
		}
		var labels = Descendants(view).OfType<Label>()
			.Select(label => label.Text?.Trim())
			.Where(text => !string.IsNullOrWhiteSpace(text))
			.Distinct(StringComparer.Ordinal)
			.Take(3)
			.ToArray();
		return labels.Length == 0 ? null : string.Join(", ", labels);
	}

	static IEnumerable<View> Descendants(View view)
	{
		yield return view;
		IEnumerable<View> children = view switch
		{
			Layout layout => layout.Children.OfType<View>(),
			Border { Content: View child } => [child],
			ContentView { Content: View child } => [child],
			ScrollView { Content: View child } => [child],
			_ => []
		};
		foreach (var child in children.SelectMany(Descendants))
		{
			yield return child;
		}
	}

	internal static string Localize(string? locale, string key)
	{
		var language = locale?.Split('-', '_')[0] ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		return (language, key) switch
		{
			("es", "Action") => "Acción",
			("es", "Select option") => "Seleccionar opción",
			("es", "Change plan") => "Cambiar plan",
			("es", "Activate") => "Activar",
			("es", "On") => "Activado",
			("es", "Off") => "Desactivado",
			("es", "Purchase") => "Comprar",
			("es", "Restore purchases") => "Restaurar compras",
			("es", "Close") => "Cerrar",
			("es", "Busy") => "En curso",
			("fr", "Action") => "Action",
			("fr", "Select option") => "Sélectionner une option",
			("fr", "Change plan") => "Changer de formule",
			("fr", "Activate") => "Activer",
			("fr", "On") => "Activé",
			("fr", "Off") => "Désactivé",
			("fr", "Purchase") => "Acheter",
			("fr", "Restore purchases") => "Restaurer les achats",
			("fr", "Close") => "Fermer",
			("fr", "Busy") => "En cours",
			_ => key
		};
	}

	static readonly BindableProperty Selected = BindableProperty.CreateAttached("Selected", typeof(bool), typeof(PaywallAccessibility), false,
		propertyChanged: (bindable, _, _) =>
		{
			if (bindable is Button button)
			{
				UpdateNative(button, States.GetValue(button, _ => throw new InvalidOperationException()).Kind);
			}
		});
	static readonly BindableProperty Busy = BindableProperty.CreateAttached("Busy", typeof(bool), typeof(PaywallAccessibility), false);
	sealed class ActionState(View? visual, string? fallback, string? locale, PaywallActionKind kind)
	{
		public View? Visual { get; } = visual;
		public string? Fallback { get; } = fallback;
		public string? Locale { get; } = locale;
		public PaywallActionKind Kind { get; } = kind;
		public bool Enabled { get; set; } = true;
	}

	static void UpdateNative(Button button, PaywallActionKind kind)
	{
#if ANDROID
		if (button.Handler?.PlatformView is Android.Widget.Button native)
		{
			native.Focusable = button.IsEnabled;
			native.ContentDescription = SemanticProperties.GetDescription(button);
			native.Selected = kind != PaywallActionKind.Button && (bool)button.GetValue(Selected);
			if (native is NativePaywallAccessibleButton accessible)
			{
				accessible.Kind = kind;
				accessible.IsChoiceSelected = (bool)button.GetValue(Selected);
				accessible.IsBusy = (bool)button.GetValue(Busy);
				accessible.Locale = States.GetValue(button, _ => throw new InvalidOperationException()).Locale;
			}
		}
#elif IOS || MACCATALYST
		if (button.Handler?.PlatformView is UIKit.UIButton native)
		{
			native.AccessibilityTraits = UIKit.UIAccessibilityTrait.Button |
				((bool)button.GetValue(Selected) ? UIKit.UIAccessibilityTrait.Selected : default) |
				(!button.IsEnabled ? UIKit.UIAccessibilityTrait.NotEnabled : default);
			native.AccessibilityValue = (bool)button.GetValue(Busy)
				? Localize(States.GetValue(button, _ => throw new InvalidOperationException()).Locale, "Busy")
				: kind == PaywallActionKind.Toggle
					? Localize(States.GetValue(button, _ => throw new InvalidOperationException()).Locale,
						(bool)button.GetValue(Selected) ? "On" : "Off")
					: null;
		}
#endif
	}

}
