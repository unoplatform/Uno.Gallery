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
#if !__WASM__ && !__MACOS__
	[SamplePage(SampleCategory.UIComponents, "FlipView", Description = "This control is used to show a collection of items, one item at a time. You have to \"flip\" to through the items.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.flipview")]
#endif
	public sealed partial class FlipViewSamplePage : Page
	{
		public FlipViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
