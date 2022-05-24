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
	[SamplePage(SampleCategory.Components, "Progress Ring/Bar", Description = "These controls are used to display progress, either as a bar or a \"spinner\".", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/microsoft.ui.xaml.controls.progressbar")]
	[SampleConditional(SampleConditionals.Disabled, Reason = "todo: styles not implemented")]
	public sealed partial class ProgressSamplePage : Page
	{
		public ProgressSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
