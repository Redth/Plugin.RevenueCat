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
    }
}