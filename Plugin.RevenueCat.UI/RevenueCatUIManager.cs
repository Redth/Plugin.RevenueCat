using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Plugin.RevenueCat.UI;

internal class RevenueCatUIManager(IRevenueCatUIPlatformImplementation platformImplementation, ILoggerFactory? loggerFactory = null) : IRevenueCatUIManager
{
	private readonly IRevenueCatUIPlatformImplementation platformImplementation = platformImplementation;
	private readonly ILogger logger = loggerFactory?.CreateLogger<RevenueCatUIManager>()
		?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RevenueCatUIManager>.Instance;

	public Task<PaywallPresentationResult?> PresentPaywallAsync(PaywallPresentationOptions? options = null)
	{
		options ??= new PaywallPresentationOptions();

		return Request(
			nameof(PresentPaywallAsync),
			() => platformImplementation.PresentPaywallAsync(
				GetPlatformContext(),
				options.OfferingIdentifier,
				null,
				options.DisplayCloseButton),
			RevenueCatUISerializerContext.Default.PaywallPresentationResult);
	}

	public Task<PaywallPresentationResult?> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallPresentationOptions? options = null)
	{
		if (string.IsNullOrWhiteSpace(requiredEntitlementIdentifier))
			throw new ArgumentException("A required entitlement identifier is required.", nameof(requiredEntitlementIdentifier));

		options ??= new PaywallPresentationOptions();

		return Request(
			nameof(PresentPaywallIfNeededAsync),
			() => platformImplementation.PresentPaywallAsync(
				GetPlatformContext(),
				options.OfferingIdentifier,
				requiredEntitlementIdentifier,
				options.DisplayCloseButton),
			RevenueCatUISerializerContext.Default.PaywallPresentationResult);
	}

	public Task<CustomerCenterResult?> PresentCustomerCenterAsync()
		=> Request(
			nameof(PresentCustomerCenterAsync),
			() => platformImplementation.PresentCustomerCenterAsync(GetPlatformContext()),
			RevenueCatUISerializerContext.Default.CustomerCenterResult);

	private async Task<TModel?> Request<TModel>(string name, Func<Task<string?>> requestFunc, System.Text.Json.Serialization.Metadata.JsonTypeInfo<TModel> typeInfo)
		where TModel : class
	{
		logger.LogInformation("RevenueCatUI->{Name}: Starting request...", name);

		string? json;

		try
		{
			json = await requestFunc().ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "RevenueCatUI->{Name}: Request failed.", name);
			return default;
		}

		if (string.IsNullOrWhiteSpace(json))
		{
			logger.LogWarning("RevenueCatUI->{Name}: Result payload was empty.", name);
			return default;
		}

		try
		{
			return JsonSerializer.Deserialize(json, typeInfo);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "RevenueCatUI->{Name}: Failed to deserialize payload.", name);
			return default;
		}
	}

	private static object? GetPlatformContext()
	{
#if ANDROID
		return Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
#else
		return null;
#endif
	}
}
