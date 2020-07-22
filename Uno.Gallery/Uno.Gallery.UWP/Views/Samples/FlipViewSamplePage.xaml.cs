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
	[SamplePage("FlipView", "Simple FlipView sample", SourceSdk.UnoToolkit)]
	public sealed partial class FlipViewSamplePage : Page
	{
		public FlipViewSamplePage()
		{
			this.InitializeComponent();

			// TODO: Workaround because FlipView doesn't stretch vertically on Android
#if __ANDROID__
			//void SyncSize()
			//{
			//	flipView.Height = ActualHeight;
			//	flipView.Width = ActualWidth;
			//}

			//SizeChanged += (s, e) => SyncSize();
			//SyncSize();
#endif
		}
	}
}
