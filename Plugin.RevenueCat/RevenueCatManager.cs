using System.Text.Json;
using Microsoft.Extensions.Logging;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public class CustomerInfoUpdatedEventArgs(CustomerInfo customerInfoRequest) : EventArgs
{
	public CustomerInfo CustomerInfoRequest => customerInfoRequest;
}

public class RevenueCatManager : IRevenueCatManager
{
	public RevenueCatManager(RevenueCatOptions options, IRevenueCatPlatformImplementation platformImplementation, ILoggerFactory? loggerFactory = null)
	{
		Options = options;
		PlatformImplementation = platformImplementation;
		Logger = loggerFactory?.CreateLogger<RevenueCatManager>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RevenueCatManager>.Instance;
	}

	public readonly IRevenueCatPlatformImplementation PlatformImplementation;
	
	public readonly RevenueCatOptions Options;
	
	public event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;
	
	public string? ApiKey => PlatformImplementation.ApiKey;

	protected readonly ILogger Logger;
	
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
				Options.CustomerInfoUpdatedCallback?.Invoke(customerInfoRequest);
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
	
	public Task<CustomerInfo?> SyncPurchasesAsync()
		=> Request<CustomerInfo>(nameof(SyncPurchasesAsync), PlatformImplementation.SyncPurchasesAsync);

	public Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
		=> Request<CustomerInfo>(nameof(PurchaseAsync), () => PlatformImplementation.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier));

	public Task SyncOfferingsAndAttributesIfNeeded()
		=> PlatformImplementation.SyncOfferingsAndAttributesIfNeeded();

	public void SetEmail(string email)
		=> PlatformImplementation.SetEmail(email);

	public void SetDisplayName(string displayName)
		=> PlatformImplementation.SetDisplayName(displayName);

	public void SetAd(string ad)
		=> PlatformImplementation.SetAd(ad);

	public void SetAdGroup(string adGroup)
		=> PlatformImplementation.SetAdGroup(adGroup);

	public void SetCampaign(string campaign)
		=> PlatformImplementation.SetCampaign(campaign);

	public void SetCreative(string creative)
		=> PlatformImplementation.SetCreative(creative);

	public void SetKeyword(string keyword)
		=> PlatformImplementation.SetKeyword(keyword);

	public void SetAttribute(string key, string? value)
		=> PlatformImplementation.SetAttribute(key, value);

	public void SetAttributes(IDictionary<string, string> attributes)
		=> PlatformImplementation.SetAttributes(attributes);

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

	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Uses source generated context.")]
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Uses source generated context.")]
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

