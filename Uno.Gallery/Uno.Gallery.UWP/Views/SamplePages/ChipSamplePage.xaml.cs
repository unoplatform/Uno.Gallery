using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
using Uno.Toolkit.UI;
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
	[SamplePage(SampleCategory.Toolkit, "Chip",
		SourceSdk.UnoToolkit,
		Description = "Chips are compact elements that represent an input, attribute, or action.",
		DocumentationLink = "https://material.io/components/chips",
		DataType = typeof(TestCollections))]
	public sealed partial class ChipSamplePage : Page
	{
		public ChipSamplePage()
		{
			this.InitializeComponent();
		}

	}
}
