#nullable enable

using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

public sealed class DefaultPaywallRenderer : IPaywallRenderer
{
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

	View RenderComponent(PaywallComponent component, PaywallRenderRequest request)
	{
		if (component is PaywallUnknownComponent { Fallback: { } fallback })
		{
			return RenderComponent(fallback, request);
		}

		return component switch
		{
			PaywallStackComponent stack => RenderStack(stack, request),
			PaywallTextComponent text => RenderText(text, request),
			PaywallImageComponent image => RenderImage(image, request),
			PaywallIconComponent icon => RenderIcon(icon),
			PaywallButtonComponent button => RenderButton(button, request),
			PaywallPackageComponent package => RenderPackage(package, request),
			PaywallPurchaseButtonComponent purchaseButton => RenderPurchaseButton(purchaseButton, request),
			PaywallHeaderComponent header when header.Stack is not null => RenderStack(header.Stack, request),
			PaywallStickyFooterComponent footer when footer.Stack is not null => RenderStack(footer.Stack, request),
			{ Fallback: { } componentFallback } => RenderComponent(componentFallback, request),
			_ => new ContentView { IsVisible = false }
		};
	}

	View RenderStack(PaywallStackComponent component, PaywallRenderRequest request)
	{
		if (component.Visible == false)
		{
			return new ContentView { IsVisible = false };
		}

		var layout = CreateStackLayout(component);

		foreach (var child in component.Components)
		{
			layout.Children.Add(RenderComponent(child, request));
		}

		var view = WrapContainer(
			layout,
			component.Padding,
			component.Margin,
			component.BackgroundColor ?? component.Background,
			component.Shape,
			component.Border,
			component.Shadow,
			request.UiConfig);
		view = ApplyBadge(view, component.Badge, request);
		PaywallMauiStyleResolver.ApplySize(view, component.Size);

		return view;
	}

