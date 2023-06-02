using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Views.NestedPages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.UIComponents, nameof(CommandBar),
		Description = "This control provides navigation and related actions for the current page.",
		DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.commandbar")]
	public sealed partial class CommandBarSamplePage : Page
	{
		public CommandBarSamplePage()
		{
			this.InitializeComponent();
		}

		private void LaunchFullScreenSample(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<CommandBarSample_NestedMaterialPage1>(
#if __ANDROID__ || __IOS__ || __MACCATALYST__
				useNativeUnoFrame: true,
#endif
				clearStack: true);
		}
	}
}
