using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Views.NestedPages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.UIComponents, nameof(CommandBar),
		Description = "This control provides navigation and related actions for the current page.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.commandbar")]
	public sealed partial class CommandBarSamplePage : Page
	{
		public CommandBarSamplePage()
		{
			this.InitializeComponent();
		}

		private void LaunchFullScreenSample(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<CommandBarSample_NestedMaterialPage1>(clearStack: true);
		}
	}
}
