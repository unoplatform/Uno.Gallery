using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "RelativePanel", Description = "RelativePanel lets you layout UI elements by specifying where they go in relation to other elements and in relation to the panel. By default, an element is positioned in the upper left corner of the panel.", DocumentationLink = "https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.relativepanel")]
	public sealed partial class RelativePanelSamplePage : Page
	{
		public RelativePanelSamplePage()
		{
			InitializeComponent();
		}
	}
}
