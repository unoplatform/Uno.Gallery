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
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Fluent,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
		AccessibilityNotes = new[] { "Selector items and the Select Unread action are keyboard accessible, and the current selection is repeated as text." },
		ResetBehavior = SampleContractDefaults.ReloadToReset,
		Variants = new[] { "Text-only items", "Icon and text items", "Programmatic selection" },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "tabbar", "segmentedcontrol" })]
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
			if (sender.SelectedItem is SelectorBarItem item)
			{
				// FindName does not cross XamlDisplay/DataTemplate scope boundaries.
				// Use GetSampleChild which walks the visual tree from the ContentPresenter.
				var result = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "SelectorBarResult");
				if (result is not null)
					result.Text = $"Selected: {item.Text}";
			}
		}

		private void SelectUnread_Click(object sender, RoutedEventArgs e)
		{
			var bar = SamplePageLayoutRoot.GetSampleChild<SelectorBar>(Design.Fluent, "SelectorBarDefault");
			if (bar is not null && bar.Items.Count > 1)
				bar.SelectedItem = bar.Items[1]; // Index 1 = "Unread"
		}
	}
}
