using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "TextBlock",
		Description = "A lightweight control for displaying text.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.textblock")]
	public sealed partial class TextBlockSamplePage : Page
	{
		public TextBlockSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
