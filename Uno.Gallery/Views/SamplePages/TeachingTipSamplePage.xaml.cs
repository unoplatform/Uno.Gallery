using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "TeachingTip", Description = Description, DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/dialogs-and-flyouts/teaching-tip")]
	public sealed partial class TeachingTipSamplePage : Page
	{
		private const string Description = "A teaching tip is a notification flyout used to provide contextually relevant information.";

		public TeachingTipSamplePage()
		{
			this.InitializeComponent();
		}

		private void ShowTip_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var parent = (FrameworkElement)button.Parent;
			var tip = (TeachingTip)parent.FindName("TeachingTipForTest");
			tip.IsOpen = true;
		}
	}
}
