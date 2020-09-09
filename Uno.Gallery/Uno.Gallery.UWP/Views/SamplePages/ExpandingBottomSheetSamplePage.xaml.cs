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

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Components, "ExpandingBottomSheet", SourceSdk.UnoMaterial, Description = "This control allows users to toggle optional page content.", DocumentationLink = "https://material.io/components/sheets-bottom#expanding-bottom-sheet")]

	public sealed partial class ExpandingBottomSheetSamplePage : Page
	{
		public ExpandingBottomSheetSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
