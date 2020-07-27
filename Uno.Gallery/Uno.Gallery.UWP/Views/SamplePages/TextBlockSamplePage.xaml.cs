using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Components, "TextBlock")]
	[OverviewExample(Design.Material, "MaterialTextBoxExampleTemplate")]
	[OverviewExample(Design.Fluent, "FluentTextBlockExampleTemplate")]
	public sealed partial class TextBlockSamplePage : Page
	{
		public TextBlockSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
