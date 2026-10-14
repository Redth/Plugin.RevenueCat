using System.Collections.ObjectModel;

namespace PaywallGallerySample;

public partial class PaywallGalleryPage : ContentPage
{
	bool loaded;

	public PaywallGalleryPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	public ObservableCollection<PaywallExample> Examples { get; } = new();

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (loaded)
		{
			return;
		}

		loaded = true;
		try
		{
			foreach (var example in await PaywallExampleLoader.LoadAsync())
			{
				Examples.Add(example);
			}
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Unable to load paywalls", ex.Message, "OK");
		}
	}

	async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is CollectionView collectionView)
		{
			collectionView.SelectedItem = null;
		}

		if (e.CurrentSelection.FirstOrDefault() is PaywallExample example)
		{
			await OpenExampleAsync(example);
		}
	}

	async void OnPreviewClicked(object sender, EventArgs e)
	{
		if (sender is Button { BindingContext: PaywallExample example })
		{
			await OpenExampleAsync(example);
		}
	}

	Task OpenExampleAsync(PaywallExample example) =>
		Navigation.PushAsync(new PaywallPreviewPage(example));
}
