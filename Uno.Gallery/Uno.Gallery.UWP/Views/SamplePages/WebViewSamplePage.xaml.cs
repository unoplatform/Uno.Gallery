using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	// There is no WebView implementation in WebAssembly
#if !__WASM__
	[SamplePage(SampleCategory.Features, "WebView", Description = "This control hosts a web page or HTML content within an application.")]
#endif
	public sealed partial class WebViewSamplePage : Page
	{
		public WebViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
