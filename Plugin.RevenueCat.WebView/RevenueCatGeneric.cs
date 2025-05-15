using Plugin.RevenueCat.Api;
using Plugin.RevenueCat.Models;

namespace Plugin.RevenueCat;

public class RevenueCatGeneric(IRevenueCatApiV1 RevenueCatApiVi) : IRevenueCatManager
{
	public string? ApiKey { get; }

	public event EventHandler<CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	public async Task<CustomerInfo?> GetCustomerInfoAsync(bool force)
	{
		if (string.IsNullOrEmpty(UserId))
			return default;

		var c = await RevenueCatApiVi.GetOrCreateCustomer(UserId);
		
		var customerInfo = new CustomerInfo();

		customerInfo.CustomerInfoRequestDate = c.RequestDate;
		customerInfo.RequestDate = c.RequestDate ?? DateTimeOffset.Now;
		customerInfo.Subscriber = c.Subscriber ?? new Subscriber();

		return customerInfo;
	}

	public async Task<Offering?> GetOfferingAsync(string offeringIdentifier)
	{
		var c = await RevenueCatApiVi.GetOfferings(UserId);

		return c.Offerings?.FirstOrDefault(c => c.Identifier == offeringIdentifier); ;
	}

	public void Initialize()
	{
	}

	public string UserId { get; private set; } = string.Empty;

	public Task<CustomerInfo?> LoginAsync(string userId)
	{
		UserId = userId;

		return GetCustomerInfoAsync(true);
	}

	public Task<CustomerInfo?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
	{
		
	}

	public Task<CustomerInfo?> RestoreAsync()
	{
		return GetCustomerInfoAsync(true);
	}
}
