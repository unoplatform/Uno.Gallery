using Windows.UI.Xaml.Controls;
using Uno.Gallery.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Components, "ToggleSwitch")]
	[OverviewExample(Design.Material, "MaterialToggleSwitchExampleTemplate")]
	[OverviewExample(Design.Fluent, "FluentToggleSwitchExampleTemplate")]
	public sealed partial class ToggleSwitchSamplePage : Page
	{
		public ToggleSwitchSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
