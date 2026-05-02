#nullable enable

using Plugin.RevenueCat.Models;

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
		_ => null
	};

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
