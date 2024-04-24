using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "Grid", Description = "Defines a flexible grid area that consists of columns and rows. Child elements of the Grid are measured and arranged according to their row/column assignments (set by using Grid.Row and Grid.Column attached properties) and other logic.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.grid")]
	public sealed partial class GridSamplePage : Page
	{
		public GridSamplePage()
		{
			InitializeComponent();
		}
	}
}
