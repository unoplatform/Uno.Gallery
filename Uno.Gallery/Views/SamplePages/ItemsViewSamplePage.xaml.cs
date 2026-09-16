using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Entities.Data;

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
		// GetSampleChild is more reliable than FindName across XamlDisplay namescope boundaries.
		var display = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "SelectedTitle_Single");
		if (display is null) return;
		var title = (sender.SelectedItem as GalleryItem)?.Title ?? "(none)";
		display.Text = $"Selected: {title}";
	}

	private void ItemsViewMultiple_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
	{
		var display = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "MultiSelectionCountDisplay");
		if (display is not null)
			display.Text = $"Selected count: {sender.SelectedItems.Count}";
	}

	private void SelectFirst_Click(object sender, RoutedEventArgs e)
	{
		var iv = SamplePageLayoutRoot.GetSampleChild<ItemsView>(Design.Fluent, "ItemsViewSingle");
		if (iv is null) return;
		iv.Select(0);
		// Belt-and-suspenders: update display directly in case SelectionChanged hasn't propagated yet.
		var display = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "SelectedTitle_Single");
		if (display is not null)
			display.Text = $"Selected: {(iv.SelectedItem as GalleryItem)?.Title ?? "(none)"}";
	}

	private void SelectFirstTwo_Click(object sender, RoutedEventArgs e)
	{
		var iv = SamplePageLayoutRoot.GetSampleChild<ItemsView>(Design.Fluent, "ItemsViewMultiple");
		if (iv is null) return;
		iv.Select(0);
		iv.Select(1);
		var display = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "MultiSelectionCountDisplay");
		if (display is not null)
			display.Text = $"Selected count: {iv.SelectedItems.Count}";
	}
}
