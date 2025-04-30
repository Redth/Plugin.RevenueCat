using Plugin.RevenueCat.Models;

namespace Tests
{
	[TestClass]
	public sealed class CustomerInfo_JsonTests
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		[DeploymentItem("data/custoimerinfo_json_android.json")]
		public void Can_Deserialize_Android_Json()
		{

			// Simulate creating a log file for this test
			var jsonFile = Path.Combine(AppContext.BaseDirectory, "data", "customerinfo_json_android.json");
			
			var json = File.ReadAllText(jsonFile);

			var c = CustomerInfoRequest.FromJson(json);

			Assert.IsNotNull(c);
		}
	}
}
