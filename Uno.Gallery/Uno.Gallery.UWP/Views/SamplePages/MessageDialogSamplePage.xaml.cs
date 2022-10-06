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
using Uno.UI.Toolkit;
using Windows.UI.Popups;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Components, "MessageDialog", Description = "This represents a simple dialog to show to users. Customization is limited to title text, content text and commands.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.popups.messagedialog")]
	public sealed partial class MessageDialogSamplePage : Page
	{
		public MessageDialogSamplePage()
		{
			this.InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var messageDialog = new MessageDialog("Hello world!");
			messageDialog.ShowAsync();
		}

		private void Button_Click2(object sender, RoutedEventArgs e)
		{
			var messageDialog = new MessageDialog("This is a very important message.", "Notice");
			messageDialog.ShowAsync();
		}

		private void Button_Click3(object sender, RoutedEventArgs e)
		{
			var messageDialog = new MessageDialog("Are you sure you want to log out?", "Log out")
			{
				Commands =
				{
					new UICommand("Cancel"),
					new UICommand("Log out"),
				}
			};
			messageDialog.ShowAsync();
		}

		private void Button_Click4(object sender, RoutedEventArgs e)
		{
			var deleteCommand = new UICommand("Delete");
			deleteCommand.SetDestructive(true);

			var messageDialog = new MessageDialog("Are you sure you want to delete this item?", "Delete")
			{
				
				Commands =
				{
					new UICommand("Cancel"),
					deleteCommand,
				}
			};
			messageDialog.ShowAsync();
		}
	}
}
