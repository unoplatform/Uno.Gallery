using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Sharing", Description = "Programmatically initiates an exchange of content with other apps.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.datatransfer.datatransfermanager")]
    public sealed partial class SharingSamplePage : Page
	{
        public SharingSamplePage()
		{
			this.InitializeComponent();
		}

		private async void ShareText_Click(object sender, RoutedEventArgs args)
		{
			if (DataTransferManager.IsSupported())
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_Text;
				DataTransferManager.ShowShareUI();
			}
			else
			{
				var dialog = new ContentDialog()
				{
					Title = "Sharing is not supported on this device.",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}

		private void DataRequested_Text(DataTransferManager sender, DataRequestedEventArgs args)
		{
			args.Request.Data.Properties.Title = "Uno Gallery - Share-Sample Title";
			args.Request.Data.Properties.Description = "See this awesome shared text:";

			args.Request.Data.SetText("This text was created on " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString());

			sender.DataRequested -= DataRequested_Text;
		}

		private void ShareURI_Click(object sender, RoutedEventArgs args)
		{
			if (DataTransferManager.IsSupported())
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_URI;
				DataTransferManager.ShowShareUI();
			}
		}

		private void DataRequested_URI(DataTransferManager sender, DataRequestedEventArgs args)
		{
			args.Request.Data.Properties.Title = "Uno Gallery - Share-Sample Title";
			args.Request.Data.Properties.Description = "See this awesome project:";

			args.Request.Data.SetWebLink(new Uri("https://platform.uno"));

			sender.DataRequested -= DataRequested_URI;
		}

		private void ShareAsync_Click(object sender, RoutedEventArgs args)
		{
			if (DataTransferManager.IsSupported())
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_Async;
				DataTransferManager.ShowShareUI();
			}
		}

		private async void DataRequested_Async(DataTransferManager sender, DataRequestedEventArgs args)
		{
			var deferral = args.Request.GetDeferral();

			await Task.Delay(3000);

			args.Request.Data.Properties.Title = "Uno Gallery - Share-Sample Title";
			args.Request.Data.Properties.Description = "You've waited so long, here it is:";

			args.Request.Data.SetText("This text was delayed for 3 second.");

			sender.DataRequested -= DataRequested_URI;

			deferral.Complete();
		}

		private void ShareTextDark_Click(object sender, RoutedEventArgs args)
		{
			if (DataTransferManager.IsSupported())
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_TextDark;
				DataTransferManager.ShowShareUI(new ShareUIOptions { Theme = ShareUITheme.Dark });
			}
		}

		private void DataRequested_TextDark(DataTransferManager sender, DataRequestedEventArgs args)
		{
			args.Request.Data.Properties.Title = "Uno Gallery - Share-Sample Title";
			args.Request.Data.Properties.Description = "See this awesome shared dark theme text:";

			args.Request.Data.SetText("This dark theme text was created on " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString());

			sender.DataRequested -= DataRequested_TextDark;
		}

		private void ShareTextLight_Click(object sender, RoutedEventArgs args)
		{
			if (DataTransferManager.IsSupported())
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_TextLight;
				DataTransferManager.ShowShareUI(new ShareUIOptions { Theme = ShareUITheme.Light });
			}
		}

		private void DataRequested_TextLight(DataTransferManager sender, DataRequestedEventArgs args)
		{
			args.Request.Data.Properties.Title = "Uno Gallery - Share-Sample Title";
			args.Request.Data.Properties.Description = "See this awesome shared light theme text:";

			args.Request.Data.SetText("This light theme text was created on " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString());

			sender.DataRequested -= DataRequested_TextLight;
		}
	}
}
