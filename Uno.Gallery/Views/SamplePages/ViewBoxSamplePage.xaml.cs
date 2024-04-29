using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "ViewBox", Description = "Defines a content decorator that can stretch and scale a single child to fill the available space.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.viewbox")]
	public sealed partial class ViewBoxSamplePage : Page
	{
		public ViewBoxSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
