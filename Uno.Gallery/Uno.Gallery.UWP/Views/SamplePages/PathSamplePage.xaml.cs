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
	[SamplePage(SampleCategory.Features, "Path", Description = "Draws a series of connected lines and curves. The line and curve dimensions are declared through the Data property, and can be specified either with Move and draw commands syntax, or with an object model.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.shapes.path")]
	public sealed partial class PathSamplePage : Page
	{
		public PathSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
