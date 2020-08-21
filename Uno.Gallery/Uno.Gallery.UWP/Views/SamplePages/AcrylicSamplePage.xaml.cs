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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.Samples
{

	[SamplePage(SampleCategory.Features, "Acrylic", Description = "AcrylicBrush is a translucent brush that can be used as background.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/style/acrylic")]
	public sealed partial class AcrylicSamplePage : Page
	{
		public AcrylicSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
