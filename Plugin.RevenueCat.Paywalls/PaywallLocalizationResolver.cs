#nullable enable

using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat.Paywalls;

public static class PaywallLocalizationResolver
{
	public static string? ResolveText(PaywallComponentsData? paywallData, string? requestedLocale, string? localizationId)
	{
		if (paywallData is null || string.IsNullOrWhiteSpace(localizationId))
		{
			return null;
		}

		var locale = ResolveLocale(paywallData, requestedLocale);
		if (locale is not null &&
			paywallData.ComponentsLocalizations.TryGetValue(locale, out var localeValues) &&
			localeValues.TryGetValue(localizationId, out var preferredLocalization) &&
			preferredLocalization.Text is not null)
		{
			return preferredLocalization.Text;
		}

		foreach (var values in paywallData.ComponentsLocalizations.Values)
		{
			if (values.TryGetValue(localizationId, out var fallbackLocalization) && fallbackLocalization.Text is not null)
			{
				return fallbackLocalization.Text;
			}
		}

		return null;
	}

	public static string? ResolveLocale(PaywallComponentsData paywallData, string? requestedLocale)
	{
		var candidates = new[]
		{
			NormalizeLocale(requestedLocale),
			NormalizeLocale(paywallData.DefaultLocale)
		};

		foreach (var candidate in candidates)
		{
			if (candidate is not null && paywallData.ComponentsLocalizations.ContainsKey(candidate))
			{
				return candidate;
			}
		}

		return paywallData.ComponentsLocalizations.Keys.FirstOrDefault();
	}

	static string? NormalizeLocale(string? locale) =>
		string.IsNullOrWhiteSpace(locale) ? null : locale.Replace('-', '_');
}
