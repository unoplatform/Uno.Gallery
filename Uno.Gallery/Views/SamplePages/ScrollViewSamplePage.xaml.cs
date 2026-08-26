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
	private ScrollView? _verticalScrollView;

	public ScrollViewSamplePage()
	{
		this.InitializeComponent();
	}

	private void VerticalScrollView_Loaded(object sender, RoutedEventArgs e)
		=> _verticalScrollView = (ScrollView)sender;

	private void VerticalScrollView_ViewChanged(ScrollView sender, object args)
		=> UpdateOffsetDisplay(sender);

	private void ScrollDown_Click(object sender, RoutedEventArgs e)
	{
		var display = GetOffsetDisplay();
		if (_verticalScrollView is not { } scrollView)
		{
			if (display is not null)
				display.Text = "Vertical scroll view is not loaded.";
			return;
		}

		scrollView.UpdateLayout();
		scrollView.ScrollTo(
			0,
			200,
			new ScrollingScrollOptions(ScrollingAnimationMode.Disabled, ScrollingSnapPointsMode.Ignore));
	}

	private void ReadOffset_Click(object sender, RoutedEventArgs e)
	{
		if (_verticalScrollView is { } scrollView)
			UpdateOffsetDisplay(scrollView);
	}

	private void UpdateOffsetDisplay(ScrollView scrollView)
	{
		if (GetOffsetDisplay() is { } display)
			display.Text = "Vertical offset: " +
				(scrollView.ScrollPresenter?.VerticalOffset ?? 0).ToString("F1", CultureInfo.InvariantCulture);
	}

	private TextBlock? GetOffsetDisplay()
		=> SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "OffsetDisplay");
}
