using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "TwoPaneView",
		Description = "Represents a container with two views that size and position content in the available space, either side-by-side or top-bottom.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.twopaneview?view=winui-2.3")]
	public sealed partial class TwoPaneViewSamplePage : Page
	{
		public TwoPaneViewSamplePage()
		{
			InitializeComponent();
		}
	}
}