	Layout CreateStackLayout(PaywallStackComponent component)
	{
		var dimensionType = PaywallMauiStyleResolver.GetType(component.Dimension);
		var spacing = component.Spacing ?? 0;

		if (dimensionType == "horizontal")
		{
			return new HorizontalStackLayout
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

	View RenderText(PaywallTextComponent component, PaywallRenderRequest request)
	{
		if (component.Visible == false)
		{
			return new ContentView { IsVisible = false };
		}

		var text = PaywallLocalizationResolver.ResolveText(request.PaywallData, request.Locale, component.TextLocalizationId)
			?? component.TextLocalizationId
			?? string.Empty;
		var package = FindPackage(request, request.SelectedPackageIdentifier);
		var variableProvider = request.VariableProvider ?? new DefaultPaywallVariableProvider();
		text = PaywallTextProcessor.ProcessVariables(text, variableProvider, new PaywallVariableContext
		{
			Package = package,
			ApplicationName = request.ApplicationName,
			Locale = request.Locale
		});

		var label = new Label
		{
			Text = text,
			TextColor = PaywallMauiStyleResolver.ResolveColor(component.Color, request.UiConfig) ?? Colors.Black,
			HorizontalTextAlignment = PaywallMauiStyleResolver.ResolveTextAlignment(component.HorizontalAlignment),
			FontSize = PaywallMauiStyleResolver.ResolveFontSize(component.FontSize),
			FontAttributes = PaywallMauiStyleResolver.ResolveFontAttributes(component.FontWeight, component.FontWeightInt),
			LineBreakMode = LineBreakMode.WordWrap
		};

		ApplyBoxStyles(label, component.Padding, component.Margin, component.BackgroundColor, request.UiConfig);
		PaywallMauiStyleResolver.ApplySize(label, component.Size);

		return label;
	}

	View RenderImage(PaywallImageComponent component, PaywallRenderRequest request)
	{
		if (component.Visible == false)
		{
			return new ContentView { IsVisible = false };
		}

		var image = new Image
		{
			Aspect = PaywallMauiStyleResolver.ResolveAspect(component.FitMode)
		};

		var source = PaywallMauiStyleResolver.ResolveImageUrl(component.Source);
		if (component.OverrideSourceLocalizationId is not null)
		{
			source = PaywallMauiStyleResolver.ResolveLocalizedImageUrl(
				request.PaywallData,
				request.Locale,
				component.OverrideSourceLocalizationId) ?? source;
		}

		if (!string.IsNullOrWhiteSpace(source))
		{
			image.Source = CreateImageSource(source);
		}

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
		var image = new Image
		{
			Aspect = Aspect.AspectFit
		};

		var asset = GetIconAsset(component);
		if (asset is not null)
		{
			image.Source = CreateImageSource(asset);
		}

		PaywallMauiStyleResolver.ApplySize(image, component.Size);
		return image;
	}

	View RenderButton(PaywallButtonComponent component, PaywallRenderRequest request)
	{
		var content = component.Stack is null
			? new Label { Text = component.Action?.Type ?? "Button" }
			: RenderStack(component.Stack, request);

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
					Url = TryGetUrl(component.Action?.Url)
				});
			}
		});

		return button;
	}

	View RenderPackage(PaywallPackageComponent component, PaywallRenderRequest request)
	{
		var content = component.Stack is null
			? new Label { Text = component.PackageId }
			: RenderStack(component.Stack, request);
		var border = WrapTappable(content, () =>
		{
			if (!string.IsNullOrWhiteSpace(component.PackageId))
			{
				request.PackageSelected?.Invoke(component.PackageId);
			}

			return Task.CompletedTask;
		});

		if (string.Equals(component.PackageId, request.SelectedPackageIdentifier, StringComparison.Ordinal))
		{
			border.Stroke = Colors.DeepSkyBlue;
			border.StrokeThickness = 2;
			border.StrokeShape = content is Border { StrokeShape: { } strokeShape }
				? strokeShape
				: new RoundRectangle { CornerRadius = new CornerRadius(18) };
		}

		return border;
	}

	View RenderPurchaseButton(PaywallPurchaseButtonComponent component, PaywallRenderRequest request)
	{
		var content = component.Stack is null
			? new Label { Text = "Purchase" }
			: RenderStack(component.Stack, request);

		return WrapTappable(content, async () =>
		{
			var packageIdentifier = request.SelectedPackageIdentifier ?? request.Packages.FirstOrDefault()?.Identifier;
			if (!string.IsNullOrWhiteSpace(packageIdentifier) && request.ActionHandler is not null)
			{
				await request.ActionHandler.PurchaseAsync(new PaywallPurchaseRequest
				{
					OfferingIdentifier = request.OfferingIdentifier,
					PackageIdentifier = packageIdentifier,
					PlatformContext = request.PlatformContext
				});
			}
		});
	}

	static Border WrapTappable(View content, Func<Task> tapped)
	{
		var border = new Border
		{
			Content = content,
			Padding = 0
		};
		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) => await tapped();
		border.GestureRecognizers.Add(tap);
		return border;
	}

	static Package? FindPackage(PaywallRenderRequest request, string? packageIdentifier) =>
		request.Packages.FirstOrDefault(p => string.Equals(p.Identifier, packageIdentifier, StringComparison.Ordinal))
			?? request.Packages.FirstOrDefault();

	static string? TryGetUrl(JsonElement? url)
	{
		if (url is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
		{
			return null;
		}

		if (url.Value.ValueKind == JsonValueKind.String)
		{
			return url.Value.GetString();
		}

		if (url.Value.TryGetProperty("url_lid", out var urlLid) && urlLid.ValueKind == JsonValueKind.String)
		{
			return urlLid.GetString();
		}

		return null;
	}

	static ImageSource CreateImageSource(string source) =>
		Uri.TryCreate(source, UriKind.Absolute, out var uri)
			? ImageSource.FromUri(uri)
			: ImageSource.FromFile(source);

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

	View ApplyBadge(View content, JsonElement? badge, PaywallRenderRequest request)
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

		var badgeView = RenderStack(badgeStack, request);
		var alignment = badge.Value.TryGetProperty("alignment", out var alignmentElement) &&
			alignmentElement.ValueKind == JsonValueKind.String
				? alignmentElement.GetString()
				: "top_trailing";
		ApplyBadgeAlignment(badgeView, alignment);
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
}
