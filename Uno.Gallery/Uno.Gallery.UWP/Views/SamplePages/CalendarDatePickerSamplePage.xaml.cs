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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.SamplePages
{
#if !__IOS__ && !__ANDROID__ && !__MACOS__
	[SamplePage(SampleCategory.UIComponents, "CalendarDatePicker", Description = "Represents a control that allows a user to pick a date from a calendar display.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.calendardatepicker")]
#endif
	public sealed partial class CalendarDatePickerSamplePage : Page
	{
		public CalendarDatePickerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
