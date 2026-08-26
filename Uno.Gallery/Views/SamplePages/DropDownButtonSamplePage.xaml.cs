using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "DropDownButton",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/buttons#create-a-drop-down-button",
		Slug = "drop-down-button",
		Tags = new[] { "button", "input", "command", "flyout", "dropdown" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "Button", "SplitButton", "Flyout" })]
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
