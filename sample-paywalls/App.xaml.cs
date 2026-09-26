namespace PaywallGallerySample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new NavigationPage(new PaywallGalleryPage()))
		{
			Width = 430,
			Height = 932
		};
	}
}
