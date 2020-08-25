using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Features, "ViewBox", Description = "Defines a content decorator that can stretch and scale a single child to fill the available space.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.viewbox")]
	public sealed partial class ViewBoxSamplePage : Page
	{
		public ViewBoxSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
