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
	[SamplePage(SampleCategory.UIFeatures, "Lottie", Description = "Lottie is a library for Android, iOS, Web, and Windows that parses Adobe After Effects animations exported as json with Bodymovin and renders them natively on mobile and on the web.", DocumentationLink = "https://airbnb.io/lottie/#/")]
	public sealed partial class LottieSamplePage : Page
	{
		public LottieSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
