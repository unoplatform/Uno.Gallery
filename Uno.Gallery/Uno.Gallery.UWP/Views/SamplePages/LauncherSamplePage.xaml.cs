using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Uno.Gallery.ViewModels;

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

			if(!result && ((Sample)DataContext).Data is LauncherSamplePageViewModel viewModel)
			{
				viewModel.IsInfoBarOpen = true;
			}
		}

		private void LaunchWebsiteButton_Click(object sender, RoutedEventArgs e)
		{
			Launcher.LaunchUriAsync(new Uri("https://platform.uno/"));
		}
	}

	public class LauncherSamplePageViewModel : ViewModelBase
	{
		private const string _iosSettingsTextContent = "Open Settings";
		private const string _settingsContent = "Open Sound Settings";

		public LauncherSamplePageViewModel()
		{
#if __IOS__
			SettingsButtonContent = _iosSettingsTextContent;
#else
			SettingsButtonContent = _settingsContent;
#endif
		}

		public string SettingsButtonContent { get => GetProperty<string>(); set => SetProperty(value); }

		public bool IsInfoBarOpen { get => GetProperty<bool>(); set => SetProperty(value); }
	}
}
