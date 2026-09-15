using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "DropDownButton",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/buttons#create-a-drop-down-button",
		Slug = "drop-down-button",
		Tags = new[] { "button", "input", "command", "flyout", "dropdown" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Fluent,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
		AccessibilityNotes = new[] { "Each enabled button and menu item supports keyboard focus and exposes its visible action label." },
		ResetBehavior = "Dismiss the menu flyout; selections do not persist outside the open menu.",
		Variants = new[] { "Text button", "Icon button", "Disabled button" },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "button", "split-button", "flyout" })]
	public sealed partial class DropDownButtonSamplePage : Page
	{
		private const string Description =
			"DropDownButton is a button that shows a chevron to indicate a flyout opens when clicked. " +
			"Unlike SplitButton, the entire button surface is a single clickable target that opens the flyout.";

		public DropDownButtonSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
