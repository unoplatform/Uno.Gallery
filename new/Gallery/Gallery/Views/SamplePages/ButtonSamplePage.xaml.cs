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
	[SamplePage(SampleCategory.UIComponents, "Button", Description = Description, DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/buttons")]
	public sealed partial class ButtonSamplePage : Page
	{
		private const string Description = "A button is used to interpret a user click or tap interaction. They're often bound to commands.";

		public ButtonSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
