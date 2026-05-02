using System.Text.Json;
using Plugin.RevenueCat.Models;

namespace PaywallGallerySample;

public static class PaywallExampleLoader
{
	static readonly PaywallExampleDefinition[] Definitions =
	[
		new(
			"Basic subscription",
			"Minimal stack, package card, sticky purchase button, and product variables.",
			"stack, text, package, purchase_button, sticky_footer",
			"paywalls/basic.json"),
		new(
			"Media header",
			"Hero image, icon, header region, and local image assets.",
			"header, image, icon, stack, purchase_button",
			"paywalls/header_media.json"),
		new(
			"Package selector",
			"Multiple packages with selected-state highlighting and price variables.",
			"package, horizontal stack, text variables, purchase_button",
			"paywalls/package_selector.json"),
		new(
			"Actions and fallback",
			"Restore and navigation actions plus unknown-component fallback rendering.",
			"button, restore, navigation, unknown fallback",
			"paywalls/actions_fallback.json"),
		new(
			"Best value badge",
			"Annual package card with RevenueCat-style badge overlay, rounded border, and shadow.",
			"badge, shape, border, shadow, package",
			"paywalls/best_value_badge.json"),
		new(
			"Dark gradient",
			"Full-screen dark gradient, glassy rounded feature card, and pill purchase button.",
			"linear gradient, rounded card, pill CTA",
			"paywalls/dark_gradient.json"),
		new(
			"Feature checklist",
			"Benefit rows and compact plan card inspired by checklist-style subscription templates.",
			"horizontal stacks, icon assets, rounded rows",
			"paywalls/feature_checklist.json"),
		new(
			"Lifetime launch offer",
			"High-contrast hero layout for a one-time lifetime purchase.",
			"hero image, lifetime package, sticky footer",
			"paywalls/lifetime_offer.json"),
		new(
			"Bottom sheet",
			"Sheet-like paywall surface with rounded top corners and legal action buttons.",
			"rounded sheet, restore, terms, sticky footer",
			"paywalls/bottom_sheet.json"),
		new(
			"Two-column benefits",
			"Card grid layout using horizontal stacks and RevenueCat color aliases.",
			"color aliases, horizontal stacks, cards",
			"paywalls/two_column_benefits.json"),
		new(
			"Trial story",
			"Timeline-like onboarding story using supported stacks and fallback-safe components.",
			"story cards, fallback, package, purchase",
			"paywalls/trial_story.json"),
		new(
			"Premium comparison",
			"Plan comparison cards with separate monthly and annual CTAs.",
			"multiple packages, badges, rounded comparison cards",
			"paywalls/premium_comparison.json")
	];

	public static async Task<IReadOnlyList<PaywallExample>> LoadAsync()
	{
		var examples = new List<PaywallExample>();
		foreach (var definition in Definitions)
		{
			await using var stream = await FileSystem.OpenAppPackageFileAsync(definition.AssetName);
			var response = await JsonSerializer.DeserializeAsync(stream, ModelSerializerContext.Default.PaywallOfferingsResponse)
				?? throw new InvalidOperationException($"Paywall fixture '{definition.AssetName}' could not be parsed.");

			examples.Add(new PaywallExample(
				definition.Title,
				definition.Description,
				definition.ComponentSummary,
				response));
		}

		return examples;
	}

	sealed record PaywallExampleDefinition(
		string Title,
		string Description,
		string ComponentSummary,
		string AssetName);
}
