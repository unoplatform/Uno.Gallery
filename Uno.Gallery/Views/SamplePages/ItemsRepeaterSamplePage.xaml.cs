using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Entities.Data;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "ItemsRepeater",
	Description = "A data-driven layout panel with pluggable layouts (StackLayout, UniformGridLayout). Unlike ListView/GridView it has no selection or scrolling built-in — compose with ScrollViewer.",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.itemsrepeater",
	DataType = typeof(GalleryItemCollection),
	Tags = new[] { "collection", "layout", "repeater", "virtualization" },
	RelatedSamples = new[] { "listview", "gridview", "itemsview" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-26")]
public sealed partial class ItemsRepeaterSamplePage : Page
{
	public ItemsRepeaterSamplePage()
	{
		this.InitializeComponent();
	}
}
