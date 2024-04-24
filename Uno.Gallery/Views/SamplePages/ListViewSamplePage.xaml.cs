using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.Samples
{

	[SamplePage(SampleCategory.UIComponents, "ListView",
		Description = "Represents a control that displays data items in a vertical stack.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.listview",
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
