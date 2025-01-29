using System.Threading.Tasks;
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
        //this.revenueCatManager.SetEntitlementsUpdatedHandler(entitlements =>
        //{
        //    Dispatcher.Dispatch(() =>
        //    {
        //        txtEntitlements.Text = "Entitlements: "
        //                                + string.Join(", ", entitlements)
        //                                + Environment.NewLine
        //                                + "Refreshed at: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString();
        //    });
        //});
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
        if (!isInitialized)
        {
            if (string.IsNullOrEmpty(rcApiKey.Text))
            {
                this.DisplayAlert("Error", "API Key is required", "OK");
                return;
            }

            string? appStore = null;
            object platformContext = null;
            #if ANDROID
            appStore = "google";
            platformContext = this.Window.Handler.MauiContext.Context;
            #endif

            revenueCatManager.Initialize(platformContext, true, appStore, rcApiKey.Text, rcUserId.Text);
            isInitialized = true;
        }

        if (string.IsNullOrEmpty(rcUserId.Text))
        {
            this.DisplayAlert("Error", "User ID is required", "OK");
            return;
        }

        await revenueCatManager.LoginAsync(rcUserId.Text);
    }


    private async void Purchase_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(rcOfferingId.Text))
        {
            this.DisplayAlert("Error", "Offering Identifier is required", "OK");
            return;
        }

        var offering = await revenueCatManager.GetOfferingAsync("pro1year");

        Console.WriteLine(offering.Identifier);
    }

    private async void Update_Clicked(object? sender, EventArgs e)
    {
        var ci = await revenueCatManager.GetCustomerInfoAsync(true);
    }
}

