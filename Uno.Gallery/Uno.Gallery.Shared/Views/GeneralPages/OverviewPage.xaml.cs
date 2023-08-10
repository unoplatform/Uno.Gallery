using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Extensions;
using Uno.Logging;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using static Uno.Gallery.SamplePageLayout;

namespace Uno.Gallery.Views.GeneralPages
{
	[SamplePage(SampleCategory.None, "Overview", glyph: "\uE10F")]
	public sealed partial class OverviewPage : Page
	{
		public OverviewPage()
		{
			this.InitializeComponent();
		}
	}
}
