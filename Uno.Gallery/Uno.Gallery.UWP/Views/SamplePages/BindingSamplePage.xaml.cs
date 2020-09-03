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
	[SamplePage(SampleCategory.Features, "Binding", Description = "Bindings allow you to pass data between your UI and business logic.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/data-binding/")]
	public sealed partial class BindingSamplePage : Page
	{
		public BindingSamplePage()
		{
			this.InitializeComponent();
		}
	}
}