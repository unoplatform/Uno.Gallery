using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "ToggleSwitch", Description = "This control allows users to switch between only two values, on or off.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.toggleswitch")]
	public sealed partial class ToggleSwitchSamplePage : Page
	{
		public ToggleSwitchSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
