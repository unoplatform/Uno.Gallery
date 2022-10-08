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
	[SamplePage(SampleCategory.UIComponents, "ElevatedView", Description = Description, DocumentationLink = "https://platform.uno/docs/articles/features/ElevatedView.html")]
	public sealed partial class ElevatedViewSamplePage : Page
	{
		private const string Description = "ElevatedView component allow to highlight the different levels of layout";

		public ElevatedViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
