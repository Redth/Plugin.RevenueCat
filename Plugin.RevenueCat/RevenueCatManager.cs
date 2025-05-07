using System.Text.Json;
using Microsoft.Extensions.Logging;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public class CustomerInfoUpdatedEventArgs(CustomerInfoRequest customerInfoRequest) : EventArgs
{
	public CustomerInfoRequest CustomerInfoRequest => customerInfoRequest;
}

public class RevenueCatManager(IRevenueCatImpl revenueCatImpl, ILogger? logger = null) : IRevenueCatManager
{
	public event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	public string ApiKey { get;  private set; } = string.Empty;

	ILogger Logger => logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
	
	public void Initialize(string apiKey, bool debugLog = false, string? appStore = null, string? userId = null, Action<CustomerInfoRequest>? customerInfoUpdatedCallback = null)
	{
		ApiKey = apiKey;
		
		Logger.LogInformation($"RevenueCat->{nameof(Initialize)}: Initializing...");
		
		revenueCatImpl.SetCustomerInfoUpdatedHandler((json) =>
		{
			Logger.LogInformation($"RevenueCat->{nameof(CustomerInfoUpdated)}: Deserializing JSON...");
			
			var customerInfoRequest = ParseJson<CustomerInfoRequest>(nameof(Initialize), json);
			
			if (customerInfoRequest is not null && customerInfoUpdatedCallback is not null)
			{
				Logger.LogInformation($"RevenueCat->{nameof(CustomerInfoUpdated)}: Callback...");
				
				// Call callback first
				customerInfoUpdatedCallback.Invoke(customerInfoRequest);
				// Raise event too
				CustomerInfoUpdated?.Invoke(this, new CustomerInfoUpdatedEventArgs(customerInfoRequest));
			}
		});

		try
		{
			revenueCatImpl.Initialize(apiKey, debugLog, appStore, userId);
			Logger.LogInformation($"RevenueCat->{nameof(Initialize)}: Initialized.");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, $"RevenueCatManager->{nameof(Initialize)}: Request failed.");
		}
	}

	public Task<CustomerInfoRequest?> LoginAsync(string userId)
		=> Request<CustomerInfoRequest>(nameof(LoginAsync), () => revenueCatImpl.LoginAsync(userId));

	public Task<CustomerInfoRequest?> GetCustomerInfoAsync(bool force)
		=> Request<CustomerInfoRequest>(nameof(GetCustomerInfoAsync), () => revenueCatImpl.GetCustomerInfoAsync(force));
	
	public Task<Offering?> GetOfferingAsync(string offeringIdentifier)
		=> Request<Offering>(nameof(GetOfferingAsync), () => revenueCatImpl.GetOfferingAsync(offeringIdentifier));

	public Task<CustomerInfoRequest?> RestoreAsync()
		=> Request<CustomerInfoRequest>(nameof(RestoreAsync), revenueCatImpl.RestoreAsync);

	public Task<CustomerInfoRequest?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
		=> Request<CustomerInfoRequest>(nameof(PurchaseAsync), () => revenueCatImpl.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier));

	async Task<TObject?> Request<TObject>(string name, Func<Task<string?>> requestFunc)
	{
		Logger.LogInformation("RevenueCatManager->{Name}: Starting request...", name);

		string? json;

		try
		{
			json = await requestFunc();
			
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

