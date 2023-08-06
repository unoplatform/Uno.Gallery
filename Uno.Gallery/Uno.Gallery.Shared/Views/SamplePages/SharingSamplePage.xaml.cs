using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using WinRT;
using WinRT.Interop;

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

				var dataTransferManager = DataTransferManagerHelper.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_Text;
				DataTransferManagerHelper.ShowShareUI();
			}
			else
			{
				var dialog = new AppDialog()
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
				var dataTransferManager = DataTransferManagerHelper.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_URI;
				DataTransferManagerHelper.ShowShareUI();
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
				var dataTransferManager = DataTransferManagerHelper.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_Async;
				DataTransferManagerHelper.ShowShareUI();
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
				var dataTransferManager = DataTransferManagerHelper.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_TextDark;
				DataTransferManagerHelper.ShowShareUI(new ShareUIOptions { Theme = ShareUITheme.Dark });
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
				var dataTransferManager = DataTransferManagerHelper.GetForCurrentView();
				dataTransferManager.DataRequested += DataRequested_TextLight;
				DataTransferManagerHelper.ShowShareUI(new ShareUIOptions { Theme = ShareUITheme.Light });
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

	public static class DataTransferManagerHelper
	{
		private static readonly Guid _dtm_iid = new Guid("a5caee9b-8708-49d1-8d36-67d25a8da00c");

#if WINDOWS
		static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();
#endif

		public static DataTransferManager GetForCurrentView()
		{
#if WINDOWS
			IntPtr result;
			var hwnd = WindowNative.GetWindowHandle(App.Instance.MainWindow); 
			result = DataTransferManagerInterop.GetForWindow(hwnd, _dtm_iid);
			DataTransferManager dataTransferManager = MarshalInterface<DataTransferManager>.FromAbi(result);
			return (dataTransferManager);
#else
			return DataTransferManager.GetForCurrentView();
#endif
		}

		public static void ShowShareUI(ShareUIOptions options = null)
		{
#if WINDOWS
			var hwnd = WindowNative.GetWindowHandle(App.Instance.MainWindow); 
			DataTransferManagerInterop.ShowShareUIForWindow(hwnd, options);
#else
			DataTransferManager.ShowShareUI(options);
#endif
		}

#if WINDOWS
		[ComImport]
		[Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IDataTransferManagerInterop
		{
			IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
			void ShowShareUIForWindow(IntPtr appWindow, ShareUIOptions options);
		}
#endif
	}
}
