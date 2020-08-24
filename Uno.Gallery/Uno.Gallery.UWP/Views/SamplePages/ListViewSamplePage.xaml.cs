using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.Samples
{

	[SamplePage(SampleCategory.Features, "ListView",
		Description = "Represents a control that displays data items in a vertical stack.",
		DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.listview",
		DataType = typeof(RecordCollection)
	)]
	public sealed partial class ListViewSamplePage : Page
	{
		public ListViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}