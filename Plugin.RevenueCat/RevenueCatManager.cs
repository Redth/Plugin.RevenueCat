using System.Text.Json;
using Microsoft.Extensions.Logging;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public class CustomerInfoUpdatedEventArgs(CustomerInfo customerInfo) : EventArgs
{
	public CustomerInfo CustomerInfo => customerInfoRequest;
}

public class RevenueCatManager : IRevenueCatManager
{
	public RevenueCatManager(RevenueCatOptions options, IRevenueCatPlatformImplementation platformImplementation, IServiceProvider serviceProvider, ILoggerFactory? loggerFactory = null)
	{
		Options = options;
		PlatformImplementation = platformImplementation;
		ServiceProvider = serviceProvider;
		Logger = loggerFactory?.CreateLogger<RevenueCatManager>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RevenueCatManager>.Instance;
	}

	public readonly IRevenueCatPlatformImplementation PlatformImplementation;
	
	public readonly RevenueCatOptions Options;
	
	public event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;
	
	public string? ApiKey => PlatformImplementation.ApiKey;

	protected readonly ILogger Logger;
	protected readonly IServiceProvider ServiceProvider;
	
	public void Initialize()
	{
		Logger.LogInformation($"RevenueCat->{nameof(Initialize)}: Initializing...");

		PlatformImplementation.SetCustomerInfoUpdatedHandler((json) =>
		{
			if (Options.Debug)
				Logger.LogInformation($"RevenueCat->{nameof(CustomerInfoUpdated)}: Received JSON: {json}");

			Logger.LogInformation($"RevenueCat->{nameof(CustomerInfoUpdated)}: Deserializing JSON...");

			var customerInfoRequest = ParseJson<CustomerInfo>(nameof(Initialize), json);

			if (customerInfoRequest is not null)
			{
				Logger.LogInformation($"RevenueCat->{nameof(CustomerInfoUpdated)}: Callback...");

				// Call callback first
				Options.CustomerInfoUpdatedCallback?.Invoke(ServiceProvider, customerInfoRequest);
				// Raise event too
				CustomerInfoUpdated?.Invoke(this, new CustomerInfoUpdatedEventArgs(customerInfoRequest));
			}
		});

		try
		{
			PlatformImplementation.Initialize(Options);
			Logger.LogInformation($"RevenueCat->{nameof(Initialize)}: Initialized.");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, $"RevenueCatManager->{nameof(Initialize)}: Request failed.");
		}
	}

	public Task<CustomerInfo?> LoginAsync(string userId)
		=> Request<CustomerInfo>(nameof(LoginAsync), () => PlatformImplementation.LoginAsync(userId));

	public Task<CustomerInfo?> GetCustomerInfoAsync(bool force)
		=> Request<CustomerInfo>(nameof(GetCustomerInfoAsync), () => PlatformImplementation.GetCustomerInfoAsync(force));
	
	public Task<Offering?> GetOfferingAsync(string offeringIdentifier)
		=> Request<Offering>(nameof(GetOfferingAsync), () => PlatformImplementation.GetOfferingAsync(offeringIdentifier));

	public Task<CustomerInfo?> RestoreAsync()
		=> Request<CustomerInfo>(nameof(RestoreAsync), PlatformImplementation.RestoreAsync);

	public Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
		=> Request<CustomerInfo>(nameof(PurchaseAsync), () => PlatformImplementation.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier));

	async Task<TObject?> Request<TObject>(string name, Func<Task<string?>> requestFunc)
	{
		Logger.LogInformation("RevenueCatManager->{Name}: Starting request...", name);

		string? json;

		try
		{
			json = await requestFunc();

			if (Options.Debug)
				Logger.LogInformation("RevenueCat->{Name}: Received JSON: {json}", name, json);

			Logger.LogInformation("RevenueCatManager->{Name}: Received json response: {JsonLength}.", name, json?.Length);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "RevenueCatManager->{Name}: Request failed.", name);
			return default;
		}

		var obj = ParseJson<TObject>(name, json);

		Logger.LogInformation("RevenueCatManager->{Name} Request Complete.", name);

		return obj;
	}

	TObject? ParseJson<TObject>(string name, string? json)
	{
		if (string.IsNullOrEmpty(json))
		{
			Logger.LogWarning("RevenueCatManager->{Name}: JSON response is null or empty.", name);
			return default;
		}
		
		TObject? obj = default;
		
		try
		{
			obj = JsonSerializer.Deserialize<TObject>(json, ModelExtensions.Settings);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "RevenueCatManager->{Name}: Error parsing JSON response.", name);
		}

		return obj;
	}
}

