using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "MenuBar", Description = "Represents a specialized container that presents a set of menus in a horizontal row, typically at the top of an app window.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.menubar")]
	public sealed partial class MenuBarSamplePage : Page
	{
		public MenuBarSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
