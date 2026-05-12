﻿using System.Text.Json;
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

	public string? AppUserId => PlatformImplementation.AppUserId;

	public bool IsAnonymous => PlatformImplementation.IsAnonymous;

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
		=> GetValue(LoginWithResultAsync(userId));

	public Task<RevenueCatOperationResult<CustomerInfo>> LoginWithResultAsync(string userId)
		=> RequestResult<CustomerInfo>(nameof(LoginAsync), () => PlatformImplementation.LoginAsync(userId));

	public Task<CustomerInfo?> LogOutAsync()
		=> GetValue(LogOutWithResultAsync());

	public Task<RevenueCatOperationResult<CustomerInfo>> LogOutWithResultAsync()
		=> RequestResult<CustomerInfo>(nameof(LogOutAsync), PlatformImplementation.LogOutAsync);

	public Task<CustomerInfo?> GetCustomerInfoAsync(bool force)
		=> GetValue(GetCustomerInfoWithResultAsync(force));

	public Task<RevenueCatOperationResult<CustomerInfo>> GetCustomerInfoWithResultAsync(bool force)
		=> RequestResult<CustomerInfo>(nameof(GetCustomerInfoAsync), () => PlatformImplementation.GetCustomerInfoAsync(force));

	public void InvalidateCustomerInfoCache()
		=> PlatformImplementation.InvalidateCustomerInfoCache();
	
	public Task<Offering?> GetOfferingAsync(string offeringIdentifier)
	{
		Logger.LogInformation("RevenueCatManager->{Name}: Requesting offering '{OfferingIdentifier}'.", nameof(GetOfferingAsync), offeringIdentifier);
		return GetValue(GetOfferingWithResultAsync(offeringIdentifier));
	}

	public Task<RevenueCatOperationResult<Offering>> GetOfferingWithResultAsync(string offeringIdentifier)
		=> RequestResult<Offering>(nameof(GetOfferingAsync), () => PlatformImplementation.GetOfferingAsync(offeringIdentifier));

	public Task<Offerings?> GetOfferingsAsync()
		=> GetValue(GetOfferingsWithResultAsync());

	public Task<RevenueCatOperationResult<Offerings>> GetOfferingsWithResultAsync()
		=> RequestResult<Offerings>(nameof(GetOfferingsAsync), PlatformImplementation.GetOfferingsAsync);

	public async Task<Offering?> GetCurrentOfferingAsync()
		=> (await GetOfferingsAsync().ConfigureAwait(false))?.Current;

	public async Task<RevenueCatOperationResult<Offering>> GetCurrentOfferingWithResultAsync()
	{
		var offerings = await GetOfferingsWithResultAsync().ConfigureAwait(false);
		return offerings.IsSuccess
			? RevenueCatOperationResult<Offering>.Success(offerings.Value?.Current)
			: RevenueCatOperationResult<Offering>.Failure(offerings.Error ?? UnknownError(nameof(GetCurrentOfferingAsync)));
	}

	public Task<Offering?> GetOfferingForPlacementAsync(string placementIdentifier)
		=> GetValue(GetOfferingForPlacementWithResultAsync(placementIdentifier));

	public Task<RevenueCatOperationResult<Offering>> GetOfferingForPlacementWithResultAsync(string placementIdentifier)
		=> RequestResult<Offering>(nameof(GetOfferingForPlacementAsync), () => PlatformImplementation.GetOfferingForPlacementAsync(placementIdentifier));

	public async Task<IReadOnlyList<StoreProduct>> GetProductsAsync(IEnumerable<string> productIdentifiers, RevenueCatProductType? type = null)
		=> (await GetProductsWithResultAsync(productIdentifiers, type).ConfigureAwait(false)).Value ?? Array.Empty<StoreProduct>();

	public async Task<RevenueCatOperationResult<IReadOnlyList<StoreProduct>>> GetProductsWithResultAsync(IEnumerable<string> productIdentifiers, RevenueCatProductType? type = null)
	{
		var productIds = productIdentifiers?.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).Distinct().ToArray()
			?? Array.Empty<string>();

		if (productIds.Length == 0)
			return RevenueCatOperationResult<IReadOnlyList<StoreProduct>>.Success(Array.Empty<StoreProduct>());

		var products = await RequestResult<List<StoreProduct>>(
			nameof(GetProductsAsync),
			() => PlatformImplementation.GetProductsAsync(string.Join(",", productIds), ToNativeProductType(type))).ConfigureAwait(false);

		return products.IsSuccess
			? RevenueCatOperationResult<IReadOnlyList<StoreProduct>>.Success(products.Value is null ? Array.Empty<StoreProduct>() : products.Value)
			: RevenueCatOperationResult<IReadOnlyList<StoreProduct>>.Failure(products.Error ?? UnknownError(nameof(GetProductsAsync)));
	}

	public Task<CustomerInfo?> RestoreAsync()
		=> GetValue(RestoreWithResultAsync());

	public Task<RevenueCatOperationResult<CustomerInfo>> RestoreWithResultAsync()
		=> RequestResult<CustomerInfo>(nameof(RestoreAsync), PlatformImplementation.RestoreAsync);
	
	public Task<CustomerInfo?> SyncPurchasesAsync()
		=> GetValue(SyncPurchasesWithResultAsync());

	public Task<RevenueCatOperationResult<CustomerInfo>> SyncPurchasesWithResultAsync()
		=> RequestResult<CustomerInfo>(nameof(SyncPurchasesAsync), PlatformImplementation.SyncPurchasesAsync);

	public Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
		=> GetValue(RequestResult<CustomerInfo>(nameof(PurchaseAsync), () => PlatformImplementation.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier)));

	public Task<PurchaseResult?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
		=> PurchaseWithResultAsync(platformContext, offeringIdentifier, packageIdentifier, null);

	public Task<PurchaseResult?> PurchaseWithResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, PurchaseOptions? options)
		=> GetValue(PurchaseWithOperationResultAsync(platformContext, offeringIdentifier, packageIdentifier, options));

	public Task<RevenueCatOperationResult<PurchaseResult>> PurchaseWithOperationResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
		=> PurchaseWithOperationResultAsync(platformContext, offeringIdentifier, packageIdentifier, null);

	public Task<RevenueCatOperationResult<PurchaseResult>> PurchaseWithOperationResultAsync(object? platformContext, string offeringIdentifier, string packageIdentifier, PurchaseOptions? options)
		=> RequestResult<PurchaseResult>(nameof(PurchaseWithResultAsync), () => PlatformImplementation.PurchaseWithResultAsync(platformContext, offeringIdentifier, packageIdentifier, ToPurchaseOptionsJson(options)));

	public Task<PurchaseResult?> PurchaseProductAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type = null)
		=> PurchaseProductAsync(platformContext, productIdentifier, type, null);

	public Task<PurchaseResult?> PurchaseProductAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type, PurchaseOptions? options)
		=> GetValue(PurchaseProductWithOperationResultAsync(platformContext, productIdentifier, type, options));

	public Task<RevenueCatOperationResult<PurchaseResult>> PurchaseProductWithOperationResultAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type = null)
		=> PurchaseProductWithOperationResultAsync(platformContext, productIdentifier, type, null);

	public Task<RevenueCatOperationResult<PurchaseResult>> PurchaseProductWithOperationResultAsync(object? platformContext, string productIdentifier, RevenueCatProductType? type, PurchaseOptions? options)
		=> RequestResult<PurchaseResult>(nameof(PurchaseProductAsync), () => PlatformImplementation.PurchaseProductAsync(platformContext, productIdentifier, ToNativeProductType(type), ToPurchaseOptionsJson(options)));

	public Task<PurchaseResult?> PurchaseSubscriptionOptionAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, RevenueCatProductType? type = null, PurchaseOptions? options = null)
		=> GetValue(PurchaseSubscriptionOptionWithOperationResultAsync(platformContext, productIdentifier, subscriptionOptionIdentifier, type, options));

	public Task<RevenueCatOperationResult<PurchaseResult>> PurchaseSubscriptionOptionWithOperationResultAsync(object? platformContext, string productIdentifier, string subscriptionOptionIdentifier, RevenueCatProductType? type = null, PurchaseOptions? options = null)
	{
		var effectiveOptions = options is null
			? new PurchaseOptions { SubscriptionOptionId = subscriptionOptionIdentifier }
			: new PurchaseOptions
			{
				SubscriptionOptionId = subscriptionOptionIdentifier,
				OldProductIdentifier = options.OldProductIdentifier,
				ReplacementMode = options.ReplacementMode,
				IsPersonalizedPrice = options.IsPersonalizedPrice,
				PresentedOfferingIdentifier = options.PresentedOfferingIdentifier,
				StoreProductDiscountIdentifier = options.StoreProductDiscountIdentifier,
				DiscountIdentifier = options.DiscountIdentifier,
				ExtensionData = options.ExtensionData
			};

		return RequestResult<PurchaseResult>(
			nameof(PurchaseSubscriptionOptionAsync),
			() => PlatformImplementation.PurchaseSubscriptionOptionAsync(platformContext, productIdentifier, subscriptionOptionIdentifier, ToNativeProductType(type), ToPurchaseOptionsJson(effectiveOptions)));
	}

	public async Task<bool> CanMakePaymentsAsync(object? platformContext = null)
		=> (await CanMakePaymentsWithResultAsync(platformContext).ConfigureAwait(false)).Value == true;

	public Task<RevenueCatOperationResult<bool>> CanMakePaymentsWithResultAsync(object? platformContext = null)
		=> BoolResult(nameof(CanMakePaymentsAsync), () => PlatformImplementation.CanMakePaymentsAsync(platformContext));

	public Task<string?> GetStorefrontAsync()
		=> GetValue(GetStorefrontWithResultAsync());

	public Task<RevenueCatOperationResult<string>> GetStorefrontWithResultAsync()
		=> StringResult(nameof(GetStorefrontAsync), PlatformImplementation.GetStorefrontAsync);

	public Task<VirtualCurrencies?> GetVirtualCurrenciesAsync()
		=> GetValue(GetVirtualCurrenciesWithResultAsync());

	public Task<RevenueCatOperationResult<VirtualCurrencies>> GetVirtualCurrenciesWithResultAsync()
		=> RequestResult<VirtualCurrencies>(nameof(GetVirtualCurrenciesAsync), PlatformImplementation.GetVirtualCurrenciesAsync);

	public void InvalidateVirtualCurrenciesCache()
		=> PlatformImplementation.InvalidateVirtualCurrenciesCache();

	public Task<WebPurchaseRedemptionResult?> RedeemWebPurchaseAsync(string redemptionLink)
		=> GetValue(RedeemWebPurchaseWithResultAsync(redemptionLink));

	public Task<RevenueCatOperationResult<WebPurchaseRedemptionResult>> RedeemWebPurchaseWithResultAsync(string redemptionLink)
		=> RequestResult<WebPurchaseRedemptionResult>(nameof(RedeemWebPurchaseAsync), () => PlatformImplementation.RedeemWebPurchaseAsync(redemptionLink));

	public Task<string?> GetAmazonLwaConsentStatusAsync()
		=> GetValue(GetAmazonLwaConsentStatusWithResultAsync());

	public Task<RevenueCatOperationResult<string>> GetAmazonLwaConsentStatusWithResultAsync()
		=> StringResult(nameof(GetAmazonLwaConsentStatusAsync), PlatformImplementation.GetAmazonLwaConsentStatusAsync);

	public Task SyncOfferingsAndAttributesIfNeeded()
		=> SyncOfferingsAndAttributesIfNeededWithResult();

	public async Task<RevenueCatOperationResult<bool>> SyncOfferingsAndAttributesIfNeededWithResult()
		=> await BoolResult(nameof(SyncOfferingsAndAttributesIfNeeded), async () =>
		{
			await PlatformImplementation.SyncOfferingsAndAttributesIfNeeded().ConfigureAwait(false);
			return true;
		}).ConfigureAwait(false);

	public void CollectDeviceIdentifiers()
		=> PlatformImplementation.CollectDeviceIdentifiers();

	public void SetEmail(string email)
		=> PlatformImplementation.SetEmail(email);

	public void SetPhoneNumber(string? phoneNumber)
		=> PlatformImplementation.SetPhoneNumber(phoneNumber);

	public void SetPushToken(string? pushToken)
		=> PlatformImplementation.SetPushToken(pushToken);

	public void SetDisplayName(string displayName)
		=> PlatformImplementation.SetDisplayName(displayName);

	public void SetMediaSource(string? mediaSource)
		=> PlatformImplementation.SetMediaSource(mediaSource);

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

	static string? ToNativeProductType(RevenueCatProductType? type)
		=> type switch
		{
			RevenueCatProductType.Subscription => "subs",
			RevenueCatProductType.InApp => "inapp",
			RevenueCatProductType.Consumable => "inapp",
			RevenueCatProductType.NonConsumable => "inapp",
			RevenueCatProductType.NonRenewingSubscription => "inapp",
			RevenueCatProductType.AutoRenewableSubscription => "subs",
			_ => null
		};

	static string? ToPurchaseOptionsJson(PurchaseOptions? options)
		=> options is null
			? null
			: JsonSerializer.Serialize(options, ModelSerializerContext.Default.PurchaseOptions);

	static async Task<TValue?> GetValue<TValue>(Task<RevenueCatOperationResult<TValue>> resultTask)
		=> (await resultTask.ConfigureAwait(false)).Value;

	async Task<RevenueCatOperationResult<bool>> BoolResult(string name, Func<Task<bool>> requestFunc)
	{
		try
		{
			return RevenueCatOperationResult<bool>.Success(await requestFunc().ConfigureAwait(false));
		}
		catch (Exception ex)
		{
			var error = RevenueCatError.FromException(ex);
			Logger.LogError(ex, "RevenueCatManager->{Name}: Request failed. {ErrorCode}: {ErrorMessage}", name, error.Code, error.Message);
			return RevenueCatOperationResult<bool>.Failure(error);
		}
	}

	async Task<RevenueCatOperationResult<string>> StringResult(string name, Func<Task<string?>> requestFunc)
	{
		try
		{
			return RevenueCatOperationResult<string>.Success(await requestFunc().ConfigureAwait(false));
		}
		catch (Exception ex)
		{
			var error = RevenueCatError.FromException(ex);
			Logger.LogError(ex, "RevenueCatManager->{Name}: Request failed. {ErrorCode}: {ErrorMessage}", name, error.Code, error.Message);
			return RevenueCatOperationResult<string>.Failure(error);
		}
	}

	async Task<RevenueCatOperationResult<TObject>> RequestResult<TObject>(string name, Func<Task<string?>> requestFunc)
	{
		Logger.LogInformation("RevenueCatManager->{Name}: Starting request...", name);

		string? json;

		try
		{
			json = await requestFunc().ConfigureAwait(false);

			if (Options.Debug)
				Logger.LogInformation("RevenueCat->{Name}: Received JSON: {json}", name, json);

			Logger.LogInformation("RevenueCatManager->{Name}: Received json response: {JsonLength}.", name, json?.Length);
		}
		catch (Exception ex)
		{
			var error = RevenueCatError.FromException(ex);
			Logger.LogError(ex, "RevenueCatManager->{Name}: Request failed. {ErrorCode}: {ErrorMessage}", name, error.Code, error.Message);
			return RevenueCatOperationResult<TObject>.Failure(error);
		}

		var result = ParseJsonResult<TObject>(name, json);

		Logger.LogInformation("RevenueCatManager->{Name} Request Complete.", name);

		if (result.IsSuccess &&
			result.Value is PurchaseResult { UserCancelled: true } purchaseResult)
		{
			return RevenueCatOperationResult<TObject>.Cancelled(
				(TObject)(object)purchaseResult,
				RevenueCatError.Cancelled());
		}

		return result;
	}

	TObject? ParseJson<TObject>(string name, string? json)
		=> ParseJsonResult<TObject>(name, json).Value;

	RevenueCatOperationResult<TObject> ParseJsonResult<TObject>(string name, string? json)
	{
		if (string.IsNullOrEmpty(json))
		{
			var message = "JSON response is null or empty.";
			if (typeof(TObject) == typeof(Offering))
			{
				message += " Check that the requested offering exists or that a current/default offering is configured in RevenueCat.";
			}

			Logger.LogWarning("RevenueCatManager->{Name}: {Message}", name, message);
			return RevenueCatOperationResult<TObject>.Failure(new RevenueCatError
			{
				Code = "empty_response",
				Message = message,
				Source = "dotnet"
			});
		}
		
		TObject? obj = default;
		
		try
		{
			// Use source-generated context for AOT compatibility
			if (typeof(TObject) == typeof(CustomerInfo))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.CustomerInfo)!;
			}
			else if (typeof(TObject) == typeof(Offering))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.Offering)!;
			}
			else if (typeof(TObject) == typeof(Offerings))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.Offerings)!;
			}
			else if (typeof(TObject) == typeof(List<StoreProduct>))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.ListStoreProduct)!;
			}
			else if (typeof(TObject) == typeof(PurchaseResult))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.PurchaseResult)!;
			}
			else if (typeof(TObject) == typeof(VirtualCurrencies))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.VirtualCurrencies)!;
			}
			else if (typeof(TObject) == typeof(WebPurchaseRedemptionResult))
			{
				obj = (TObject)(object)JsonSerializer.Deserialize(json, ModelSerializerContext.Default.WebPurchaseRedemptionResult)!;
			}
			else
			{
				throw new NotSupportedException($"Type {typeof(TObject).Name} is not supported for AOT deserialization. Add it to ModelSerializerContext.");
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "RevenueCatManager->{Name}: Error parsing JSON response.", name);
			return RevenueCatOperationResult<TObject>.Failure(new RevenueCatError
			{
				Code = "invalid_json",
				Message = ex.Message,
				Source = "dotnet",
				ExceptionType = ex.GetType().FullName
			});
		}

		return RevenueCatOperationResult<TObject>.Success(obj);
	}

	static RevenueCatError UnknownError(string operation)
		=> new()
		{
			Code = "unknown_error",
			Message = $"RevenueCat operation '{operation}' failed without error details.",
			Source = "dotnet"
		};
}
