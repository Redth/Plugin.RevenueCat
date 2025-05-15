namespace Plugin.RevenueCat.WebView;

public partial class RcPurchasePage : ContentPage
{
	public RcPurchasePage()
	{
		InitializeComponent();
	}

	public IRevenueCatManager RevenueCatManager { get; private set; }

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();

		if (RevenueCatManager is null)
		{
			RevenueCatManager = this.Handler.MauiContext.Services.GetRequiredService<IRevenueCatManager>();

			this.webView.Source = new UrlWebViewSource
			{
				Url = "https://pay.rev.cat/gthgdymjjhcwnfzb/"
			};
		}
	}

	
}