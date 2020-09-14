using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
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
	[SamplePage(
		category: SampleCategory.Components,
		title: "Divider",
		Description = "This control is a thin line than can be used to divide layouts or groups content inside of lists.",
		DocumentationLink = "https://material.io/components/dividers",
		DataType = typeof(DividerItems)
	)]
	public sealed partial class DividerSamplePage : Page
	{
		public DividerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
