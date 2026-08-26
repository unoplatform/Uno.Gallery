using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "ScrollView",
	Description = "The WinUI 3 replacement for ScrollViewer. Exposes a ScrollPresenter for direct access to scroll state and inertia. Supports vertical, horizontal, and zoom modes.",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.scrollview",
	Tags = new[] { "scroll", "scrollpresenter", "layout", "content" },
	RelatedSamples = new[] { "itemsrepeater", "itemsview" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-26")]
public sealed partial class ScrollViewSamplePage : Page
{
	public ScrollViewSamplePage()
	{
		this.InitializeComponent();
	}

	private void ReadOffset_Click(object sender, RoutedEventArgs e)
	{
		// Named elements inside a DataTemplate share one namescope instance.
		// FindName on any element within the instantiated template finds siblings by name.
		var btn = (FrameworkElement)sender;
		var sv = btn.FindName("VerticalScrollView") as ScrollView;
		var display = btn.FindName("OffsetDisplay") as TextBlock;
		if (sv is not null && display is not null)
		{
			var offset = sv.ScrollPresenter?.VerticalOffset ?? 0;
			display.Text = "Vertical offset: " + offset.ToString("F1", CultureInfo.InvariantCulture);
		}
	}
}
