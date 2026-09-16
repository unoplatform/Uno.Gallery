using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "AutoLayout",
		SourceSdk.UnoToolkit,
		Description = "A Figma-inspired layout panel that stacks children with configurable spacing, alignment, and independent overlay support.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/AutoLayoutControl.html",
		Tags = new[] { "layout", "panel", "figma" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Agnostic,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
		AccessibilityNotes = new[] { "The examples are static layout demonstrations with text labels describing each arrangement." },
		ResetBehavior = SampleContractDefaults.ReloadToReset,
		Variants = new[] { "Vertical and horizontal stacking", "Primary alignment", "Negative spacing", "Independent overlay layout" },
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
