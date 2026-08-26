using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "SelectorBar",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/selector-bar",
		Slug = "selector-bar",
		Tags = new[] { "navigation", "selection", "tab", "filter" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "TabBar", "SegmentedControl" })]
	public sealed partial class SelectorBarSamplePage : Page
	{
		private const string Description =
			"SelectorBar provides a row of mutually exclusive SelectorBarItem elements that navigate " +
			"or filter content. Each item can display text, an icon, or both.";

		public SelectorBarSamplePage()
		{
			this.InitializeComponent();
		}

		private void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs e)
		{
			if (sender.SelectedItem is SelectorBarItem item &&
				sender.Parent is FrameworkElement parent &&
				parent.FindName("SelectorBarResult") is TextBlock result)
			{
				result.Text = $"Selected: {item.Text}";
			}
		}
	}
}
