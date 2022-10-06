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
	[SamplePage(SampleCategory.Components, "Shape", Description = "Shape is the base class of shape elements such as Ellipse, Rectangle or Path. Shapes are not templatable as they are drawn directly.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.shapes.shape")]
	public sealed partial class ShapeSamplePage : Page
	{
		public ShapeSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
