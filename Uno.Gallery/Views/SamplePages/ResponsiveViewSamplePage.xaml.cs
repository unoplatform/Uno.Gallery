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
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Agnostic,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "Resize the application window or viewport to cross the documented breakpoints." },
		AccessibilityNotes = new[] { "Every responsive state includes a text label; resizing changes layout without moving keyboard focus." },
		ResetBehavior = "Resize the viewport back to its original width.",
		Variants = new[] { "Default breakpoints", "Inherited ResponsiveLayout resource", "Local ResponsiveLayout override" },
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
