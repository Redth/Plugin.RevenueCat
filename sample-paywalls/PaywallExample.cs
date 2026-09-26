using Plugin.RevenueCat.Models;

namespace PaywallGallerySample;

public sealed class PaywallExample
{
	public PaywallExample(string title, string description, string componentSummary, PaywallOfferingsResponse response)
	{
		Title = title;
		Description = description;
		ComponentSummary = componentSummary;
		Response = response;
		Packages = CreateDemoPackages(response.CurrentOffering ?? response.Offerings.FirstOrDefault());
	}

	public string Title { get; }

	public string Description { get; }

	public string ComponentSummary { get; }

	public string AutomationId => $"Preview{new string(Title.Where(char.IsLetterOrDigit).ToArray())}";

	public PaywallOfferingsResponse Response { get; }

	public IReadOnlyList<Package> Packages { get; }

	static IReadOnlyList<Package> CreateDemoPackages(PaywallOffering? offering)
	{
		if (offering is null)
		{
			return [];
		}

		return offering.Packages
			.Select((package, index) => CreateDemoPackage(package, index))
			.ToArray();
	}

	static Package CreateDemoPackage(PaywallPackage package, int index)
	{
		var identifier = string.IsNullOrWhiteSpace(package.Identifier) ? $"package_{index}" : package.Identifier;
		var productIdentifier = package.PlatformProductIdentifier ?? identifier;
		var details = GetDemoProductDetails(identifier);

		return new Package
		{
			Id = identifier,
			Identifier = identifier,
			PackageType = details.PackageType,
			WebCheckoutUrl = package.WebCheckoutUrl,
			StoreProduct = new StoreProduct
			{
				Identifier = productIdentifier,
				Title = details.Title,
				Description = details.Description,
				PriceString = details.Price,
				CurrencyCode = "USD",
				SubscriptionPeriod = details.Period
			}
		};
	}

	static DemoProductDetails GetDemoProductDetails(string identifier)
	{
		return identifier switch
		{
			"$rc_weekly" => new(
				"Weekly Pro",
				"Weekly access for rapid testing.",
				"$2.99",
				PackageType.Weekly,
				new SubscriptionPeriod { Value = 1, Unit = SubscriptionPeriodUnit.Week }),
			"$rc_annual" => new(
				"Annual Pro",
				"The best value annual plan.",
				"$49.99",
				PackageType.Annual,
				new SubscriptionPeriod { Value = 1, Unit = SubscriptionPeriodUnit.Year }),
			"$rc_lifetime" => new(
				"Lifetime Pro",
				"One payment for lifetime access.",
				"$149.99",
				PackageType.Lifetime,
				null),
			"$rc_three_month" => new(
				"Season Pass",
				"Three months of premium access.",
				"$17.99",
				PackageType.Three_Month,
				new SubscriptionPeriod { Value = 3, Unit = SubscriptionPeriodUnit.Month }),
			"$rc_six_month" => new(
				"Half-Year Pro",
				"Six months of premium access.",
				"$29.99",
				PackageType.Six_Month,
				new SubscriptionPeriod { Value = 6, Unit = SubscriptionPeriodUnit.Month }),
			_ => new(
				"Monthly Pro",
				"Monthly access to every premium feature.",
				"$6.99",
				PackageType.Monthly,
				new SubscriptionPeriod { Value = 1, Unit = SubscriptionPeriodUnit.Month })
		};
	}

	sealed record DemoProductDetails(
		string Title,
		string Description,
		string Price,
		PackageType PackageType,
		SubscriptionPeriod? Period);
}
