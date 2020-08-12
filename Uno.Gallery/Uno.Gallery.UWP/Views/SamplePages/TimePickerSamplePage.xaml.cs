
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
#if !__WASM__ && !__MACOS__
	[SamplePage(SampleCategory.Components, "TimePicker", Description = "This control allows users to pick a time value.")]
#endif
	public sealed partial class TimePickerSamplePage : Page
	{
		public TimePickerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
