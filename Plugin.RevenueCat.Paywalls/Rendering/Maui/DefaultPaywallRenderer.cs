#nullable enable

using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

public sealed class DefaultPaywallRenderer : IPaywallRenderer
{
	const double DefaultPackageSelectionStrokeThickness = 2;
	const uint TabTransitionDuration = 140;

	public bool HandlesPackageSelectionUpdates => true;

	public View Render(PaywallRenderRequest request)
	{
		var config = request.GetComponentsConfig();
		if (config?.Base?.Stack is null)
		{
			return new Label
			{
				Text = "Paywall data is unavailable.",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}

		var root = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		if (config.Base.Background is { } background)
		{
			root.Background = PaywallMauiStyleResolver.ResolveBackground(background, request.UiConfig);
		}

		if (config.Base.Header is not null)
		{
			var header = RenderComponent(config.Base.Header, request);
			Grid.SetRow(header, 0);
			root.Children.Add(header);
		}

		var body = new ScrollView
		{
			Content = RenderStack(config.Base.Stack, request)
		};
		Grid.SetRow(body, 1);
		root.Children.Add(body);

		if (config.Base.StickyFooter is not null)
		{
			var footer = RenderComponent(config.Base.StickyFooter, request);
			Grid.SetRow(footer, 2);
			root.Children.Add(footer);
		}

		return root;
	}

	View RenderComponent(
		PaywallComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier = null,
		Action<string>? tabSelected = null,
		string? selectedTabId = null,
		IReadOnlyDictionary<string, string>? variables = null,
		IReadOnlyList<string>? tabIds = null)
	{
		if (component is PaywallIconComponent { Visible: false })
		{
			return new ContentView { IsVisible = false };
		}

		if (component is PaywallUnknownComponent { Fallback: { } fallback })
		{
			return RenderComponent(fallback, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);
		}

		var view = component switch
		{
			PaywallStackComponent stack => RenderStack(stack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallTextComponent text => RenderText(text, request, packageContextIdentifier, variables),
			PaywallImageComponent image => RenderImage(image, request),
			PaywallIconComponent icon => RenderIcon(icon),
			PaywallButtonComponent button => RenderButton(button, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallPackageComponent package => RenderPackage(package, request, tabSelected, selectedTabId, variables, tabIds),
			PaywallPurchaseButtonComponent purchaseButton => RenderPurchaseButton(purchaseButton, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallHeaderComponent header when header.Stack is not null => RenderStack(header.Stack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallStickyFooterComponent footer when footer.Stack is not null => RenderStack(footer.Stack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallCarouselComponent carousel => RenderCarousel(carousel, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallTabsComponent tabs => RenderTabs(tabs, request, packageContextIdentifier, variables),
			PaywallTabControlButtonComponent tabButton => RenderTabControlButton(tabButton, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallTabControlToggleComponent toggle => RenderTabControlToggle(toggle, request, tabSelected, selectedTabId, tabIds),
			PaywallTimelineComponent timeline => RenderTimeline(timeline, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallCountdownComponent countdown => RenderCountdown(countdown, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			PaywallVideoComponent video => RenderVideo(video, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			{ Fallback: { } componentFallback } => RenderComponent(componentFallback, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
			_ => RenderUnsupportedComponent(component, request)
		};
		if (!string.IsNullOrWhiteSpace(component.Id))
		{
			view.AutomationId = component.Id;
		}
		return view;
	}

	static View RenderUnsupportedComponent(PaywallComponent component, PaywallRenderRequest request)
	{
		var message = $"Unsupported paywall component '{component.Type ?? component.GetType().Name}'" +
			$" ({component.Id ?? "no id"}); no fallback is available.";
		System.Diagnostics.Trace.TraceWarning("{0}", message);
		request.Diagnostic?.Invoke(message);
		return new ContentView { IsVisible = false };
	}

	View RenderStack(
		PaywallStackComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier = null,
		Action<string>? tabSelected = null,
		string? selectedTabId = null,
		IReadOnlyDictionary<string, string>? variables = null,
		IReadOnlyList<string>? tabIds = null)
	{
		var layout = CreateStackLayout(component);
		PaywallMauiStyleResolver.ApplyStackLayoutOptions(layout, component.Dimension);
		if (ShouldUseConstrainedHorizontalLayout(component))
		{
			layout.HorizontalOptions = LayoutOptions.Fill;
		}

		for (var i = 0; i < component.Components.Count; i++)
		{
			var child = component.Components[i];
			AddStackChild(
				layout,
				RenderComponent(child, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds),
				component,
				i);
		}

		var initialOverrides = request.Semantics.Resolve(component, packageContextIdentifier);
		var container = WrapContainer(
			layout,
			initialOverrides.Element("padding") ?? component.Padding,
			initialOverrides.Element("margin") ?? component.Margin,
			initialOverrides.Element("background_color") ??
				initialOverrides.Element("background") ??
				component.BackgroundColor ?? component.Background,
			component.Shape,
			component.Border,
			component.Shadow,
			request.UiConfig);
		var view = ApplyBadge(container, component.Badge, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);
		var applySize = CaptureSizeUpdater(view);
		ObserveSemantics(view, request, () =>
		{
			var overrides = request.Semantics.Resolve(component, packageContextIdentifier);
			view.IsVisible = request.Semantics.IsVisible(component, packageContextIdentifier);
			var background = overrides.Element("background_color") ?? overrides.Element("background") ??
				component.BackgroundColor ?? component.Background;
			var margin = overrides.Element("margin") ?? component.Margin;
			ApplySemanticContainerStyles(container,
				overrides.Element("padding") ?? component.Padding,
				ReferenceEquals(container, view) ? margin : null,
				background, request.UiConfig);
			if (!ReferenceEquals(container, view))
			{
				view.Margin = PaywallMauiStyleResolver.ResolveThickness(margin);
			}
			applySize(overrides.Element("size") ?? component.Size);
		});

		return view;
	}

	Layout CreateStackLayout(PaywallStackComponent component)
	{
		var dimensionType = PaywallMauiStyleResolver.GetType(component.Dimension);
		var spacing = component.Spacing ?? 0;

		if (dimensionType == "horizontal")
		{
			return ShouldUseConstrainedHorizontalLayout(component)
				? new Grid
				{
					ColumnSpacing = spacing
				}
				: new HorizontalStackLayout
				{
					Spacing = spacing
				};
		}

		if (dimensionType == "zlayer")
		{
			return new Grid();
		}

		return new VerticalStackLayout
		{
			Spacing = spacing
		};
	}

	void AddStackChild(Layout layout, View childView, PaywallStackComponent component, int index)
	{
		if (layout is Grid grid && PaywallMauiStyleResolver.GetType(component.Dimension) == "horizontal")
		{
			grid.ColumnDefinitions.Add(new ColumnDefinition(
				component.Components[index] is PaywallTextComponent ? GridLength.Star : GridLength.Auto));
			Grid.SetColumn(childView, index);
			childView.VerticalOptions = LayoutOptions.Center;

			if (childView is Label label)
			{
				label.HorizontalOptions = LayoutOptions.Fill;
				label.VerticalTextAlignment = TextAlignment.Center;
			}

			grid.Children.Add(childView);
			return;
		}

		layout.Children.Add(childView);
	}

	static bool ShouldUseConstrainedHorizontalLayout(PaywallStackComponent component)
	{
		if (PaywallMauiStyleResolver.GetType(component.Dimension) != "horizontal")
		{
			return false;
		}

		return component.Components.Any(static child => child is PaywallTextComponent);
	}

	View RenderText(
		PaywallTextComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier = null,
		IReadOnlyDictionary<string, string>? variables = null)
	{
		var baseVariableProvider = request.VariableProvider ?? new DefaultPaywallVariableProvider();
		var variableProvider = variables is null
			? baseVariableProvider
			: new OverlayPaywallVariableProvider(baseVariableProvider, variables);
		var label = new Label
		{
			TextColor = PaywallMauiStyleResolver.ResolveColor(component.Color, request.UiConfig) ?? Colors.Black,
			HorizontalTextAlignment = PaywallMauiStyleResolver.ResolveTextAlignment(component.HorizontalAlignment),
			FontSize = PaywallMauiStyleResolver.ResolveFontSize(component.FontSize),
			FontAttributes = PaywallMauiStyleResolver.ResolveFontAttributes(component.FontWeight, component.FontWeightInt),
			LineBreakMode = LineBreakMode.WordWrap
		};
		var applySize = CaptureSizeUpdater(label);

		void UpdateText()
		{
			var package = FindPackage(request, packageContextIdentifier ?? request.Selection.SelectedIdentifier, allowDefaultPackage: false);
			var overrides = request.Semantics.Resolve(component, packageContextIdentifier ?? request.Selection.SelectedIdentifier);
			var textId = overrides.String("text_lid") ?? component.TextLocalizationId;
			var text = PaywallLocalizationResolver.ResolveText(request.PaywallData, request.Locale, textId)
				?? textId ?? string.Empty;
			label.IsVisible = request.Semantics.IsVisible(component, packageContextIdentifier ?? request.Selection.SelectedIdentifier);
			label.TextColor = PaywallMauiStyleResolver.ResolveColor(overrides.Element("color") ?? component.Color, request.UiConfig)
				?? Colors.Black;
			label.FontSize = PaywallMauiStyleResolver.ResolveFontSize(overrides.Element("font_size") ?? component.FontSize);
			label.FontAttributes = PaywallMauiStyleResolver.ResolveFontAttributes(
				overrides.String("font_weight") ?? component.FontWeight,
				overrides.Number("font_weight_int") is { } weight ? (int)weight : component.FontWeightInt);
			label.HorizontalTextAlignment = PaywallMauiStyleResolver.ResolveTextAlignment(
				overrides.String("horizontal_alignment") ?? component.HorizontalAlignment);
			ApplyBoxStyles(label, overrides.Element("padding") ?? component.Padding,
				overrides.Element("margin") ?? component.Margin,
				overrides.Element("background_color") ?? component.BackgroundColor, request.UiConfig);
			applySize(overrides.Element("size") ?? component.Size);
			var eligibility = request.Semantics.IntroEligibility(package);
			label.Text = PaywallTextProcessor.ProcessVariables(text, variableProvider, new PaywallVariableContext
			{
				Package = package,
				ApplicationName = request.ApplicationName,
				Locale = request.Locale,
				IntroOfferEligibility = eligibility,
				CustomVariables = request.Semantics.Context.CustomVariables
			}, request.Diagnostic);
		}
		ObserveSemantics(label, request, UpdateText);

		return label;
	}

	View RenderImage(PaywallImageComponent component, PaywallRenderRequest request)
	{
		if (component.Visible == false)
		{
			return new ContentView { IsVisible = false };
		}

		var source = PaywallMauiStyleResolver.ResolveImageUrl(component.Source);
		if (component.OverrideSourceLocalizationId is not null)
		{
			source = PaywallMauiStyleResolver.ResolveLocalizedImageUrl(
				request.PaywallData,
				request.Locale,
				component.OverrideSourceLocalizationId) ?? source;
		}

		var image = CreateImageView(
			source,
			PaywallMauiStyleResolver.ResolveAspect(component.FitMode),
			component.MaskShape);

		var view = WrapContainer(
			image,
			component.Padding,
			component.Margin,
			null,
			component.MaskShape,
			component.Border,
			component.Shadow,
			request.UiConfig);
		PaywallMauiStyleResolver.ApplySize(view, component.Size);

		return view;
	}

	View RenderIcon(PaywallIconComponent component)
	{
		var asset = GetIconAsset(component);
		var image = CreateImageView(asset, Aspect.AspectFit);

		PaywallMauiStyleResolver.ApplySize(image, component.Size);
		return image;
	}

	View RenderButton(
		PaywallButtonComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier = null,
		Action<string>? tabSelected = null,
		string? selectedTabId = null,
		IReadOnlyDictionary<string, string>? variables = null,
		IReadOnlyList<string>? tabIds = null)
	{
		var content = component.Stack is null
			? new Label { Text = component.Action?.Type ?? "Button" }
			: RenderStack(component.Stack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);

		var button = WrapTappable(content, async () =>
		{
			if (component.Action?.Type == "restore_purchases" && request.ActionHandler is not null)
			{
				await request.ActionHandler.RestoreAsync();
			}
			else if (component.Action?.Type == "navigate_back" && request.ActionHandler is not null)
			{
				await request.ActionHandler.DismissAsync();
			}
			else if (request.ActionHandler is not null)
			{
				await request.ActionHandler.NavigateAsync(new PaywallNavigationRequest
				{
					ActionType = component.Action?.Type,
					Destination = component.Action?.Destination,
					Url = TryGetUrl(component.Action?.Url, request)
				});
			}
		}, request);

		return button;
	}

	View RenderPackage(
		PaywallPackageComponent component,
		PaywallRenderRequest request,
		Action<string>? tabSelected = null,
		string? selectedTabId = null,
		IReadOnlyDictionary<string, string>? variables = null,
		IReadOnlyList<string>? tabIds = null)
	{
		var content = component.Stack is null
			? new Label { Text = component.PackageId }
			: RenderStack(component.Stack, request, component.PackageId, tabSelected, selectedTabId, variables, tabIds);
		var packageView = MakePackageTappable(content, () =>
		{
			if (!string.IsNullOrWhiteSpace(component.PackageId))
			{
				request.SelectPackage(component.PackageId);
			}

			return Task.CompletedTask;
		}, request);
		ReservePackageSelectionStroke(packageView);

		var border = FindFirstBorder(packageView);
		var unselectedStroke = border?.Stroke;
		ObserveSelection(packageView, request, () =>
		{
			if (string.Equals(component.PackageId, request.Selection.SelectedIdentifier, StringComparison.Ordinal))
			{
				ApplySelectedPackageStyle(packageView);
			}
			else if (border is not null)
			{
				border.Stroke = unselectedStroke;
			}
		});
		ObserveSemantics(packageView, request, () =>
		{
			packageView.IsVisible = request.Semantics.IsVisible(component, component.PackageId) &&
				(component.Stack is null || request.Semantics.IsVisible(component.Stack, component.PackageId));
			packageView.IsEnabled = packageView.IsVisible &&
				FindPackage(request, component.PackageId, allowDefaultPackage: false) is not null;
		});

		return packageView;
	}

	View RenderPurchaseButton(
		PaywallPurchaseButtonComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier = null,
		Action<string>? tabSelected = null,
		string? selectedTabId = null,
		IReadOnlyDictionary<string, string>? variables = null,
		IReadOnlyList<string>? tabIds = null)
	{
		var content = component.Stack is null
			? new Label { Text = "Purchase" }
			: RenderStack(component.Stack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);

		return WrapTappable(content, async () =>
		{
			var method = component.Method is { } methodDefinition
				? PaywallMauiStyleResolver.GetType(methodDefinition)
				: component.Action ?? "in_app_checkout";
			if (method != "in_app_checkout")
			{
				throw new NotSupportedException($"Paywall purchase method '{method}' is not supported. It will not be replaced with an in-app purchase.");
			}
			var packageIdentifier = packageContextIdentifier ?? request.Selection.SelectedIdentifier;
			var package = FindPackage(request, packageIdentifier, allowDefaultPackage: false);
			if (package is null)
			{
				throw new InvalidOperationException("The selected paywall package is not available in the store catalog.");
			}
			if (request.ActionHandler is not null)
			{
				await request.ActionHandler.PurchaseAsync(new PaywallPurchaseRequest
				{
					OfferingIdentifier = request.OfferingIdentifier,
					PackageIdentifier = package.Identifier,
					PlatformContext = request.PlatformContext
				});
			}
		}, request);
	}

	View RenderCarousel(
		PaywallCarouselComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds)
	{
		if (component.Visible == false || component.Pages.Count == 0)
		{
			return new ContentView { IsVisible = false };
		}

		var carousel = new CarouselView
		{
			ItemsSource = component.Pages,
			ItemTemplate = new DataTemplate(() =>
			{
				var host = new ContentView();
				host.BindingContextChanged += (_, _) =>
				{
					host.Content = host.BindingContext is PaywallStackComponent page
						? RenderStack(page, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds)
						: null;
				};
				return host;
			}),
			Loop = component.Loop,
			Position = Math.Clamp(component.InitialPageIndex ?? 0, 0, component.Pages.Count - 1),
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill
		};
		var fixedHeight = PaywallMauiStyleResolver.ResolveFixedHeight(component.Size);
		if (fixedHeight is > 0)
		{
			carousel.HeightRequest = fixedHeight.Value;
		}

		if (component.PagePeek is { } pagePeek)
		{
			carousel.PeekAreaInsets = new Thickness(Math.Max(0, pagePeek), 0);
		}

		if (component.PageSpacing is { } pageSpacing)
		{
			carousel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = pageSpacing
			};
		}
		StartCarouselAutoAdvance(carousel, component.AutoAdvance, component.Pages.Count, component.Loop);

		View content = carousel;
		if (component.PageControl is { ValueKind: JsonValueKind.Object } pageControl)
		{
			var indicator = CreateIndicatorView(pageControl, request.UiConfig);
			carousel.IndicatorView = indicator;
			if (fixedHeight is > 32)
			{
				carousel.HeightRequest = fixedHeight.Value - 24;
			}

			var layout = new VerticalStackLayout
			{
				Spacing = 8
			};
			if (fixedHeight is > 0)
			{
				layout.HeightRequest = fixedHeight.Value;
			}

			if (GetString(pageControl, "position") == "top")
			{
				layout.Children.Add(indicator);
				layout.Children.Add(carousel);
			}
			else
			{
				layout.Children.Add(carousel);
				layout.Children.Add(indicator);
			}

			content = layout;
		}

		var view = WrapContainer(
			content,
			component.Padding,
			component.Margin,
			component.BackgroundColor ?? component.Background,
			component.Shape,
			component.Border,
			component.Shadow,
			request.UiConfig);
		PaywallMauiStyleResolver.ApplySize(view, component.Size);
		return view;
	}

	View RenderTabs(
		PaywallTabsComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		IReadOnlyDictionary<string, string>? variables)
	{
		if (component.Tabs.Count == 0)
		{
			return new ContentView { IsVisible = false };
		}

		var selectedTabId = component.Tabs.FirstOrDefault(t => t.Id == component.DefaultTabId)?.Id ??
			component.Tabs.First().Id;
		var controlHost = new ContentView();
		var tabHost = new Grid();
		tabHost.Unloaded += (_, _) => tabHost.AbortAnimation("PaywallTabCrossfade");
		var root = new VerticalStackLayout
		{
			Spacing = 12,
			Children =
			{
				controlHost,
				tabHost
			}
		};

		void SelectTab(string tabId)
		{
			if (string.Equals(selectedTabId, tabId, StringComparison.Ordinal) ||
				!component.Tabs.Any(t => t.Id == tabId))
			{
				return;
			}

			selectedTabId = tabId;
			request.Semantics.Apply(component.StateUpdates,
				JsonSerializer.SerializeToElement(tabId, ModelSerializerContext.Default.String));
			Refresh(animateContent: true);
		}

		void Refresh(bool animateContent = false)
		{
			controlHost.Content = TryGetTabsControlStack(component.Control) is { } controlStack
				? RenderStack(
					controlStack,
					request,
					packageContextIdentifier,
					SelectTab,
					selectedTabId,
					variables,
					component.Tabs
						.Select(tab => tab.Id)
						.Where(static id => !string.IsNullOrWhiteSpace(id))
						.Select(static id => id!)
						.ToArray())
				: CreateDefaultTabControl(component.Tabs, selectedTabId, SelectTab, request);

			var tab = component.Tabs.FirstOrDefault(t => string.Equals(t.Id, selectedTabId, StringComparison.Ordinal))
				?? component.Tabs.First();
			request.Selection.SetActiveTab(component, tab.Id);
			request.SelectPackage(request.Selection.PreferredPackage(tab.Stack));
			var tabContent = tab.Stack is null
				? new ContentView { IsVisible = false }
				: RenderStack(tab.Stack, request, packageContextIdentifier, null, null, variables);
			if (animateContent)
			{
				AnimateContentSwap(tabHost, tabContent);
			}
			else
			{
				tabHost.Children.Clear();
				tabHost.Children.Add(tabContent);
			}
		}

		Refresh();

		var view = WrapContainer(
			root,
			component.Padding,
			component.Margin,
			component.BackgroundColor ?? component.Background,
			component.Shape,
			component.Border,
			component.Shadow,
			request.UiConfig);
		var applySize = CaptureSizeUpdater(view);
		ObserveSemantics(view, request, () =>
		{
			var overrides = request.Semantics.Resolve(component, packageContextIdentifier);
			view.IsVisible = request.Semantics.IsVisible(component, packageContextIdentifier);
			ApplySemanticContainerStyles(view,
				overrides.Element("padding") ?? component.Padding,
				overrides.Element("margin") ?? component.Margin,
				overrides.Element("background_color") ?? overrides.Element("background") ??
				component.BackgroundColor ?? component.Background, request.UiConfig);
			applySize(overrides.Element("size") ?? component.Size);
		});
		return view;
	}

	static void AnimateContentSwap(Grid host, View newContent)
	{
		host.AbortAnimation("PaywallTabCrossfade");
		var oldContent = host.Children.LastOrDefault() as View;
		if (oldContent is null || host.Handler is null)
		{
			host.Children.Clear();
			host.Children.Add(newContent);
			return;
		}

		oldContent.Opacity = 1;
		oldContent.InputTransparent = true;
		newContent.Opacity = 0;
		host.Children.Add(newContent);
		new Animation(progress =>
		{
			oldContent.Opacity = 1 - progress;
			newContent.Opacity = progress;
		}).Commit(host, "PaywallTabCrossfade", length: TabTransitionDuration, easing: Easing.CubicOut,
			finished: (_, _) =>
			{
				newContent.Opacity = 1;
				host.Children.Remove(oldContent);
			});
	}

	View RenderTabControlButton(
		PaywallTabControlButtonComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds)
	{
		var content = component.Stack is null
			? new Label { Text = component.TabId }
			: RenderStack(component.Stack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);
		var button = WrapTappable(content, () =>
		{
			if (!string.IsNullOrWhiteSpace(component.TabId))
			{
				tabSelected?.Invoke(component.TabId);
			}

			return Task.CompletedTask;
		}, request);
		ReservePackageSelectionStroke(button);

		if (string.Equals(component.TabId, selectedTabId, StringComparison.Ordinal))
		{
			ApplySelectedPackageStyle(button);
		}

		return button;
	}

	static View RenderTabControlToggle(
		PaywallTabControlToggleComponent component,
		PaywallRenderRequest request,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyList<string>? tabIds)
	{
		var tabIdList = tabIds ?? Array.Empty<string>();
		var offTabId = tabIdList.Count > 0 ? tabIdList[0] : null;
		var onTabId = tabIdList.Count > 1 ? tabIdList[1] : offTabId;
		var isToggled = selectedTabId is null
			? component.DefaultValue == true
			: string.Equals(selectedTabId, onTabId, StringComparison.Ordinal);

		var track = new Border
		{
			WidthRequest = 64,
			HeightRequest = 32,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) }
		};
		var thumb = new Border
		{
			WidthRequest = 28,
			HeightRequest = 28,
			Margin = 2,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
			VerticalOptions = LayoutOptions.Center
		};
		var toggle = new Grid
		{
			WidthRequest = 64,
			HeightRequest = 32,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				track,
				thumb
			}
		};

		var currentValue = isToggled;
		ApplyToggleState(track, thumb, component, request.UiConfig, currentValue);
		AddTapGesture(toggle, () =>
		{
			currentValue = !currentValue;
			ApplyToggleState(track, thumb, component, request.UiConfig, currentValue);
			var tabId = currentValue ? onTabId : offTabId;
			if (!string.IsNullOrWhiteSpace(tabId))
			{
				tabSelected?.Invoke(tabId);
			}

			return Task.CompletedTask;
		}, request);
		return toggle;
	}

	View RenderTimeline(
		PaywallTimelineComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds)
	{
		if (component.Visible == false || component.Items.Count == 0)
		{
			return new ContentView { IsVisible = false };
		}

		var layout = new VerticalStackLayout
		{
			Spacing = component.ItemSpacing ?? 12
		};

		for (var i = 0; i < component.Items.Count; i++)
		{
			var row = RenderTimelineItem(
				component.Items[i],
				request,
				packageContextIdentifier,
				tabSelected,
				selectedTabId,
				variables,
				tabIds,
				component,
				i == component.Items.Count - 1);
			if (row is not null)
			{
				layout.Children.Add(row);
			}
		}

		ApplyBoxStyles(layout, component.Padding, component.Margin, null, request.UiConfig);
		PaywallMauiStyleResolver.ApplySize(layout, component.Size);
		return layout;
	}

	View RenderCountdown(
		PaywallCountdownComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds)
	{
		var host = new ContentView();

		void Refresh()
		{
			var remaining = ResolveCountdownRemaining(component.Style);
			host.Content = RenderCountdownContent(
				component,
				request,
				packageContextIdentifier,
				tabSelected,
				selectedTabId,
				variables,
				tabIds,
				remaining);
		}

		Refresh();
		StartCountdownRefresh(host, component.Style, Refresh);
		return host;
	}

	View RenderCountdownContent(
		PaywallCountdownComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds,
		TimeSpan remaining)
	{
		var stack = remaining <= TimeSpan.Zero
			? component.EndStack ?? component.Fallback as PaywallStackComponent
			: component.CountdownStack ?? component.Fallback as PaywallStackComponent;

		if (stack is null)
		{
			return new Label
			{
				Text = FormatCountdown(remaining, component.CountFrom),
				HorizontalTextAlignment = TextAlignment.Center
			};
		}

		var countdownVariables = CreateCountdownVariables(remaining, component.CountFrom, variables);
		return RenderStack(stack, request, packageContextIdentifier, tabSelected, selectedTabId, countdownVariables, tabIds);
	}

	View RenderVideo(
		PaywallVideoComponent component,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds)
	{
		if (component.Fallback is not null)
		{
			return RenderComponent(component.Fallback, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);
		}

		var source = PaywallMauiStyleResolver.ResolveImageUrl(component.Source);
		return new Label
		{
			Text = string.IsNullOrWhiteSpace(source) ? "Video" : $"Video: {source}",
			HorizontalTextAlignment = TextAlignment.Center,
			TextColor = Colors.Gray
		};
	}

	IndicatorView CreateIndicatorView(JsonElement pageControl, PaywallUiConfig? uiConfig)
	{
		var indicator = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			IndicatorColor = Colors.LightGray,
			SelectedIndicatorColor = Colors.DeepSkyBlue
		};

		if (pageControl.TryGetProperty("default", out var defaultIndicator) &&
			defaultIndicator.ValueKind == JsonValueKind.Object)
		{
			indicator.IndicatorColor = ResolveIndicatorColor(defaultIndicator, uiConfig) ?? indicator.IndicatorColor;
			var width = GetDouble(defaultIndicator, "width");
			var height = GetDouble(defaultIndicator, "height");
			if (width > 0)
			{
				indicator.IndicatorSize = width;
			}
			if (height > 0)
			{
				indicator.IndicatorSize = Math.Max(indicator.IndicatorSize, height);
			}
		}

		if (pageControl.TryGetProperty("active", out var activeIndicator) &&
			activeIndicator.ValueKind == JsonValueKind.Object)
		{
			indicator.SelectedIndicatorColor = ResolveIndicatorColor(activeIndicator, uiConfig) ?? indicator.SelectedIndicatorColor;
		}

		return indicator;
	}

	static void StartCarouselAutoAdvance(CarouselView carousel, JsonElement? autoAdvance, int pageCount, bool loop)
	{
		if (pageCount <= 1 || ResolveAutoAdvanceInterval(autoAdvance) is not { } interval)
		{
			return;
		}

		IDispatcherTimer? timer = null;

		void Stop()
		{
			timer?.Stop();
			timer = null;
		}

		void Start()
		{
			if (timer is not null || !carousel.IsLoaded || carousel.Handler is null)
			{
				return;
			}

			timer = carousel.Dispatcher.CreateTimer();
			timer.Interval = interval;
			timer.Tick += (_, _) =>
			{
				if (!carousel.IsLoaded || carousel.Handler is null)
				{
					Stop();
					return;
				}

				var next = carousel.Position + 1;
				if (next >= pageCount)
				{
					if (!loop)
					{
						Stop();
						return;
					}

					next = 0;
				}

				carousel.ScrollTo(next, position: ScrollToPosition.Center, animate: true);
				carousel.Position = next;
			};
			timer.Start();
		}

		carousel.Loaded += (_, _) => Start();
		carousel.Unloaded += (_, _) => Stop();
		carousel.HandlerChanged += (_, _) => Start();
		carousel.HandlerChanging += (_, _) => Stop();
		if (carousel.Handler is not null)
		{
			Start();
		}
	}

	static TimeSpan? ResolveAutoAdvanceInterval(JsonElement? autoAdvance)
	{
		if (autoAdvance is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			autoAdvance.Value.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		var milliseconds = GetDouble(autoAdvance.Value, "ms_time_per_page");
		if (milliseconds <= 0)
		{
			milliseconds = GetDouble(autoAdvance.Value, "msTimePerPage");
		}

		return milliseconds <= 0 ? null : TimeSpan.FromMilliseconds(milliseconds);
	}

	static void StartCountdownRefresh(ContentView host, JsonElement? style, Action refresh)
	{
		if (ResolveCountdownRemaining(style) <= TimeSpan.Zero)
		{
			return;
		}

		IDispatcherTimer? timer = null;

		void Stop()
		{
			timer?.Stop();
			timer = null;
		}

		void Start()
		{
			if (timer is not null || !host.IsLoaded || host.Handler is null)
			{
				return;
			}

			timer = host.Dispatcher.CreateTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += (_, _) =>
			{
				if (!host.IsLoaded)
				{
					Stop();
					return;
				}
				refresh();
				if (ResolveCountdownRemaining(style) <= TimeSpan.Zero)
				{
					Stop();
				}
			};
			timer.Start();
		}

		host.Loaded += (_, _) =>
		{
			refresh();
			if (ResolveCountdownRemaining(style) > TimeSpan.Zero)
			{
				Start();
			}
		};
		host.Unloaded += (_, _) => Stop();
		host.HandlerChanged += (_, _) => Start();
		host.HandlerChanging += (_, _) => Stop();
		if (host.Handler is not null)
		{
			Start();
		}
	}

	static void ApplyToggleState(
		Border track,
		Border thumb,
		PaywallTabControlToggleComponent component,
		PaywallUiConfig? uiConfig,
		bool isToggled)
	{
		var trackColor = PaywallMauiStyleResolver.ResolveColor(
				isToggled ? component.TrackColorOn : component.TrackColorOff,
				uiConfig)
			?? (isToggled ? Colors.DeepSkyBlue : Colors.LightGray);
		var thumbColor = PaywallMauiStyleResolver.ResolveColor(
				isToggled ? component.ThumbColorOn : component.ThumbColorOff,
				uiConfig)
			?? Colors.White;

		track.Background = new SolidColorBrush(trackColor);
		thumb.Background = new SolidColorBrush(thumbColor);
		thumb.HorizontalOptions = isToggled ? LayoutOptions.End : LayoutOptions.Start;
	}

	static Color? ResolveIndicatorColor(JsonElement indicator, PaywallUiConfig? uiConfig)
	{
		return indicator.TryGetProperty("color", out var color)
			? PaywallMauiStyleResolver.ResolveColor(color, uiConfig)
			: null;
	}

	static PaywallStackComponent? TryGetTabsControlStack(JsonElement? control)
	{
		if (control is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			control.Value.ValueKind != JsonValueKind.Object ||
			!control.Value.TryGetProperty("stack", out var stack) ||
			stack.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		return stack.Deserialize(ModelSerializerContext.Default.PaywallStackComponent);
	}

	static View CreateDefaultTabControl(
		IReadOnlyList<PaywallTabComponent> tabs,
		string? selectedTabId,
		Action<string> selectTab,
		PaywallRenderRequest request)
	{
		var layout = new HorizontalStackLayout
		{
			Spacing = 8,
			HorizontalOptions = LayoutOptions.Center
		};

		foreach (var tab in tabs)
		{
			var label = new Label
			{
				Text = tab.Name ?? tab.Id,
				FontAttributes = FontAttributes.Bold,
				TextColor = Colors.Black,
				HorizontalTextAlignment = TextAlignment.Center
			};
			var border = new Border
			{
				Content = label,
				Padding = new Thickness(14, 8),
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
				StrokeThickness = 2,
				Stroke = string.Equals(tab.Id, selectedTabId, StringComparison.Ordinal)
					? Colors.DeepSkyBlue
					: Colors.Transparent,
				Background = new SolidColorBrush(Colors.White)
			};
			AddTapGesture(border, () =>
			{
				if (!string.IsNullOrWhiteSpace(tab.Id))
				{
					selectTab(tab.Id);
				}

				return Task.CompletedTask;
			}, request);
			layout.Children.Add(border);
		}

		return layout;
	}

	View? RenderTimelineItem(
		JsonElement item,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds,
		PaywallTimelineComponent component,
		bool isLast)
	{
		if (item.ValueKind != JsonValueKind.Object ||
			item.TryGetProperty("visible", out var visibleElement) && visibleElement.ValueKind == JsonValueKind.False ||
			!item.TryGetProperty("title", out var titleElement) ||
			!item.TryGetProperty("icon", out var iconElement))
		{
			return null;
		}

		var title = titleElement.Deserialize(ModelSerializerContext.Default.PaywallTextComponent);
		var icon = iconElement.Deserialize(ModelSerializerContext.Default.PaywallIconComponent);
		if (title is null || icon is null)
		{
			return null;
		}

		var row = new Grid
		{
			ColumnSpacing = component.ColumnGutter ?? 12,
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)
			}
		};

		var timelineMarker = CreateTimelineMarker(item, icon, request.UiConfig, component, isLast);
		row.Children.Add(timelineMarker);

		var textStack = new VerticalStackLayout
		{
			Spacing = component.TextSpacing ?? 4,
			VerticalOptions = LayoutOptions.Center
		};
		textStack.Children.Add(RenderText(title, request, packageContextIdentifier, variables));

		if (item.TryGetProperty("description", out var descriptionElement) &&
			descriptionElement.ValueKind == JsonValueKind.Object &&
			descriptionElement.Deserialize(ModelSerializerContext.Default.PaywallTextComponent) is { } description)
		{
			textStack.Children.Add(RenderText(description, request, packageContextIdentifier, variables));
		}

		Grid.SetColumn(textStack, 1);
		row.Children.Add(textStack);
		return row;
	}

	View CreateTimelineMarker(
		JsonElement item,
		PaywallIconComponent icon,
		PaywallUiConfig? uiConfig,
		PaywallTimelineComponent component,
		bool isLast)
	{
		var marker = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Fill
		};

		var iconView = RenderIcon(icon);
		iconView.VerticalOptions = component.IconAlignment == "title_and_description"
			? LayoutOptions.Center
			: LayoutOptions.Start;
		marker.Children.Add(iconView);

		if (!isLast && CreateTimelineConnector(item, uiConfig) is { } connector)
		{
			Grid.SetRow(connector, 1);
			marker.Children.Add(connector);
		}

		return marker;
	}

	View? CreateTimelineConnector(JsonElement item, PaywallUiConfig? uiConfig)
	{
		JsonElement? connectorElement = item.TryGetProperty("connector", out var connector) &&
			connector.ValueKind == JsonValueKind.Object
				? connector
				: null;
		if (connectorElement is { } explicitConnector &&
			explicitConnector.TryGetProperty("visible", out var visible) &&
			visible.ValueKind == JsonValueKind.False)
		{
			return null;
		}

		var width = connectorElement is { } connectorObject
			? GetDouble(connectorObject, "width")
			: 2;
		if (width <= 0)
		{
			width = 2;
		}

		var color = connectorElement is { } colorConnector &&
			colorConnector.TryGetProperty("color", out var colorElement)
				? PaywallMauiStyleResolver.ResolveColor(colorElement, uiConfig)
				: Color.FromArgb("#cbd5e1");
		var margin = connectorElement is { } marginConnector &&
			marginConnector.TryGetProperty("margin", out var marginElement)
				? PaywallMauiStyleResolver.ResolveThickness(marginElement)
				: new Thickness(0, 4, 0, 0);

		return new BoxView
		{
			WidthRequest = width,
			Margin = margin,
			Color = color ?? Color.FromArgb("#cbd5e1"),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Fill
		};
	}

	static TimeSpan ResolveCountdownRemaining(JsonElement? style)
	{
		if (style is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			style.Value.ValueKind != JsonValueKind.Object ||
			!style.Value.TryGetProperty("date", out var dateElement) ||
			dateElement.ValueKind != JsonValueKind.String ||
			!DateTimeOffset.TryParse(dateElement.GetString(), out var endDate))
		{
			return TimeSpan.Zero;
		}

		return endDate - DateTimeOffset.UtcNow;
	}

	static IReadOnlyDictionary<string, string> CreateCountdownVariables(
		TimeSpan remaining,
		string? countFrom,
		IReadOnlyDictionary<string, string>? existingVariables)
	{
		var safeRemaining = remaining <= TimeSpan.Zero ? TimeSpan.Zero : remaining;
		var variables = existingVariables is null
			? new Dictionary<string, string>(StringComparer.Ordinal)
			: new Dictionary<string, string>(existingVariables, StringComparer.Ordinal);

		variables["days"] = Math.Floor(safeRemaining.TotalDays).ToString("0");
		variables["hours"] = Math.Floor(safeRemaining.TotalHours).ToString("0");
		variables["minutes"] = Math.Floor(safeRemaining.TotalMinutes).ToString("0");
		variables["seconds"] = Math.Floor(safeRemaining.TotalSeconds).ToString("0");
		variables["countdown"] = FormatCountdown(safeRemaining, countFrom);
		return variables;
	}

	static string FormatCountdown(TimeSpan remaining, string? countFrom)
	{
		var safeRemaining = remaining <= TimeSpan.Zero ? TimeSpan.Zero : remaining;
		return countFrom switch
		{
			"days" => $"{Math.Ceiling(safeRemaining.TotalDays):0} days",
			"hours" => $"{Math.Ceiling(safeRemaining.TotalHours):0} hours",
			"minutes" => $"{Math.Ceiling(safeRemaining.TotalMinutes):0} minutes",
			_ => $"{(int)safeRemaining.TotalDays:00}:{safeRemaining.Hours:00}:{safeRemaining.Minutes:00}:{safeRemaining.Seconds:00}"
		};
	}

	static View WrapTappable(View content, Func<Task> tapped, PaywallRenderRequest request)
	{
		AddTapGesture(content, tapped, request);
		return content;
	}

	static View MakePackageTappable(View content, Func<Task> tapped, PaywallRenderRequest request)
	{
		var target = FindFirstBorder(content) is null
			? new Border
			{
				Content = content,
				Padding = 0,
				StrokeThickness = 0
			}
			: content;

		AddTapGesture(target, tapped, request);
		return target;
	}

	static void AddTapGesture(View target, Func<Task> tapped, PaywallRenderRequest request)
	{
		var tap = new TapGestureRecognizer
		{
			Command = new Command(async () =>
			{
				if (request.ActionInProgress || !target.IsEnabled)
				{
					return;
				}
				request.ActionInProgress = true;
				try
				{
					await tapped();
				}
				catch (Exception error)
				{
					System.Diagnostics.Trace.TraceError("Paywall action failed: {0}", error);
					request.ActionFailed?.Invoke(error);
				}
				finally
				{
					request.ActionInProgress = false;
				}
			})
		};
		target.GestureRecognizers.Add(tap);
	}

	static void ObserveSelection(View view, PaywallRenderRequest request, Action update)
	{
		update();
		request.Selection.Changed += update;
		view.Unloaded += (_, _) => request.Selection.Changed -= update;
		view.Loaded += (_, _) =>
		{
			request.Selection.Changed -= update;
			request.Selection.Changed += update;
			update();
		};
	}

	static void ObserveSemantics(View view, PaywallRenderRequest request, Action update)
	{
		update();
		request.Semantics.Changed += update;
		view.Unloaded += (_, _) => request.Semantics.Changed -= update;
		view.Loaded += (_, _) =>
		{
			request.Semantics.Changed -= update;
			request.Semantics.Changed += update;
			update();
		};
	}

	static Package? FindPackage(PaywallRenderRequest request, string? packageIdentifier, bool allowDefaultPackage)
	{
		var package = request.Packages.FirstOrDefault(p => string.Equals(p.Identifier, packageIdentifier, StringComparison.Ordinal));
		return package ?? (allowDefaultPackage ? request.Packages.FirstOrDefault() : null);
	}

	static string? TryGetUrl(JsonElement? url, PaywallRenderRequest request)
	{
		if (url is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
		{
			return null;
		}

		if (url.Value.ValueKind == JsonValueKind.String)
		{
			return url.Value.GetString();
		}

		if (url.Value.ValueKind == JsonValueKind.Object &&
			url.Value.TryGetProperty("url_lid", out var urlLid) && urlLid.ValueKind == JsonValueKind.String)
		{
			return PaywallLocalizationResolver.ResolveText(request.PaywallData, request.Locale, urlLid.GetString())
				?? throw new InvalidOperationException($"No localization was found for paywall URL '{urlLid.GetString()}'.");
		}

		return null;
	}

	static string? GetString(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
			? value.GetString()
			: null;

	static double GetDouble(JsonElement element, string propertyName) =>
		element.TryGetProperty(propertyName, out var value) && value.TryGetDouble(out var number)
			? number
			: 0;

	static View CreateImageView(string? source, Aspect aspect, JsonElement? maskShape = null)
	{
		if (IsSvgSource(source))
		{
			return new SvgPaywallImageView
			{
				Source = source,
				Aspect = aspect,
				ClipCornerRadius = PaywallMauiStyleResolver.ResolveCornerRadius(maskShape)
			};
		}

		var image = new Image
		{
			Aspect = aspect
		};

		if (!string.IsNullOrWhiteSpace(source))
		{
			image.Source = CreateImageSource(source);
		}

		return image;
	}

	static bool IsSvgSource(string? source)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			return false;
		}

		if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
		{
			return string.Equals(System.IO.Path.GetExtension(uri.AbsolutePath), ".svg", StringComparison.OrdinalIgnoreCase);
		}

		return string.Equals(System.IO.Path.GetExtension(source), ".svg", StringComparison.OrdinalIgnoreCase);
	}

	static ImageSource CreateImageSource(string source)
	{
		if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
		{
			return ImageSource.FromUri(uri);
		}

		return ImageSource.FromFile(System.IO.Path.GetFileName(source.TrimStart('/', '\\')));
	}

	static string? GetIconAsset(PaywallIconComponent component)
	{
		foreach (var format in new[] { "webp", "png", "jpeg", "jpg", "svg" })
		{
			if (component.Formats.TryGetValue(format, out var asset) && !string.IsNullOrWhiteSpace(asset))
			{
				if (Uri.TryCreate(asset, UriKind.Absolute, out _))
				{
					return asset;
				}

				return string.IsNullOrWhiteSpace(component.BaseUrl)
					? asset
					: $"{component.BaseUrl.TrimEnd('/')}/{asset.TrimStart('/')}";
			}
		}

		return null;
	}

	View ApplyBadge(
		View content,
		JsonElement? badge,
		PaywallRenderRequest request,
		string? packageContextIdentifier,
		Action<string>? tabSelected,
		string? selectedTabId,
		IReadOnlyDictionary<string, string>? variables,
		IReadOnlyList<string>? tabIds)
	{
		if (badge is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } ||
			badge.Value.ValueKind != JsonValueKind.Object ||
			!badge.Value.TryGetProperty("stack", out var stack) ||
			stack.ValueKind != JsonValueKind.Object)
		{
			return content;
		}

		var badgeStack = stack.Deserialize(ModelSerializerContext.Default.PaywallStackComponent);
		if (badgeStack is null)
		{
			return content;
		}

		var grid = new Grid { Margin = content.Margin };
		content.Margin = default;
		grid.Children.Add(content);

		var badgeView = RenderStack(badgeStack, request, packageContextIdentifier, tabSelected, selectedTabId, variables, tabIds);
		var alignment = badge.Value.TryGetProperty("alignment", out var alignmentElement) &&
			alignmentElement.ValueKind == JsonValueKind.String
				? alignmentElement.GetString()
				: "top_trailing";
		ApplyBadgeAlignment(badgeView, alignment);
		ApplyBadgeOverlayOffset(badgeView, alignment);
		grid.Children.Add(badgeView);

		return grid;
	}

	static void ApplyBadgeAlignment(View view, string? alignment)
	{
		view.HorizontalOptions = alignment switch
		{
			"top_leading" or "bottom_leading" or "leading" => LayoutOptions.Start,
			"top_trailing" or "bottom_trailing" or "trailing" => LayoutOptions.End,
			_ => LayoutOptions.Center
		};
		view.VerticalOptions = alignment switch
		{
			"bottom_leading" or "bottom_trailing" or "bottom" => LayoutOptions.End,
			"top_leading" or "top_trailing" or "top" => LayoutOptions.Start,
			_ => LayoutOptions.Center
		};
	}

	static void ApplyBadgeOverlayOffset(View view, string? alignment)
	{
		const double edgeInset = 12;
		const double floatOffset = 10;
		var margin = view.Margin;

		switch (alignment)
		{
			case "top_trailing":
				view.Margin = new Thickness(margin.Left, margin.Top - floatOffset, margin.Right + edgeInset, margin.Bottom);
				break;
			case "top_leading":
				view.Margin = new Thickness(margin.Left + edgeInset, margin.Top - floatOffset, margin.Right, margin.Bottom);
				break;
			case "bottom_trailing":
				view.Margin = new Thickness(margin.Left, margin.Top, margin.Right + edgeInset, margin.Bottom - floatOffset);
				break;
			case "bottom_leading":
				view.Margin = new Thickness(margin.Left + edgeInset, margin.Top, margin.Right, margin.Bottom - floatOffset);
				break;
		}
	}

	static View WrapContainer(
		View content,
		JsonElement? padding,
		JsonElement? margin,
		JsonElement? background,
		JsonElement? shape,
		JsonElement? border,
		JsonElement? shadow,
		PaywallUiConfig? uiConfig)
	{
		content.Margin = default;

		if (!PaywallMauiStyleResolver.HasContainerDecoration(shape, border, shadow))
		{
			ApplyBoxStyles(content, padding, margin, background, uiConfig);
			return content;
		}

		var container = new Border
		{
			Content = content,
			Padding = PaywallMauiStyleResolver.ResolveThickness(padding),
			Margin = PaywallMauiStyleResolver.ResolveThickness(margin),
			Background = PaywallMauiStyleResolver.ResolveBackground(background, uiConfig),
			StrokeShape = PaywallMauiStyleResolver.ResolveStrokeShape(shape),
			Stroke = PaywallMauiStyleResolver.ResolveBorderBrush(border, uiConfig),
			StrokeThickness = PaywallMauiStyleResolver.ResolveBorderWidth(border)
		};
		var resolvedShadow = PaywallMauiStyleResolver.ResolveShadow(shadow, uiConfig);
		if (resolvedShadow is not null)
		{
			container.Shadow = resolvedShadow;
		}

		if (container.StrokeThickness <= 0)
		{
			container.Stroke = null;
		}

		return container;
	}

	static void ApplySelectedPackageStyle(View view)
	{
		var border = FindFirstBorder(view);
		if (border is null)
		{
			return;
		}

		border.Stroke = Colors.DeepSkyBlue;
		if (border.StrokeThickness <= 0)
		{
			border.StrokeThickness = DefaultPackageSelectionStrokeThickness;
		}

		if (border.StrokeShape is null or Rectangle)
		{
			border.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) };
		}
	}

	static void ReservePackageSelectionStroke(View view)
	{
		var border = FindFirstBorder(view);
		if (border is null || border.StrokeThickness > 0)
		{
			return;
		}

		border.Stroke = Colors.Transparent;
		border.StrokeThickness = DefaultPackageSelectionStrokeThickness;
		if (border.StrokeShape is null or Rectangle)
		{
			border.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) };
		}
	}

	static Border? FindFirstBorder(View view)
	{
		if (view is Border border)
		{
			return border;
		}

		if (view is Layout layout)
		{
			foreach (var child in layout.Children.OfType<View>())
			{
				var result = FindFirstBorder(child);
				if (result is not null)
				{
					return result;
				}
			}
		}
		else if (view is ContentView { Content: View content })
		{
			return FindFirstBorder(content);
		}

		return null;
	}

	static void ApplyBoxStyles(View view, JsonElement? padding, JsonElement? margin, JsonElement? background, PaywallUiConfig? uiConfig)
	{
		view.Margin = PaywallMauiStyleResolver.ResolveThickness(margin);

		if (view is Layout layout)
		{
			layout.Padding = PaywallMauiStyleResolver.ResolveThickness(padding);
			layout.Background = PaywallMauiStyleResolver.ResolveBackground(background, uiConfig);
		}
		else
		{
			view.Background = PaywallMauiStyleResolver.ResolveBackground(background, uiConfig);
		}
	}

	static void ApplySemanticContainerStyles(
		View view, JsonElement? padding, JsonElement? margin, JsonElement? background, PaywallUiConfig? uiConfig)
	{
		if (view is Border border)
		{
			border.Padding = PaywallMauiStyleResolver.ResolveThickness(padding);
			border.Margin = PaywallMauiStyleResolver.ResolveThickness(margin);
			border.Background = PaywallMauiStyleResolver.ResolveBackground(background, uiConfig);
		}
		else
		{
			ApplyBoxStyles(view, padding, margin, background, uiConfig);
		}
	}

	static Action<JsonElement?> CaptureSizeUpdater(View view)
	{
		var initialWidth = view.WidthRequest;
		var initialHeight = view.HeightRequest;
		var initialHorizontal = view.HorizontalOptions;
		var initialVertical = view.VerticalOptions;
		var expectedHorizontal = initialHorizontal;
		var expectedVertical = initialVertical;
		var horizontalOwned = true;
		var verticalOwned = true;
		JsonElement? previousWidth = null;
		JsonElement? previousHeight = null;
		return size =>
		{
			UpdateConstraint(GetConstraint(size, "width"), ref previousWidth, isWidth: true);
			UpdateConstraint(GetConstraint(size, "height"), ref previousHeight, isWidth: false);
		};

		void UpdateConstraint(JsonElement? next, ref JsonElement? previous, bool isWidth)
		{
			if (isWidth && !view.HorizontalOptions.Equals(expectedHorizontal))
			{
				horizontalOwned = false;
			}
			else if (!isWidth && !view.VerticalOptions.Equals(expectedVertical))
			{
				verticalOwned = false;
			}
			if (previous?.GetRawText() == next?.GetRawText())
			{
				return;
			}

			if (previous is { } old)
			{
				var oldType = PaywallMauiStyleResolver.GetType(old);
				if (oldType == "fixed" && old.TryGetProperty("value", out var fixedValue) &&
					fixedValue.TryGetDouble(out var lastValue))
				{
					if (isWidth && view.WidthRequest == lastValue)
					{
						view.WidthRequest = initialWidth;
					}
					else if (!isWidth && view.HeightRequest == lastValue)
					{
						view.HeightRequest = initialHeight;
					}
				}
				else if (oldType == "fill")
				{
					if (isWidth && horizontalOwned)
					{
						view.HorizontalOptions = initialHorizontal;
						expectedHorizontal = initialHorizontal;
					}
					else if (!isWidth && verticalOwned)
					{
						view.VerticalOptions = initialVertical;
						expectedVertical = initialVertical;
					}
				}
			}

			if (next is { } constraint)
			{
				if (PaywallMauiStyleResolver.GetType(constraint) != "fill" ||
					(isWidth ? horizontalOwned : verticalOwned))
				{
					PaywallMauiStyleResolver.ApplySizeConstraint(view, constraint, isWidth);
					if (PaywallMauiStyleResolver.GetType(constraint) == "fill")
					{
						if (isWidth)
						{
							expectedHorizontal = LayoutOptions.Fill;
						}
						else
						{
							expectedVertical = LayoutOptions.Fill;
						}
					}
				}
			}
			previous = next;
		}
	}

	static JsonElement? GetConstraint(JsonElement? size, string dimension) =>
		size is { ValueKind: JsonValueKind.Object } value &&
		value.TryGetProperty(dimension, out var constraint) && constraint.ValueKind == JsonValueKind.Object
			? constraint : null;

	sealed class OverlayPaywallVariableProvider(
		IPaywallVariableProvider inner,
		IReadOnlyDictionary<string, string> variables) : IPaywallVariableProvider
	{
		public string? Resolve(string variableName, PaywallVariableContext context) =>
			variables.TryGetValue(variableName, out var value)
				? value
				: inner.Resolve(variableName, context);
	}
}
