using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage("MessageDialog", "Simple MessageDialog sample", SourceSdk.UnoToolkit)]
	public sealed partial class MessageDialogSamplePage : Page
	{
		public MessageDialogSamplePage()
		{
			this.InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog("Hello world!");
			messageDialog.ShowAsync();
		}

		private void Button_Click2(object sender, RoutedEventArgs e)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog("This is a very important message.", "Notice");
			messageDialog.ShowAsync();
		}

		private void Button_Click3(object sender, RoutedEventArgs e)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog("Are you sure you want to log out?", "Log out")
			{
				Commands =
				{
					new Windows.UI.Popups.UICommand("Cancel"),
					new Windows.UI.Popups.UICommand("Log out"),
				}
			};
			messageDialog.ShowAsync();
		}
	}
}
