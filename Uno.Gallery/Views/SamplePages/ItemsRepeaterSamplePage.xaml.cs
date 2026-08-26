using Microsoft.UI.Xaml;
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

	private void ScrollStackDown_Click(object sender, RoutedEventArgs e)
	{
		var sv = SamplePageLayoutRoot.GetSampleChild<ScrollViewer>(Design.Fluent, "StackScrollViewer");
		if (sv is null) return;
		sv.ChangeView(null, 100.0, null, disableAnimation: true);
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "StackScrollStatus");
		if (status is not null)
			status.Text = $"Offset: {sv.VerticalOffset:F1}";
	}
}
