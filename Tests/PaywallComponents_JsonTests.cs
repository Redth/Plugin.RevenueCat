using System.Text.Json;
using Plugin.RevenueCat.Models;

namespace Tests
{
	[TestClass]
	public sealed class PaywallComponents_JsonTests
	{
		[TestMethod]
		[DeploymentItem("data/paywall_offerings_response.json")]
		public void Can_Deserialize_V2_Paywall_Offerings_Response()
		{
			var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "paywall_offerings_response.json"));

			var response = json.ToPaywallOfferingsResponse();

			Assert.IsNotNull(response);
			Assert.AreEqual("default", response.CurrentOfferingId);
			Assert.AreEqual("default", response.CurrentOffering?.Identifier);
			Assert.AreEqual(1, response.Offerings.Count);

			var paywall = response.CurrentOffering?.PaywallComponents;
			Assert.IsNotNull(paywall);
			Assert.AreEqual("components", paywall.TemplateName);
			Assert.AreEqual("en_US", paywall.DefaultLocale);
			Assert.AreEqual("Go Pro", paywall.ComponentsLocalizations["en_US"]["title_lid"].Text);

			var stack = paywall.ComponentsConfig?.Base?.Stack;
			Assert.IsNotNull(stack);
			Assert.AreEqual("stack", stack.Type);
			Assert.AreEqual(3, stack.Components.Count);
			Assert.IsTrue(stack.Components[0] is PaywallTextComponent);
			Assert.IsTrue(stack.Components[1] is PaywallPackageComponent);
			Assert.IsTrue(stack.Components[2] is PaywallPurchaseButtonComponent);

			var title = (PaywallTextComponent)stack.Components[0];
			Assert.AreEqual("title_lid", title.TextLocalizationId);
			Assert.AreEqual("heading_l", title.FontSize?.GetString());

			var package = (PaywallPackageComponent)stack.Components[1];
			Assert.AreEqual("$rc_monthly", package.PackageId);
			Assert.IsTrue(package.IsSelectedByDefault);

			Assert.IsNotNull(response.UiConfig);
			Assert.IsTrue(response.UiConfig.CustomVariables.ContainsKey("player_name"));
		}

		[TestMethod]
		public void Unknown_Component_Preserves_Raw_Json_And_Deserializes_Fallback()
		{
			const string json = """
			{
			  "type": "future_component",
			  "id": "future",
			  "custom_value": 42,
			  "fallback": {
			    "type": "text",
			    "id": "fallback-text",
			    "text_lid": "fallback_lid",
			    "color": { "light": { "type": "hex", "value": "#000000ff" } }
			  }
			}
			""";

			var component = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallComponent);

			Assert.IsNotNull(component);
			Assert.IsTrue(component is PaywallUnknownComponent);

			var unknown = (PaywallUnknownComponent)component;
			Assert.AreEqual("future_component", unknown.Type);
			Assert.AreEqual(JsonValueKind.Object, unknown.Raw.ValueKind);
			Assert.IsTrue(unknown.Raw.TryGetProperty("custom_value", out var customValue));
			Assert.AreEqual(42, customValue.GetInt32());
			if (unknown.Fallback is not PaywallTextComponent fallback)
			{
				Assert.Fail("Expected the unknown component fallback to deserialize as a text component.");
				return;
			}

			Assert.AreEqual("fallback_lid", fallback.TextLocalizationId);
		}

		[TestMethod]
		public void Known_Component_Deserializes_Fallback_For_Unsupported_Rendering()
		{
			const string json = """
			{
			  "type": "timeline",
			  "id": "timeline",
			  "items": [],
			  "fallback": {
			    "type": "text",
			    "id": "timeline-fallback",
			    "text_lid": "timeline_fallback_lid",
			    "color": { "light": { "type": "hex", "value": "#000000ff" } }
			  }
			}
			""";

			var component = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallComponent);

			Assert.IsNotNull(component);
			Assert.IsTrue(component is PaywallTimelineComponent);
			if (component.Fallback is not PaywallTextComponent fallback)
			{
				Assert.Fail("Expected the known component fallback to deserialize as a text component.");
				return;
			}

			Assert.AreEqual("timeline_fallback_lid", fallback.TextLocalizationId);
		}

		[TestMethod]
		public void Components_Localizations_Allows_Empty_Array_From_RevenueCat_Fixtures()
		{
			const string json = """
			{
			  "id": "pw_empty_localizations",
			  "template_name": "components",
			  "revision": 1,
			  "components_config": {
			    "base": {
			      "stack": {
			        "type": "stack",
			        "components": []
			      }
			    }
			  },
			  "components_localizations": []
			}
			""";

			var paywall = JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PaywallComponentsData);

			Assert.IsNotNull(paywall);
			Assert.AreEqual(0, paywall.ComponentsLocalizations.Count);
		}

		[TestMethod]
		public void Sample_Paywall_Fixtures_Deserialize_Advanced_Components()
		{
			var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
			var paywallDirectory = Path.Combine(repoRoot, "sample-paywalls", "Resources", "Raw", "paywalls");
			var fixturePaths = Directory.GetFiles(paywallDirectory, "*.json");

			Assert.IsTrue(fixturePaths.Length >= 15);
			foreach (var fixturePath in fixturePaths)
			{
				var json = File.ReadAllText(fixturePath);
				var response = json.ToPaywallOfferingsResponse();
				Assert.IsNotNull(response, $"Fixture failed to parse: {Path.GetFileName(fixturePath)}");
			}

			var carousel = File.ReadAllText(Path.Combine(paywallDirectory, "carousel_onboarding.json")).ToPaywallOfferingsResponse()
				?.CurrentOffering?.PaywallComponents?.ComponentsConfig?.Base?.Stack?.Components
				.OfType<PaywallCarouselComponent>()
				.SingleOrDefault();
			Assert.IsNotNull(carousel);
			Assert.AreEqual(2, carousel.Pages.Count);
			Assert.AreEqual(18, carousel.PagePeek);

			var tabs = File.ReadAllText(Path.Combine(paywallDirectory, "tabbed_plans.json")).ToPaywallOfferingsResponse()
				?.CurrentOffering?.PaywallComponents?.ComponentsConfig?.Base?.Stack?.Components
				.OfType<PaywallTabsComponent>()
				.SingleOrDefault();
			Assert.IsNotNull(tabs);
			Assert.AreEqual("annual", tabs.DefaultTabId);
			Assert.AreEqual(2, tabs.Tabs.Count);

			var timelineComponents = File.ReadAllText(Path.Combine(paywallDirectory, "timeline_countdown.json")).ToPaywallOfferingsResponse()
				?.CurrentOffering?.PaywallComponents?.ComponentsConfig?.Base?.Stack?.Components;
			Assert.IsNotNull(timelineComponents);
			Assert.IsTrue(timelineComponents.OfType<PaywallTimelineComponent>().Any());
			Assert.IsTrue(timelineComponents.OfType<PaywallCountdownComponent>().Any());
			Assert.IsTrue(timelineComponents.OfType<PaywallVideoComponent>().Any());
		}
	}
}
