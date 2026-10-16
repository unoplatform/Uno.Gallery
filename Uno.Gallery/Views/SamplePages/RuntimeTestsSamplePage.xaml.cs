using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	/// <summary>
	/// Hosts the runtime tests runner (Uno.Gallery/RuntimeTests).
	/// </summary>
	/// <remarks>
	/// Listed in Debug and canary builds; otherwise, only once the debug mode is entered or when the app is launched with `--rtt=1` (or `?rtt=1` on WebAssembly), see <see cref="App.GetSamples"/>.
	/// </remarks>
	[SamplePage(SampleCategory.Canary, "Runtime Tests")]
	public sealed partial class RuntimeTestsSamplePage : Page
	{
		public RuntimeTestsSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
