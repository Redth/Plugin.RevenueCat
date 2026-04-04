using System.Text.Json.Serialization;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.UI;

public enum PaywallPresentationResultType
{
	Unknown,
	NotPresented,
	Cancelled,
	Purchased,
	Restored,
	Error
}

public enum CustomerCenterResultType
{
	Unknown,
	Dismissed,
	Error
}

public sealed class PaywallPresentationResult
{
	[JsonPropertyName("result")]
	public string? Result { get; init; }

	[JsonPropertyName("customerInfo")]
	public CustomerInfo? CustomerInfo { get; init; }

	[JsonPropertyName("errorMessage")]
	public string? ErrorMessage { get; init; }

	[JsonIgnore]
	public PaywallPresentationResultType ResultType => Result?.ToLowerInvariant() switch
	{
		"notpresented" => PaywallPresentationResultType.NotPresented,
		"cancelled" => PaywallPresentationResultType.Cancelled,
		"purchased" => PaywallPresentationResultType.Purchased,
		"restored" => PaywallPresentationResultType.Restored,
		"error" => PaywallPresentationResultType.Error,
		_ => PaywallPresentationResultType.Unknown
	};
}

public sealed class CustomerCenterResult
{
	[JsonPropertyName("result")]
	public string? Result { get; init; }

	[JsonPropertyName("customerInfo")]
	public CustomerInfo? CustomerInfo { get; init; }

	[JsonPropertyName("errorMessage")]
	public string? ErrorMessage { get; init; }

	[JsonPropertyName("action")]
	public string? Action { get; init; }

	[JsonPropertyName("actionIdentifier")]
	public string? ActionIdentifier { get; init; }

	[JsonIgnore]
	public CustomerCenterResultType ResultType => Result?.ToLowerInvariant() switch
	{
		"dismissed" => CustomerCenterResultType.Dismissed,
		"error" => CustomerCenterResultType.Error,
		_ => CustomerCenterResultType.Unknown
	};
}
