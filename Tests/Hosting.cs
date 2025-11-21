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
		// Load .env file by walking up the directory tree from the assembly location
		var assemblyDir = Path.GetDirectoryName(typeof(Hosting).Assembly.Location);
		var currentDir = assemblyDir;
		
		while (currentDir != null)
		{
			// Check in current directory
			var envPath = Path.Combine(currentDir, ".env");
			if (File.Exists(envPath))
			{
				DotNetEnv.Env.Load(envPath);
				break;
			}
			
			// Also check in Tests subdirectory if we're at the repo root
			var testsEnvPath = Path.Combine(currentDir, "Tests", ".env");
			if (File.Exists(testsEnvPath))
			{
				DotNetEnv.Env.Load(testsEnvPath);
				break;
			}
			
			currentDir = Directory.GetParent(currentDir)?.FullName;
		}

		// Build configuration with environment variables
		// Note: Environment variables use __ as separator, which maps to : in configuration
		Configuration = new ConfigurationManager()
			.AddEnvironmentVariables()
			.AddUserSecrets(typeof(Hosting).Assembly, true)
			.Build();

		services = new ServiceCollection()
			.AddLogging();

		var apiKeyV1 = Configuration["AppSettings:ApiKeyV1"];
		var apiKeyV2 = Configuration["AppSettings:ApiKeyV2"];

		if (string.IsNullOrEmpty(apiKeyV1))
			throw new InvalidOperationException("AppSettings:ApiKeyV1 is not configured. Please set it in .env file, user secrets, or environment variables.");
		
		if (string.IsNullOrEmpty(apiKeyV2))
			throw new InvalidOperationException("AppSettings:ApiKeyV2 is not configured. Please set it in .env file, user secrets, or environment variables.");

		services.AddRevenueCatApiV1(new Plugin.RevenueCat.Api.V1.RevenueCatApiV1Settings
			{
				ApiKey = apiKeyV1
			});
		services.AddRevenueCatApiV2(new Plugin.RevenueCat.Api.V2.RevenueCatApiV2Settings
		{
			ApiKey = apiKeyV2
		});

		ServiceProvider = services.BuildServiceProvider();
	}


	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{
		services.Clear();
	}
}
