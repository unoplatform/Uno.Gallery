using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Entities.Data;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "ItemsView",
	Description = "The modern WinUI 3 collection control with pluggable layouts. Uses ItemContainer to wrap each item, providing built-in selection, keyboard navigation, and accessible state.",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.itemsview",
	DataType = typeof(GalleryItemCollection),
	Tags = new[] { "collection", "selection", "itemcontainer", "layout" },
	RelatedSamples = new[] { "ListView", "GridView", "ItemsRepeater" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-26")]
public sealed partial class ItemsViewSamplePage : Page
{
	public ItemsViewSamplePage()
	{
		this.InitializeComponent();
	}
}
