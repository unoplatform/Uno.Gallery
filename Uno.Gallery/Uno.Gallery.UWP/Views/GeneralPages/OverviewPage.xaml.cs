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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Uno.Gallery.SamplePageLayout;

namespace Uno.Gallery.Views.GeneralPages
{
	[SamplePage(SampleCategory.None, "Overview")]
	public sealed partial class OverviewPage : Page
	{
		public OverviewPage()
		{
			this.InitializeComponent();
		}
	}
}
