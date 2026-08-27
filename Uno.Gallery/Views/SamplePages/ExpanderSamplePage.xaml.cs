using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "Expander",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/expander",
		Slug = "expander",
		Tags = new[] { "layout", "disclosure", "toggle", "container" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Fluent,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
		AccessibilityNotes = new[] { "Enabled headers are keyboard-focusable and expose expanded or collapsed state; the disabled variant cannot be invoked." },
		ResetBehavior = SampleContractDefaults.ReloadToReset,
		Variants = new[] { "Expanded and collapsed", "Custom header", "Expand upward", "Disabled" },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26")]
	public sealed partial class ExpanderSamplePage : Page
	{
		private const string Description =
			"Expander is a control that shows or hides supplementary content associated with a header. " +
			"It supports custom headers, up/down expand direction, and disabled state.";

		public ExpanderSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
