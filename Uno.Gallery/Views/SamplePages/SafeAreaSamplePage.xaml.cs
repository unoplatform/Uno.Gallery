using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "SafeArea",
		SourceSdk.UnoToolkit,
		Description = "Attached properties that protect content from device notches, rounded corners, home indicators, and soft keyboard overlaps.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/SafeArea.html",
		Tags = new[] { "layout", "platform", "insets", "notch", "keyboard" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Agnostic,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "A mobile device or emulator with display cutouts, system bars, or a software keyboard is needed to observe nonzero insets." },
		AccessibilityNotes = new[] { "The software-keyboard example uses a labeled text field and keeps focused content within the visible bounds." },
		ResetBehavior = "Dismiss the software keyboard or navigate away to restore the initial layout.",
		Variants = new[] { "All-edge padding", "VisibleBounds shorthand", "Margin mode", "SoftInput keyboard avoidance" },
		KnownLimitations = new[] { "Desktop targets normally report zero safe-area insets, so the examples appear flat there." },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "drawer" },
		SortOrder = 50)]
	public sealed partial class SafeAreaSamplePage : Page
	{
		public SafeAreaSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
