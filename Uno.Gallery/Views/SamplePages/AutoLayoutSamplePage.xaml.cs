using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "AutoLayout",
		SourceSdk.UnoToolkit,
		Description = "A Figma-inspired layout panel that stacks children with configurable spacing, alignment, and independent overlay support.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/AutoLayout.html",
		Tags = new[] { "layout", "panel", "figma" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		SortOrder = 10)]
	public sealed partial class AutoLayoutSamplePage : Page
	{
		public AutoLayoutSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
