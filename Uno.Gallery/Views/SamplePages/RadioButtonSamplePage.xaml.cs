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

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "RadioButton", Description = "This button allows user to select a single option from a group of options.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.radiobutton")]
	public sealed partial class RadioButtonSamplePage : Page
	{
		public RadioButtonSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
