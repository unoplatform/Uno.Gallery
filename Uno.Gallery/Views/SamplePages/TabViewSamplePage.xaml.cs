using System.Linq;
using Microsoft.UI.Dispatching;
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

		// Try to tag the add-button immediately; template parts are usually available by Loaded.
		// If not (deferred/lazy template apply), retry at low priority on the dispatcher queue.
		if (!TryTagAddButton(tabView))
		{
			tabView.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
			{
				if (!TryTagAddButton(tabView))
					System.Diagnostics.Debug.WriteLine(
						"[TabViewSamplePage] AddButton template part not found after deferral; " +
						"AutomationId 'TabView_AddTabButton' was not set. " +
						"Expected part name: 'AddButton' or 'PART_AddButton'.");
			});
		}

		// Tag close buttons on initial closable tabs.
		// At TabView.Loaded the item visual tree may still be pending; use a low-priority
		// dispatcher retry rather than item.Loaded (which fires only once and may already
		// have been missed if the template was applied before this handler ran).
		foreach (var item in tabView.TabItems.OfType<TabViewItem>())
		{
			if (!TagCloseButton(item))
			{
				var capturedItem = item;
				tabView.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
				{
					if (!TagCloseButton(capturedItem))
						System.Diagnostics.Debug.WriteLine(
							$"[TabViewSamplePage] CloseButton template part not found after deferral on initial tab '{AutomationProperties.GetAutomationId(capturedItem)}'; " +
							$"AutomationId '_Close' suffix was not set.");
				});
			}
		}
	}

	private static bool TryTagAddButton(TabView tabView)
	{
		var addBtn = VisualTreeHelperEx.GetFirstDescendant<ButtonBase>(tabView,
			b => b is FrameworkElement fe && (fe.Name is "AddButton" or "PART_AddButton"));
		if (addBtn is null) return false;
		AutomationProperties.SetAutomationId(addBtn, "TabView_AddTabButton");
		return true;
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
	/// Returns true when the button was found and tagged, or when no tagging is needed.
	/// Returns false when the tab is closable but the template part is not yet in the visual tree.
	/// </summary>
	private static bool TagCloseButton(TabViewItem item)
	{
		if (!item.IsClosable) return true;
		var id = AutomationProperties.GetAutomationId(item);
		if (string.IsNullOrEmpty(id)) return true;
		var closeBtn = VisualTreeHelperEx.GetFirstDescendant<Button>(item,
			b => b is FrameworkElement fe && (fe.Name is "CloseButton" or "PART_CloseButton"));
		if (closeBtn is null)
		{
			System.Diagnostics.Debug.WriteLine(
				$"[TabViewSamplePage] CloseButton template part not found on {id}; " +
				$"AutomationId '{id}_Close' was not set. " +
				"Expected part name: 'CloseButton' or 'PART_CloseButton'.");
			return false;
		}
		AutomationProperties.SetAutomationId(closeBtn, id + "_Close");
		return true;
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
