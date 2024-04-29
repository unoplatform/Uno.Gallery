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
			XamlRoot = App.Instance.MainWindow.Content.XamlRoot;
		}

		public AppDialog(string content, string title) : this()
		{
			Title = title;
			Content = content;
			PrimaryButtonText = "OK";
		}
	}
}
