using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Local Settings", Description = "Use settings to remember the user-customizable aspects of your app. For example, a news reader could use app settings to save which news sources to display and what font to use for reading articles.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/uwp/get-started/settings-learning-track")]
	public sealed partial class LocalSettingsSamplePage : Page
	{
		private const string _settingsKey = "MySettingName";

		public LocalSettingsSamplePage()
		{
			this.InitializeComponent();
		}

		private void Save_Click(object sender, RoutedEventArgs args)
		{
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			localSettings.Values[_settingsKey] = DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString();
		}

		private async void Retrieve_Click(object sender, RoutedEventArgs args)
		{
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			var localValue = localSettings.Values[_settingsKey] as string;
			
			if (!string.IsNullOrEmpty(localValue))
			{
				var dialog = new AppDialog()
				{
					Title = "Stored value",
					Content = new TextBlock() { Text = localValue },
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
			else
			{
				var dialog = new AppDialog()
				{
					Title = "No data stored",
					PrimaryButtonText = "OK"
				};
				await dialog.ShowAsync();
			}
		}
	}
}
