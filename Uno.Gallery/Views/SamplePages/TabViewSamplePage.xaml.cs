using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "TabView",
	Description = "A WinUI 3 tabbed document interface control. Supports closable tabs, an add-tab button, and tab-width modes (Equal, SizeToContent, Compact).",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.tabview",
	Tags = new[] { "navigation", "tabs", "tabview" },
	RelatedSamples = new[] { "NavigationView", "TabBar" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-26")]
public sealed partial class TabViewSamplePage : Page
{
	private int _nextTabIndex = 4;

	public TabViewSamplePage()
	{
		this.InitializeComponent();
	}

	private void DynamicTabView_AddTabButtonClick(TabView sender, object e)
	{
		var item = new TabViewItem
		{
			Header = $"Tab {_nextTabIndex++}",
			Content = new TextBlock { Text = "New tab content.", Margin = new Thickness(16) }
		};
		sender.TabItems.Add(item);
		sender.SelectedItem = item;
	}

	private void DynamicTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs e)
	{
		sender.TabItems.Remove(e.Tab);
	}
}
