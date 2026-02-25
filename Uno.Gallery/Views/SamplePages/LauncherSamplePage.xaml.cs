using System;
using System.Threading.Tasks;
using Uno.Gallery.ViewModels;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Launcher", Description = "Provides the ability to launch websites or even apps.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.system.launcher", DataType = typeof(LauncherSamplePageViewModel))]
	public sealed partial class LauncherSamplePage : Page
	{
		public LauncherSamplePage()
		{
			this.InitializeComponent();
		}

		private async void LaunchSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			var result = await Launcher.LaunchUriAsync(new Uri("ms-settings:sound"));

			if (!result && ((Sample)DataContext).Data is LauncherSamplePageViewModel viewModel)
			{
				viewModel.IsSettingsInfoBarOpen = true;
			}
		}

		private void LaunchWebsiteButtonAsync_Click(object sender, RoutedEventArgs e)
		{
			if (((Sample)DataContext).Data is LauncherSamplePageViewModel viewModel)
			{
				viewModel.LaunchUri();
			}
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public class LauncherSamplePageViewModel : ViewModelBase
	{
		public string SettingsButtonContent { get => GetProperty<string>(); set => SetProperty(value); }
		public string LaunchUriText { get => GetProperty<string>(); set => SetProperty(value); }
		public bool IsWebsiteInfoBarOpen { get => GetProperty<bool>(); set => SetProperty(value); }
		public string WebsiteInfoBarText { get => GetProperty<string>(); set => SetProperty(value); }
		public bool IsSettingsInfoBarOpen { get => GetProperty<bool>(); set => SetProperty(value); }

		public LauncherSamplePageViewModel()
		{
#if __IOS__
			//In case of iOS, any special URI opens the main page of system settings (there is no settings deep-linking available on iOS)
			SettingsButtonContent = "Open Settings";
#else
			SettingsButtonContent = "Open Sound Settings";
#endif
			LaunchUriText = "https://platform.uno/";
		}

		public
#if !__WASM__
			async
#endif
			void LaunchUri()
		{
			if (!Uri.TryCreate(LaunchUriText, UriKind.Absolute, out var uri))
			{
				WebsiteInfoBarText = "Houston, we have a problem! The launch was aborted, it appears that this is not a correct URI.";
				IsWebsiteInfoBarOpen = true;

				return;
			}

#if !__WASM__
			var supportStatus = await Launcher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri);
#else
			var supportStatus = LaunchQuerySupportStatus.Available;
#endif

			if (supportStatus == LaunchQuerySupportStatus.Available)
			{
				_ = Launcher.LaunchUriAsync(uri);
				IsWebsiteInfoBarOpen = false;
			}
			else
			{
				WebsiteInfoBarText = "This special URI is not supported on the current device/platform.";
				IsWebsiteInfoBarOpen = true;
			}
		}
	}
}
