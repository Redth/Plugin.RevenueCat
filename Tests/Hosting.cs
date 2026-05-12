using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plugin.RevenueCat.Api;


namespace Tests;

[TestClass]
public static class Hosting
{
	public static IServiceProvider ServiceProvider { get; private set; } = null!;
	public static IConfigurationRoot Configuration { get; private set; } = null!;

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

		if (!string.IsNullOrEmpty(apiKeyV1))
		{
			services.AddRevenueCatApiV1(new Plugin.RevenueCat.Api.V1.RevenueCatApiV1Settings
			{
				ApiKey = apiKeyV1
			});
		}

		if (!string.IsNullOrEmpty(apiKeyV2))
		{
			services.AddRevenueCatApiV2(new Plugin.RevenueCat.Api.V2.RevenueCatApiV2Settings
			{
				ApiKey = apiKeyV2
			});
		}

		ServiceProvider = services.BuildServiceProvider();
	}

	public static IRevenueCatApiV1 RequireApiV1()
	{
		if (string.IsNullOrEmpty(Configuration["AppSettings:ApiKeyV1"]))
			Assert.Inconclusive("AppSettings:ApiKeyV1 is not configured. Set it in .env, user secrets, or environment variables to run API V1 integration tests.");

		return ServiceProvider.GetRequiredService<IRevenueCatApiV1>();
	}

	public static IRevenueCatApiV2 RequireApiV2()
	{
		if (string.IsNullOrEmpty(Configuration["AppSettings:ApiKeyV2"]))
			Assert.Inconclusive("AppSettings:ApiKeyV2 is not configured. Set it in .env, user secrets, or environment variables to run API V2 integration tests.");

		return ServiceProvider.GetRequiredService<IRevenueCatApiV2>();
	}

	public static string RequireSetting(string key)
	{
		var value = Configuration[key];
		if (string.IsNullOrEmpty(value))
			Assert.Inconclusive($"{key} must be configured to run this integration test.");

		return value;
	}

	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{
		services.Clear();
	}
}
