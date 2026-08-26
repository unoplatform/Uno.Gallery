using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "SplitButton",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/buttons#create-a-split-button",
		Slug = "split-button",
		Tags = new[] { "button", "input", "command", "flyout" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "Button", "DropDownButton", "CommandBarFlyout" })]
	public sealed partial class SplitButtonSamplePage : Page
	{
		private const string Description =
			"SplitButton has two parts: a primary button that executes a command and a secondary " +
			"button that opens a flyout with additional options. ToggleSplitButton adds an on/off " +
			"checked state to the primary part.";

		public SplitButtonSamplePage()
		{
			this.InitializeComponent();
		}

		private void OnSplitButtonPrimaryClick(SplitButton sender, SplitButtonClickEventArgs e)
		{
			// x:Name inside DataTemplate is scoped to the template; use FindName on the parent panel.
			if (sender.Parent is FrameworkElement parent &&
				parent.FindName("SplitButtonResult") is TextBlock result)
			{
				result.Text = "Primary action: New document";
			}
		}

		private void OnToggleSplitButtonCheckedChanged(ToggleSplitButton sender, ToggleSplitButtonIsCheckedChangedEventArgs e)
		{
			if (sender.Parent is FrameworkElement parent &&
				parent.FindName("ToggleSplitButtonResult") is TextBlock result)
			{
				result.Text = $"Toggle state: {(sender.IsChecked ? "On" : "Off")}";
			}
		}
	}
}
