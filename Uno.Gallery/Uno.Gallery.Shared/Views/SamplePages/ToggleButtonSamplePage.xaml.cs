using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "ToggleButton", Description = "Represents a control that a user can select (check) or clear (uncheck). Base class for controls that can switch states, such as CheckBox and RadioButton.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.primitives.togglebutton?view=winrt-22621")]
	public sealed partial class ToggleButtonSamplePage : Page
	{
		public ToggleButtonSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
