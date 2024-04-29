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


namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.UIComponents, "InfoBar", Description = "This control is an inline notification for essential app-wide messages.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.infobar")]
	[SampleConditional(SampleConditionals.Disabled, Reason = "todo: styles not implemented")]
	public sealed partial class InfoBarSamplePage : Page
	{
		public InfoBarSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
