using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using Plugin.RevenueCat;

namespace MauiSample;


public partial class MainPage : ContentPage
{
    readonly IRevenueCatManager revenueCatManager;

    public MainPage(IRevenueCatManager revenueCatManager)
    {
        InitializeComponent();

        this.revenueCatManager = revenueCatManager;

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
}

