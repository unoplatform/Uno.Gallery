using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	// There is no WebView implementation in WebAssembly
#if !__WASM__ && !__SKIA__
	[SamplePage(SampleCategory.UIComponents, "WebView", Description = "This control hosts a web page or HTML content within an application.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.webview2")]
#endif
	public sealed partial class WebViewSamplePage : Page
	{
		public WebViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
