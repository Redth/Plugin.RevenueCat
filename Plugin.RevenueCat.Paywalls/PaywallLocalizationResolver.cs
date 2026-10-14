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

		foreach (var locale in GetLocaleCandidates(paywallData, requestedLocale))
		{
			if (paywallData.ComponentsLocalizations[locale].TryGetValue(localizationId, out var localization) &&
				localization.Text is not null)
			{
				return localization.Text;
			}
		}

		return null;
	}

	public static string? ResolveLocale(PaywallComponentsData paywallData, string? requestedLocale) =>
		GetLocaleCandidates(paywallData, requestedLocale).FirstOrDefault();

	internal static IEnumerable<string> GetLocaleCandidates(PaywallComponentsData paywallData, string? requestedLocale)
	{
		var keys = paywallData.ComponentsLocalizations.Keys;
		var requested = NormalizeLocale(requestedLocale);
		var language = requested?.Split('_')[0];
		var candidates = new List<string>();
		foreach (var candidate in new[] { requested, language, NormalizeLocale(paywallData.DefaultLocale) })
		{
			var match = keys.FirstOrDefault(key => string.Equals(NormalizeLocale(key), candidate, StringComparison.OrdinalIgnoreCase));
			if (match is not null && !candidates.Contains(match))
			{
				candidates.Add(match);
			}
		}

		if (candidates.Count == 0 && keys.FirstOrDefault() is { } first)
		{
			candidates.Add(first);
		}
		return candidates;
	}

	static string? NormalizeLocale(string? locale) =>
		string.IsNullOrWhiteSpace(locale) ? null : locale.Replace('-', '_');
}
