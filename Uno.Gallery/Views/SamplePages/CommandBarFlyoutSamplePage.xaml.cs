using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "CommandBarFlyout",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/command-bar-flyout",
		Slug = "command-bar-flyout",
		Tags = new[] { "command", "flyout", "appbar", "context", "toolbar" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "CommandBar", "Flyout", "SplitButton" })]
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
