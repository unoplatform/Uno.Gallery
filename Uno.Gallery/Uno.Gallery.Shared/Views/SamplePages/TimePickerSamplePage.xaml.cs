
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
#if !__WASM__ && !__MACOS__
	[SamplePage(SampleCategory.UIComponents, "TimePicker", Description = "This control allows users to pick a time value.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.timepicker")]
#endif
	[SampleConditional(SampleConditionals.Disabled, Reason = "todo: styles not implemented")]
	public sealed partial class TimePickerSamplePage : Page
	{
		public TimePickerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
