using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	[SamplePage(SampleCategory.UIComponents, "SwipeControl", Description = Description, DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.swipecontrol")]
	public sealed partial class SwipeControlSamplePage : Page
	{
		private bool isArchived, isFlagged, isAccepted = false;

		private const string Description = "A control that provides access to contextual commands through swipe gestures.";
		
		public ObservableCollection<object> Items { get; } = new ObservableCollection<object>();

		public SwipeControlSamplePage()
		{
			this.InitializeComponent();
		}

		private void lv_Loaded(object sender, RoutedEventArgs e)
		{
			Items.Clear();
			var source = @"Swipe Item 1,Swipe Item 2,Swipe Item 3,Swipe Item 4".Split(',');
			foreach (var item in source)
				Items.Add(item);

			((ListView)sender).ItemsSource = Items;
		}

		private void DeleteOne_ItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			isArchived = !isArchived;

			if (isArchived)
			{
				((TextBlock)args.SwipeControl.Content).Text = "Archived - Swipe Left";
			}
			else
			{
				((TextBlock)args.SwipeControl.Content).Text = "Swipe Left";
			}
		}

		private void DeleteItem_ItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			Items.Remove(args.SwipeControl.DataContext);
		}

		private void Accept_ItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			isAccepted = !isAccepted;
			CheckAcceptFlagBool(args.SwipeControl);

			if (isAccepted)
			{
				FontIconSource cancelIcon = new FontIconSource() { Glyph = "\ue711" };
				sender.IconSource = cancelIcon;
				sender.Text = "Cancel";
			}
			else
			{
				FontIconSource acceptIcon = new FontIconSource() { Glyph = "\ue10B" };
				sender.IconSource = acceptIcon;
				sender.Text = "Accept";
			}
		}

		private void Flag_ItemInvoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
		{
			isFlagged = !isFlagged;
			CheckAcceptFlagBool(args.SwipeControl);

			if (isFlagged)
			{
				FontIconSource filledFlagIcon = new FontIconSource() { Glyph = "\ueB4B" };
				sender.IconSource = filledFlagIcon;
				sender.Text = "Unmark";
			}
			else
			{
				FontIconSource flagIcon = new FontIconSource() { Glyph = "\ue129" };
				sender.IconSource = flagIcon;
				sender.Text = "Flag";
			}
		}

		private void CheckAcceptFlagBool(SwipeControl swipeCtrl)
		{
			if (isAccepted && !isFlagged)
			{
				((TextBlock)swipeCtrl.Content).Text = "Swipe Right - Accepted";
			}
			else if (isAccepted && isFlagged)
			{
				((TextBlock)swipeCtrl.Content).Text = "Swipe Right - Accepted & Flagged";
			}
			else if (!isAccepted && isFlagged)
			{
				((TextBlock)swipeCtrl.Content).Text = "Swipe Right - Flagged";
			}
			else
			{
				((TextBlock)swipeCtrl.Content).Text = "Swipe Right";
			}
		}
	}
}
