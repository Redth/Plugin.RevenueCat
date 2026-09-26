using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

/// <summary>Optional iOS/macCatalyst capability for checking trial or introductory discount eligibility.</summary>
public interface IRevenueCatIntroEligibility
{
	Task<RevenueCatOperationResult<IReadOnlyDictionary<string, IntroEligibilityStatus>>> CheckTrialOrIntroDiscountEligibilityWithResultAsync(
		IEnumerable<string> productIdentifiers,
		CancellationToken cancellationToken = default);
}
