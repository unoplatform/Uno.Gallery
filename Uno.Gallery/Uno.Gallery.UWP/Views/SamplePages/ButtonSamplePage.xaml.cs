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
	[SamplePage(SampleCategory.Components, "Button", Description = Description)]
	[OverviewExample(Design.Material, "MaterialButtonExampleTemplate")]
	[OverviewExample(Design.Fluent, "TodoExampleTemplate")]
	public sealed partial class ButtonSamplePage : Page
	{
		private const string Description = "Button styles for actions in forms, dialogs, and more with support for multiple sizes, states, and more.";

		public ButtonSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
