using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "TabView",
	Description = "A WinUI 3 tabbed document interface control. Supports closable tabs, an add-tab button, and tab-width modes (Equal, SizeToContent, Compact).",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.tabview",
	Tags = new[] { "navigation", "tabs", "tabview" },
	RelatedSamples = new[] { "navigationview", "tabbar" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-26")]
public sealed partial class TabViewSamplePage : Page
{
	private int _nextTabIndex = 4;

	public TabViewSamplePage()
	{
		this.InitializeComponent();
	}

	private void DynamicTabView_Loaded(object sender, RoutedEventArgs e)
	{
		var tabView = (TabView)sender;

		// Expose a stable AutomationId on the add-tab button (template part "AddButton").
		var addBtn = VisualTreeHelperEx.GetFirstDescendant<ButtonBase>(tabView,
			b => (b as FrameworkElement)?.Name == "AddButton");
		if (addBtn is not null)
			AutomationProperties.SetAutomationId(addBtn, "TabView_AddTabButton");

		// Tag close buttons on initial closable tabs.
		foreach (var item in tabView.TabItems.OfType<TabViewItem>())
			TagCloseButton(item);
	}

	private void DynamicTabView_AddTabButtonClick(TabView sender, object e)
	{
		var idx = _nextTabIndex++;
		var item = new TabViewItem
		{
			Header = $"Tab {idx}",
			Content = new TextBlock { Text = "New tab content.", Margin = new Thickness(16) }
		};
		AutomationProperties.SetAutomationId(item, $"TabView_Dynamic_Tab{idx}");
		// Tag the close button once the new item loads its template.
		item.Loaded += (s, _) => TagCloseButton((TabViewItem)s);
		sender.TabItems.Add(item);
		sender.SelectedItem = item;
		UpdateTabCount(sender);
	}

	private void DynamicTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs e)
	{
		sender.TabItems.Remove(e.Tab);
		UpdateTabCount(sender);
	}

	/// <summary>
	/// Sets a stable AutomationId on the close button of a closable tab item.
	/// The button has template-part name "CloseButton" in the WinUI3 TabViewItem template.
	/// </summary>
	private static void TagCloseButton(TabViewItem item)
	{
		if (!item.IsClosable) return;
		var id = AutomationProperties.GetAutomationId(item);
		if (string.IsNullOrEmpty(id)) return;
		var closeBtn = VisualTreeHelperEx.GetFirstDescendant<Button>(item,
			b => (b as FrameworkElement)?.Name == "CloseButton");
		if (closeBtn is not null)
			AutomationProperties.SetAutomationId(closeBtn, id + "_Close");
	}

	/// <summary>
	/// Updates the count display label (AutomationId = TabView_Dynamic_TabCount) 
	/// by navigating up from the TabView to its parent container.
	/// </summary>
	private static void UpdateTabCount(TabView tabView)
	{
		var parent = VisualTreeHelper.GetParent(tabView) as DependencyObject;
		if (parent is null) return;
		var display = VisualTreeHelperEx.GetFirstDescendant<TextBlock>(parent,
			t => AutomationProperties.GetAutomationId(t) == "TabView_Dynamic_TabCount");
		if (display is not null)
			display.Text = tabView.TabItems.Count.ToString();
	}
}
