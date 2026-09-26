#nullable enable

using Plugin.RevenueCat.Models;
using System.Text.Json;

namespace Plugin.RevenueCat.Paywalls;

public interface IPaywallVariableProvider
{
	string? Resolve(string variableName, PaywallVariableContext context);
}

public sealed class PaywallVariableContext
{
	public Package? Package { get; init; }

	public string? ApplicationName { get; init; }

	public string? Locale { get; init; }

	public PaywallEligibility IntroOfferEligibility { get; init; } = PaywallEligibility.Unknown;

	public IReadOnlyDictionary<string, JsonElement>? CustomVariables { get; init; }
}

public sealed class DefaultPaywallVariableProvider : IPaywallVariableProvider
{
	public string? Resolve(string variableName, PaywallVariableContext context) => variableName switch
	{
		"app_name" => context.ApplicationName,
		"price" => context.Package?.StoreProduct?.PriceString,
		"product_name" => context.Package?.StoreProduct?.Title,
		"sub_period" => GetPeriodName(context.Package?.StoreProduct?.SubscriptionPeriod),
		"sub_duration" => GetDuration(context.Package?.StoreProduct?.SubscriptionPeriod),
		"price_per_period" => context.Package?.StoreProduct?.PriceString,
		"sub_price_per_week" => context.Package?.StoreProduct?.DefaultSubscriptionOption?.FullPricePhase?.PricePerWeek?.Formatted,
		"sub_price_per_month" => context.Package?.StoreProduct?.DefaultSubscriptionOption?.FullPricePhase?.PricePerMonth?.Formatted,
		"sub_offer_duration" => context.IntroOfferEligibility == PaywallEligibility.Eligible
			? GetDuration(GetIntroPhase(context.Package)?.BillingPeriod ??
				context.Package?.StoreProduct?.IntroductoryDiscount?.SubscriptionPeriod)
			: null,
		"sub_offer_price" => context.IntroOfferEligibility == PaywallEligibility.Eligible
			? GetIntroPhase(context.Package)?.Price?.Formatted ??
				context.Package?.StoreProduct?.IntroductoryDiscount?.PriceString
			: null,
		_ => context.CustomVariables?.TryGetValue(variableName, out var value) == true &&
			value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False
				? value.ToString() : null
	};

	static PricingPhase? GetIntroPhase(Package? package) =>
		package?.StoreProduct?.DefaultSubscriptionOption?.FreePhase ??
		package?.StoreProduct?.DefaultSubscriptionOption?.IntroPhase;

	static string? GetDuration(SubscriptionPeriod? period)
	{
		if (period is null || period.Unit == SubscriptionPeriodUnit.Unknown)
		{
			return null;
		}

		var unit = GetPeriodName(period);
		return period.Value == 1 ? unit : $"{period.Value} {unit}s";
	}

	static string? GetPeriodName(SubscriptionPeriod? period) => period?.Unit switch
	{
		SubscriptionPeriodUnit.Day => "day",
		SubscriptionPeriodUnit.Week => "week",
		SubscriptionPeriodUnit.Month => "month",
		SubscriptionPeriodUnit.Year => "year",
		_ => null
	};
}
