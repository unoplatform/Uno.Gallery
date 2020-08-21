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
	[SamplePage(SampleCategory.Features, "Brush", Description = "A brush describes how a surface is painted graphically.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.brush")]
	public sealed partial class BrushSamplePage : Page
	{
		public BrushSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
