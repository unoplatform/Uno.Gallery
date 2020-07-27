using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Components, "TextBox")]
	[OverviewExample(Design.Material, "MaterialTextBoxExampleTemplate")]
	[OverviewExample(Design.Fluent, "FluentTextBoxExampleTemplate")]
	public sealed partial class TextBoxSamplePage : Page
	{
		public TextBoxSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
