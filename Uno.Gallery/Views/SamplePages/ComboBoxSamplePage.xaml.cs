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
	[SamplePage(SampleCategory.UIComponents, "ComboBox", DataType = typeof(ComboBoxSampleViewModel), Description = "This control used for selection is a drop-down list that shows its selection in a box.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.combobox")]
	public sealed partial class ComboBoxSamplePage : Page
	{
		public ComboBoxSamplePage()
		{
			this.InitializeComponent();
		}
	}

	public class ComboBoxSampleViewModel : ViewModelBase
	{
		public string[] Items { get => GetProperty<string[]>(); set => SetProperty(value); }

		public ComboBoxSampleViewModel()
		{
			this.Items = new string[] { "a", "b", "c", "d", "e", "f", "g" };
		}
	}
}
