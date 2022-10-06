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


namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.UIComponents, "InfoBar", Description = "This control is an inline notification for essential app-wide messages.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.infobar?view=winui-2.5")]
	[SampleConditional(SampleConditionals.Disabled, Reason = "todo: styles not implemented")]
	public sealed partial class InfoBarSamplePage : Page
	{
		public InfoBarSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
