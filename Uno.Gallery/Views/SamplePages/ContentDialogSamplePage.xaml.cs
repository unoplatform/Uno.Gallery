using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShowMeTheXAML;
using System;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

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

		var xamlDisplay = VisualTreeHelperEx.FindAncestor<XamlDisplay>(sender as Button);
		if (xamlDisplay?.UniqueKey?.Contains("_Fluent_") == true)
		{
			contentDialog.EnsureXamlControlsResources();
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

		var xamlDisplay = VisualTreeHelperEx.FindAncestor<XamlDisplay>(sender as Button);
		if (xamlDisplay?.UniqueKey?.Contains("_Fluent_") == true)
		{
			contentDialog.EnsureXamlControlsResources();
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

		var xamlDisplay = VisualTreeHelperEx.FindAncestor<XamlDisplay>(sender as Button);
		if (xamlDisplay?.UniqueKey?.Contains("_Fluent_") == true)
		{
			contentDialog.EnsureXamlControlsResources();
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
