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
	[SamplePage(SampleCategory.UIComponents, "Flyout", Description = "A flyout is a UI container that can be light dismissed. It can contain other flyouts or context menus to create a nested experience.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/dialogs-and-flyouts/flyouts")]
	public sealed partial class FlyoutSamplePage : Page
	{
		public FlyoutSamplePage()
		{
			this.InitializeComponent();
		}
	}
}