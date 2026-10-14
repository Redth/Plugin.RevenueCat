using Plugin.RevenueCat.Models;
using Plugin.RevenueCat.Paywalls;

namespace Tests;

[TestClass]
public sealed class PaywallRenderingTests
{
	[TestMethod]
	public void Localization_Uses_Language_And_Default_Fallback_Per_Key()
	{
		var data = new PaywallComponentsData
		{
			DefaultLocale = "en-US",
			ComponentsLocalizations =
			{
				["de"] = new() { ["title"] = new() { Text = "Deutsch" } },
				["fr"] = new() { ["title"] = new() { Text = "Francais" } },
				["en_US"] = new() { ["title"] = new() { Text = "English" }, ["footer"] = new() { Text = "Continue" } }
			}
		};
		Assert.AreEqual("Francais", PaywallLocalizationResolver.ResolveText(data, "FR-fr", "title"));
		Assert.AreEqual("Continue", PaywallLocalizationResolver.ResolveText(data, "fr", "footer"));
		Assert.AreEqual("English", PaywallLocalizationResolver.ResolveText(data, "ja", "title"));
	}

	[TestMethod]
	public void Localization_Resolver_Falls_Back_To_Default_Locale()
	{
		var data = new PaywallComponentsData
		{
			DefaultLocale = "en_US",
			ComponentsLocalizations =
			{
				["en_US"] = new Dictionary<string, PaywallLocalizationData>
				{
					["title"] = new() { Text = "Go Pro" }
				}
			}
		};

		var text = PaywallLocalizationResolver.ResolveText(data, "fr_FR", "title");

		Assert.AreEqual("Go Pro", text);
	}

	[TestMethod]
	public void Text_Processor_Replaces_Supported_Product_Variables()
	{
		var package = new Package
		{
			Identifier = "$rc_monthly",
			StoreProduct = new StoreProduct
			{
				Title = "Pro Monthly",
				PriceString = "$4.99",
				SubscriptionPeriod = new SubscriptionPeriod
				{
					Value = 1,
					Unit = SubscriptionPeriodUnit.Month
				}
			}
		};

		var text = PaywallTextProcessor.ProcessVariables(
			"Start {{ product_name }} for {{ price }} / {{ sub_period }}",
			new DefaultPaywallVariableProvider(),
			new PaywallVariableContext
			{
				Package = package
			});

		Assert.AreEqual("Start Pro Monthly for $4.99 / month", text);
	}

	[TestMethod]
	public void Render_Request_Prefers_Paywall_Data_Component_Config()
	{
		var fromPaywallData = new PaywallComponentsConfig();
		var fromRequest = new PaywallComponentsConfig();
		var request = new PaywallRenderRequest
		{
			PaywallData = new PaywallComponentsData
			{
				ComponentsConfig = fromPaywallData
			},
			ComponentsConfig = fromRequest
		};

		Assert.AreSame(fromPaywallData, request.GetComponentsConfig());
	}
}
