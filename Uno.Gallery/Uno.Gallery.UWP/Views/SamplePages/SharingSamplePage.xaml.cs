using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Sharing", Description = "Represents a Credential Locker of credentials. Lockers are specific to a user.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.security.credentials.passwordvault")]
#if __WASM__
	[SampleConditional(SampleConditionals.Disabled, Reason = "API is not yet supported")]
#endif
    public sealed partial class SharingSamplePage : Page
	{
        public SharingSamplePage()
		{
			this.InitializeComponent();
		}

		private void ShareText_Click(object sender, RoutedEventArgs args)
		{
			if (DataTransferManager.IsSupported())
			{
				var dataTransferManager = DataTransferManager.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_Text;
				DataTransferManager.ShowShareUI();
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
	}
}
