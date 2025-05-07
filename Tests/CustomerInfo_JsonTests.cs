using Plugin.RevenueCat.Models;

namespace Tests
{
	[TestClass]
	public sealed class CustomerInfo_JsonTests
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		[DeploymentItem("data/custoimerinfo_json_android.json")]
		[DeploymentItem("data/custoimerinfo_json_ios.json")]
		[DataRow("android")]
		[DataRow("ios")]
		public void Can_Deserialize_Json(string platform)
		{
			// Simulate creating a log file for this test
			var platformFile = $"customerinfo_json_{platform}.json";
			var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", platformFile));

			var c  = json.ToModel<CustomerInfoRequest>();

			Assert.IsNotNull(c);
		}
	}
}
