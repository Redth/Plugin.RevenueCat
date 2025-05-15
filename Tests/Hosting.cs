using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api;


namespace Tests;

[TestClass]
public static class Hosting
{
	public static IServiceProvider ServiceProvider { get; private set; }
	public static IConfigurationRoot Configuration { get; private set; }

	static IServiceCollection services = new ServiceCollection();

	[AssemblyInitialize]
	public static void AssemblyInitialize(TestContext _)
	{
		Configuration = new ConfigurationManager()
			.AddUserSecrets(typeof(Hosting).Assembly, true)
			.AddEnvironmentVariables()
			.Build();

		services = new ServiceCollection()
			.AddLogging();

		services.AddRevenueCatApiV1(new Plugin.RevenueCat.Api.V1.RevenueCatApiV1Settings
			{
				ApiKey = Configuration.GetValue<string>("AppSettings:ApiKeyV1")!
			});
		services.AddRevenueCatApiV2(new Plugin.RevenueCat.Api.V2.RevenueCatApiV2Settings
		{
			ApiKey = Configuration.GetValue<string>("AppSettings:ApiKeyV2")!
		});

		ServiceProvider = services.BuildServiceProvider();
	}


	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{
		services.Clear();
	}
}
