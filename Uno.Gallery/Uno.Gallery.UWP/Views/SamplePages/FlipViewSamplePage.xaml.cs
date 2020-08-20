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
#if !__WASM__ && !__MACOS__
	[SamplePage(SampleCategory.Features, "FlipView", Description = "This control is used to show a collection of items, one item at a time. You have to \"flip\" to through the items.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.flipview")]
#endif
	public sealed partial class FlipViewSamplePage : Page
	{
		public FlipViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
