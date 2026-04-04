using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using Plugin.RevenueCat;
using Plugin.RevenueCat.UI;

namespace MauiSample;


public partial class MainPage : ContentPage
{
	readonly IRevenueCatManager revenueCatManager;
	readonly IRevenueCatUIManager revenueCatUIManager;

	public MainPage(IRevenueCatManager revenueCatManager, IRevenueCatUIManager revenueCatUIManager)
	{
		InitializeComponent();

		this.revenueCatManager = revenueCatManager;
		this.revenueCatUIManager = revenueCatUIManager;

		this.revenueCatManager.CustomerInfoUpdated += RevenueCatManager_CustomerInfoUpdated;
	}

	private void RevenueCatManager_CustomerInfoUpdated(object? sender, CustomerInfoUpdatedEventArgs e)
	{
		Dispatcher.Dispatch(() =>
		{
			txtEntitlements.Text = "Entitlements: "
									+ string.Join(", ", e.CustomerInfoRequest.Subscriber.Entitlements.Keys)
									+ Environment.NewLine
									+ "Refreshed at: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString();
		});
	}

	private bool isInitialized = false;

	private async void Identify_Clicked(object? sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rcUserId.Text))
		{
			await this.DisplayAlert("Error", "User ID is required", "OK");
			return;
		}

		await revenueCatManager.LoginAsync(rcUserId.Text);
	}


	private async void Purchase_Clicked(object? sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rcOfferingId.Text))
		{
			await this.DisplayAlert("Error", "Offering Identifier is required", "OK");
			return;
		}

		var offering = await revenueCatManager.GetOfferingAsync("pro1year");

		if (!(offering?.Packages?.Any() ?? false))
		{
			await this.DisplayAlert("Error", "No packages found for offering", "OK");
			return;
		}

		var packageChoice = await this.DisplayActionSheet("Select Package", "Cancel", null, offering?.Packages.Select(x => x.Identifier).ToArray());

		var package = offering?.Packages.FirstOrDefault(x => x.Identifier == packageChoice);

		Console.WriteLine($"Offering: {offering?.Identifier}");
		Console.WriteLine($"Package: {package?.Identifier}");

		if (offering == null || package == null)
		{
			await this.DisplayAlert("Error", "Offering or Package not found", "OK");
			return;
		}

		try
		{
			var sk = await revenueCatManager.PurchaseAsync(offering.Identifier, package.Identifier);
		}
		catch (Exception ex)
		{
			Console.WriteLine("PURCHASE FAILED");
			Console.WriteLine($"Error: {ex.Message}");
			Console.WriteLine(ex);
			await this.DisplayAlert("Error", ex.Message, "OK");
		}
	}

	private async void Restore_Clicked(object? sender, EventArgs e)
	{
		try
		{
			var sk = await revenueCatManager.RestoreAsync();

		}
		catch (Exception ex)
		{
			Console.WriteLine("RESTORE FAILED");
			Console.WriteLine($"Error: {ex.Message}");
			Console.WriteLine(ex);
			await this.DisplayAlert("Error", ex.Message, "OK");
		}

	}

	private async void Update_Clicked(object? sender, EventArgs e)
	{
		var ci = await revenueCatManager.GetCustomerInfoAsync(true);
	}

	private async void GetOffering_Clicked(object? sender, EventArgs e)
	{
		var o = await revenueCatManager.GetOfferingAsync(rcOfferingId.Text);
		if (o == null)
		{
			await this.DisplayAlert("Error", "Offering not found", "OK");
			return;
		}

		var pkg = o.Packages.FirstOrDefault();
		if (pkg == null)
		{
			await this.DisplayAlert("Error", "Package not found", "OK");
			return;
		}

		var storeProduct = pkg.StoreProduct;
		if (storeProduct == null)
		{
			await this.DisplayAlert("Error", "Store product not found", "OK");
			return;
		}
		var title = storeProduct.Title;
		var description = storeProduct.Description;
		var price = storeProduct.PriceString;
		var currency = storeProduct.CurrencyCode;

		var priceString = $"{title} - {description} - {price} {currency}";
		await this.DisplayAlert($"Offering: {rcOfferingId.Text}", priceString, "OK");
	}

	private async void ShowPaywall_Clicked(object? sender, EventArgs e)
	{
		var result = await revenueCatUIManager.PresentPaywallAsync(new PaywallPresentationOptions
		{
			OfferingIdentifier = rcOfferingId.Text,
			DisplayCloseButton = true,
		});

		if (result == null)
		{
			await DisplayAlert("Paywall", "The paywall could not be shown.", "OK");
			return;
		}

		var message = result.ErrorMessage ?? "The paywall flow finished successfully.";
		await DisplayAlert("Paywall", $"{result.ResultType}: {message}", "OK");
	}

	private async void ShowCustomerCenter_Clicked(object? sender, EventArgs e)
	{
		var result = await revenueCatUIManager.PresentCustomerCenterAsync();

		if (result == null)
		{
			await DisplayAlert("Customer Center", "The customer center could not be shown.", "OK");
			return;
		}

		var message = result.ErrorMessage ?? result.Action ?? "Dismissed";
		await DisplayAlert("Customer Center", $"{result.ResultType}: {message}", "OK");
	}
}

