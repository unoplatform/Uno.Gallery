using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	// There is no WebView implementation in WebAssembly
#if !__WASM__
	[SamplePage(SampleCategory.Components, "WebView")]
#endif
	public sealed partial class WebViewSamplePage : Page
	{
		public WebViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
