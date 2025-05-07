using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery
{
	// Wrapper to ensure that every ContentDialog used in the app properly sets XamlRoot
	// Added as a quick way to refactor dialog usages after the move to WinUI/WinAppSDK
    public partial class AppDialog : ContentDialog
	{
		public AppDialog()
		{
			Style = App.Instance.Resources["DefaultContentDialogStyle"] as Style;
			XamlRoot = App.Instance.MainWindow.Content.XamlRoot;
		}

		public AppDialog(string content, string title) : this()
		{
			Style = App.Instance.Resources["DefaultContentDialogStyle"] as Style;
			Title = title;
			Content = content;
			PrimaryButtonText = "OK";
		}
	}
}
