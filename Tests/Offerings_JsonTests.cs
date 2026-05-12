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
        [DataRow("type")]
        [DataRow("package_type")]
        public void Can_Deserialize_Package_Type_Aliases(string packageTypeProperty)
        {
            var json = $$"""
            {
                "id": "default",
                "identifier": "default",
                "description": "Default",
                "metadata": {
                    "experiment": "spring"
                },
                "packages": [
                    {
                        "id": "$rc_annual",
                        "identifier": "$rc_annual",
                        "{{packageTypeProperty}}": "annual",
                        "store_product": {
                            "identifier": "pro_annual",
                            "title": "Pro Annual",
                            "description": "Annual access",
                            "price_string": "$19.99",
                            "currency_code": "USD",
                            "product_type": "subscription"
                        }
                    }
                ]
            }
            """;

            var offering = json.ToOffering();

            Assert.IsNotNull(offering);
            Assert.AreEqual("spring", offering.Metadata["experiment"].GetString());
            Assert.AreEqual(PackageType.Annual, offering.Packages[0].PackageType);
        }

        [TestMethod]
        public void Can_Deserialize_Offerings_Collection()
        {
            const string json = """
            {
                "current": {
                    "id": "default",
                    "identifier": "default",
                    "description": "Default",
                    "packages": []
                },
                "all": {
                    "default": {
                        "id": "default",
                        "identifier": "default",
                        "description": "Default",
                        "packages": []
                    }
                },
                "offerings": [
                    {
                        "id": "default",
                        "identifier": "default",
                        "description": "Default",
                        "packages": []
                    }
                ]
            }
            """;

            var offerings = json.ToOfferings();

            Assert.IsNotNull(offerings);
            Assert.IsNotNull(offerings.Current);
            Assert.AreEqual(1, offerings.All.Count);
            Assert.AreEqual(1, offerings.Items.Count);
        }

        [TestMethod]
        public void Can_Deserialize_Purchase_Result()
        {
            const string json = """
            {
                "user_cancelled": false,
                "store_transaction": {
                    "transaction_identifier": "txn_123",
                    "product_identifier": "pro_annual",
                    "product_identifiers": ["pro_annual"],
                    "purchase_date": "2024-01-01T00:00:00Z",
                    "quantity": 1,
                    "store": "app_store"
                },
                "customer_info": {
                    "request_date": "2024-01-01T00:00:01Z",
                    "subscriber": {
                        "entitlements": {},
                        "subscriptions": {},
                        "non_subscriptions": {}
                    }
                }
            }
            """;

            var result = json.ToPurchaseResult();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.UserCancelled);
            Assert.AreEqual("txn_123", result.StoreTransaction?.TransactionIdentifier);
            Assert.AreEqual("pro_annual", result.StoreTransaction?.ProductIdentifiers[0]);
            Assert.IsNotNull(result.CustomerInfo);
        }
    }
}
