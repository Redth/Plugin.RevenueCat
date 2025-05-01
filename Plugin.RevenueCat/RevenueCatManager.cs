using Plugin.RevenueCat.Models;
using RevenueCat;

namespace Plugin.RevenueCat;

public class CustomerInfoUpdatedEventArgs(CustomerInfoRequest customerInfoRequest) : global::System.EventArgs
{
	public CustomerInfoRequest CustomerInfoRequest => customerInfoRequest;

}

// All the code in this file is included in all platforms.
public class RevenueCatManager(IRevenueCatImpl revenueCatImpl) : IRevenueCatManager
{
	public event EventHandler<Plugin.RevenueCat.CustomerInfoUpdatedEventArgs>? CustomerInfoUpdated;

	public void Initialize(object platformContext, bool debugLog, string appStore, string apiKey, string userId)
	{
		revenueCatImpl.SetCustomerInfoUpdatedHandler((json) =>
		{
			var cir = CustomerInfoRequest.FromJson(json);
			CustomerInfoUpdated?.Invoke(this, new CustomerInfoUpdatedEventArgs(cir));
		});
		revenueCatImpl.Initialize(platformContext, debugLog, appStore, apiKey, userId);
	}

	public async Task<CustomerInfoRequest?> LoginAsync(string userId)
	{
		var s = await revenueCatImpl.LoginAsync(userId);

		CustomerInfoRequest cir = null;

		try {
			cir = CustomerInfoRequest.FromJson(s);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
		Console.WriteLine(s);

		return cir;
	}

	public async Task<CustomerInfoRequest?> GetCustomerInfoAsync(bool force)
	{
		var s = await revenueCatImpl.GetCustomerInfoAsync(force);

		var cir = CustomerInfoRequest.FromJson(s);

		Console.WriteLine(s);


		return cir;
	}

	public async Task<Offering?> GetOfferingAsync(string offeringIdentifier)
	{
		Offering? o = null;

		try {
			var s = await revenueCatImpl.GetOfferingAsync(offeringIdentifier);

			o = Offering.FromJson(s);

			Console.WriteLine(s);
		} catch (Exception ex) {

			Console.WriteLine(ex);
		}

		return o;
	}

	public async Task<CustomerInfoRequest?> RestoreAsync()
	{
		var s = await revenueCatImpl.RestoreAsync();

		return CustomerInfoRequest.FromJson(s);
	}

	public async Task<CustomerInfoRequest?> PurchaseAsync(object? platformContext, string offeringIdentifier, string packageIdentifier)
	{
		var s = await revenueCatImpl.PurchaseAsync(platformContext, offeringIdentifier, packageIdentifier);
		return CustomerInfoRequest.FromJson(s);
	}
}

