namespace Plugin.RevenueCat;

public interface IRevenueCatIntroEligibilityPlatform
{
	Task<string?> CheckTrialOrIntroDiscountEligibilityAsync(string productIdentifiersCsv);
}
