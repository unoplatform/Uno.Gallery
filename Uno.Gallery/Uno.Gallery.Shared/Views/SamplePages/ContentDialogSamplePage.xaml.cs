using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.UI.Toolkit;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "ContentDialog", Description = "This represents a dialog to show to users. Fully styleable with XAML.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.contentdialog")]
	public sealed partial class ContentDialogSamplePage : Page
	{
		public ContentDialogSamplePage()
		{
			this.InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var contentDialog = new ContentDialog()
			{
				Content = "Hello world!",
				PrimaryButtonText = "OK",
				XamlRoot = this.XamlRoot,
			};

			if ((sender is Button button) && button.Tag is { } tag && tag.ToString() == "Fluent")
			{
				var resourceDictionary = new ResourceDictionary();
				resourceDictionary.MergedDictionaries.Add(new XamlControlsResources());
				contentDialog.Resources = resourceDictionary;
			}

			await contentDialog.ShowAsync();
		}

		private async void Button_Click2(object sender, RoutedEventArgs e)
		{
			var contentDialog = new ContentDialog
			{
				Title = "Notice",
				Content = "This is a very important message.",
				PrimaryButtonText = "OK",
				XamlRoot = this.XamlRoot
			};

			if ((sender is Button button) && button.Tag is { } tag && tag.ToString() == "Fluent")
			{
				var resourceDictionary = new ResourceDictionary();
				resourceDictionary.MergedDictionaries.Add(new XamlControlsResources());
				contentDialog.Resources = resourceDictionary;
			}

			await contentDialog.ShowAsync();
		}

		private async void Button_Click3(object sender, RoutedEventArgs e)
		{
			var contentDialog = new ContentDialog()
			{
				Title = "Log out",
				Content = "Are you sure you want to log out?",
				PrimaryButtonText = "Log out",
				CloseButtonText = "Cancel",
				DefaultButton = ContentDialogButton.Primary,
				XamlRoot = this.XamlRoot,
			};

			if ((sender is Button button) && button.Tag is { } tag && tag.ToString() == "Fluent")
			{
				var resourceDictionary = new ResourceDictionary();
				resourceDictionary.MergedDictionaries.Add(new XamlControlsResources());
				contentDialog.Resources = resourceDictionary;
			}

			if (await contentDialog.ShowAsync() == ContentDialogResult.Primary)
			{
				// Log out
			}
			else
			{
				// Cancel
			}
		}
	}
}
