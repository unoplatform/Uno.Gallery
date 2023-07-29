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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.SamplePages
{
#if !__IOS__ && !__ANDROID__ && !__MACOS__
	[SamplePage(SampleCategory.UIComponents, "CalendarDatePicker", Description = "Represents a control that allows a user to pick a date from a calendar display.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.calendardatepicker")]
#endif
	public sealed partial class CalendarDatePickerSamplePage : Page
	{
		public CalendarDatePickerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
