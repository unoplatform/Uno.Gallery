using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Theming, "Lightweight Extension", Description = "Using an extension with dictionaries centralizes key overrides, promoting reusability and consistency across controls, reducing the need for repetitive per-component customizations.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/apps/design/style/xaml-styles#lightweight-styling")]
	public sealed partial class LightWeightExtensionSameplePage : Page
	{
		public LightWeightExtensionSameplePage()
		{
			this.InitializeComponent();
		}
	}
}
