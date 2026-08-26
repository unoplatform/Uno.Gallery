using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Uno.Gallery.Entities.Data;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "ItemsView",
	Description = "The modern WinUI 3 collection control with pluggable layouts. Uses ItemContainer to wrap each item, providing built-in selection, keyboard navigation, and accessible state.",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.itemsview",
	DataType = typeof(GalleryItemCollection),
	Tags = new[] { "collection", "selection", "itemcontainer", "layout" },
	RelatedSamples = new[] { "listview", "gridview", "itemsrepeater" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-26")]
public sealed partial class ItemsViewSamplePage : Page
{
	public ItemsViewSamplePage()
	{
		this.InitializeComponent();
	}

	private void ItemsViewSingle_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
	{
		var display = sender.FindName("SelectedTitle_Single") as TextBlock;
		if (display is null) return;
		var title = (sender.SelectedItem as GalleryItem)?.Title ?? "(none)";
		display.Text = $"Selected: {title}";
	}

	private void ItemsViewMultiple_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
	{
		var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(sender) as DependencyObject;
		if (parent is null) return;
		var display = VisualTreeHelperEx.GetFirstDescendant<TextBlock>(parent,
			t => AutomationProperties.GetAutomationId(t) == "ItemsView_MultiSelectionCount");
		if (display is not null)
			display.Text = $"Selected count: {sender.SelectedItems.Count}";
	}
}
