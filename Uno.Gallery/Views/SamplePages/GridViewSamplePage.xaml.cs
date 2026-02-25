using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Uno.Gallery.ViewModels;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "GridView", DataType = typeof(GridViewSampleViewModel), Description = "Represents a control that displays data items in rows and columns.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.gridview?view=winrt-19041")]
	public sealed partial class GridViewSamplePage : Page
	{
		public GridViewSamplePage()
		{
			this.InitializeComponent();
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public class GridViewSampleViewModel : ViewModelBase
	{
		public char[] Items { get => GetProperty<char[]>(); set => SetProperty(value); }

		public GridViewSampleViewModel()
		{
			this.Items = "123456789ABCDEFGHIJKLMNO".ToCharArray();
		}
	}
}
