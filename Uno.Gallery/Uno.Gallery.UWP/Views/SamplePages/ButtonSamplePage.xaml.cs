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
	[SamplePage(SampleCategory.Components, "Button", Description = Description, DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/buttons")]
	public sealed partial class ButtonSamplePage : Page
	{
		private const string Description = "A button is used to interpret a user click or tap interaction. They're often bound to commands.";

		public ButtonSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
