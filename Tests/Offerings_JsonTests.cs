using Plugin.RevenueCat.Models;

namespace Tests
{
    [TestClass]
    public sealed class Offerings_JsonTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        //[DeploymentItem("data/offerings_json_android.json")]
        [DeploymentItem("data/offering_json_ios.json")]
        //[DataRow("android")]
        [DataRow("ios")]
        public void Can_Deserialize_Single_Json(string platform)
        {
            // Simulate creating a log file for this test
            var platformFile = $"offering_json_{platform}.json";
            var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", platformFile));

            var c  = json.ToModel<Offering>();

            Assert.IsNotNull(c);
        }

        [TestMethod]
        public void Can_Deserialize_Single_Offering_With_Paywall_Fields()
        {
            const string json = """
            {
              "id": "default",
              "identifier": "default",
              "description": "Default offering",
              "metadata": {
                "campaign": "spring"
              },
              "packages": [],
              "web_checkout_url": "https://checkout.revenuecat.com/example",
              "ui_config": {
                "app": {
                  "colors": {},
                  "fonts": {}
                },
                "localizations": {},
                "variable_config": {
                  "variable_compatibility_map": {},
                  "function_compatibility_map": {}
                },
                "custom_variables": {}
              },
              "paywall_components": {
                "id": "pw1",
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
                "components_localizations": {},
                "default_locale": "en_US"
              }
            }
            """;

            var offering = json.ToOffering();

            Assert.IsNotNull(offering);
            Assert.AreEqual("spring", offering.Metadata["campaign"].GetString());
            Assert.AreEqual("https://checkout.revenuecat.com/example", offering.WebCheckoutUrl);
            Assert.IsNotNull(offering.UiConfig);
            Assert.IsNotNull(offering.PaywallComponents);
            Assert.AreEqual("pw1", offering.PaywallComponents.Id);
        }
    }
}
