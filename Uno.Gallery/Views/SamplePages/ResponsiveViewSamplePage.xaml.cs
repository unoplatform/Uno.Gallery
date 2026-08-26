using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "ResponsiveView",
		SourceSdk.UnoToolkit,
		Description = "Switches between DataTemplates based on configurable width breakpoints (Narrowest/Narrow/Normal/Wide/Widest).",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/ResponsiveView.html",
		Tags = new[] { "layout", "responsive", "adaptive", "breakpoints" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		SortOrder = 40)]
	public sealed partial class ResponsiveViewSamplePage : Page
	{
		public ResponsiveViewSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
