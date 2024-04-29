using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
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
	[SamplePage(
		category: SampleCategory.Toolkit,
		title: "Divider",
		Description = "A divider is a thin line that groups content in lists and layouts.",
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
