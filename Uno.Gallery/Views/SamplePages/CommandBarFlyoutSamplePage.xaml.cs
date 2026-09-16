using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "CommandBarFlyout",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/command-bar-flyout",
		Slug = "command-bar-flyout",
		Tags = new[] { "command", "flyout", "appbar", "context", "toolbar" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Fluent,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
		AccessibilityNotes = new[] { "Launch buttons and flyout commands are keyboard accessible; text-context commands also open from the platform context gesture." },
		ResetBehavior = "Dismiss the flyout to reset its transient open state.",
		Variants = new[] { "Compact command bar", "Always-expanded command bar", "Text context command bar" },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "commandbar", "flyout", "split-button" })]
	public sealed partial class CommandBarFlyoutSamplePage : Page
	{
		private const string Description =
			"CommandBarFlyout is a specialized flyout that displays AppBarButton primary commands " +
			"as compact icon buttons, with optional secondary commands in an overflow menu. " +
			"It is ideal for context menus on selected content.";

		public CommandBarFlyoutSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
