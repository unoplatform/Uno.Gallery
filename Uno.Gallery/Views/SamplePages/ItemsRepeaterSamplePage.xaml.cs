using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Uno.Gallery.Entities.Data;
using Uno.Gallery.Helpers;
using Uno.Toolkit.UI;
using Windows.Foundation;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "ItemsRepeater",
	Description = "A data-driven layout panel with pluggable layouts. Toolkit ItemsRepeaterExtensions composes selector-style state and viewport-driven incremental loading without replacing ItemsRepeater.",
	DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.itemsrepeater",
	DataType = typeof(GalleryItemCollection),
	Tags = new[] { "collection", "layout", "repeater", "virtualization", "selection", "incremental-loading" },
	RelatedSamples = new[] { "listview", "gridview", "itemsview" },
	Owner = "t-dotitl",
	ReviewedOn = "2026-08-27",
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Fluent,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
	AccessibilityNotes = new[] { "Selection controls expose the selected index, focus can be moved explicitly, and loading progress is reported as text." },
	ResetBehavior = SampleContractDefaults.ReloadToReset,
	Variants = new[] { "Vertical StackLayout", "UniformGridLayout", "Toolkit single selection", "Incremental loading" })]
public sealed partial class ItemsRepeaterSamplePage : Page
{
	private readonly LocalIncrementalSource _incrementalItems;
	private bool _batchHandlerAttached;

	public ItemsRepeaterSamplePage()
	{
		this.InitializeComponent();
		_incrementalItems = new LocalIncrementalSource(
			DispatcherQueue ?? throw new InvalidOperationException("ItemsRepeater sample requires a UI DispatcherQueue."));
		Loaded += OnPageLoaded;
		Unloaded += OnPageUnloaded;
	}

	private void OnPageLoaded(object sender, RoutedEventArgs e)
	{
		if (!_batchHandlerAttached)
		{
			_incrementalItems.BatchLoaded += OnBatchLoaded;
			_batchHandlerAttached = true;
		}
	}

	private void OnPageUnloaded(object sender, RoutedEventArgs e)
	{
		if (_batchHandlerAttached)
		{
			_incrementalItems.BatchLoaded -= OnBatchLoaded;
			_batchHandlerAttached = false;
		}
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

	private void SelectNext_Click(object sender, RoutedEventArgs e)
	{
		var repeater = GetRequiredChild<ItemsRepeater>("SelectionRepeater");
		if (repeater.ItemsSourceView.Count == 0)
		{
			throw new InvalidOperationException("The selection repeater has no items.");
		}

		var next = (ItemsRepeaterExtensions.GetSelectedIndex(repeater) + 1) % repeater.ItemsSourceView.Count;
		ItemsRepeaterExtensions.SetSelectedIndex(repeater, next);
		UpdateSelectionStatus(repeater);
	}

	private void SelectionRepeater_Loaded(object sender, RoutedEventArgs e)
		=> DispatcherQueue.TryEnqueue(() => SynchronizeSelectionVisuals((ItemsRepeater)sender));

	private void SelectionItem_Click(object sender, RoutedEventArgs e)
	{
		var repeater = GetRequiredChild<ItemsRepeater>("SelectionRepeater");
		var index = repeater.GetElementIndex((UIElement)sender);
		if (index < 0)
		{
			throw new InvalidOperationException("The activated selection item is not realized by the repeater.");
		}
		ItemsRepeaterExtensions.SetSelectedIndex(repeater, index);
		DispatcherQueue.TryEnqueue(() => UpdateSelectionStatus(repeater));
	}

	private void FocusSecondSelectionItem_Click(object sender, RoutedEventArgs e)
	{
		var repeater = GetRequiredChild<ItemsRepeater>("SelectionRepeater");
		if (repeater.TryGetElement(1) is not Control item || !item.Focus(FocusState.Programmatic))
		{
			throw new InvalidOperationException("The second selection item could not receive keyboard focus.");
		}
	}

	private void UpdateSelectionStatus(ItemsRepeater repeater)
	{
		SynchronizeSelectionVisuals(repeater);
		AccessibilityHelper.Announce(
			GetRequiredChild<TextBlock>("SelectionStatus"),
			$"Selected index: {ItemsRepeaterExtensions.GetSelectedIndex(repeater)}");
	}

	private static void SynchronizeSelectionVisuals(ItemsRepeater repeater)
	{
		var selectedIndex = ItemsRepeaterExtensions.GetSelectedIndex(repeater);
		for (var index = 0; index < repeater.ItemsSourceView.Count; index++)
		{
			if (repeater.TryGetElement(index) is ToggleButton item)
			{
				item.IsChecked = index == selectedIndex;
			}
		}
	}

	private void IncrementalRepeater_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is ItemsRepeater repeater)
		{
			repeater.ItemsSource = _incrementalItems;
		}
	}

	private void LoadNextBatch_Click(object sender, RoutedEventArgs e)
	{
		var scrollViewer = SamplePageLayoutRoot.GetSampleChild<ScrollViewer>(Design.Fluent, "IncrementalScrollViewer");
		if (scrollViewer is null)
		{
			return;
		}

		scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null, disableAnimation: true);
	}

	private void OnBatchLoaded(object? sender, EventArgs e)
	{
		DispatcherQueue.TryEnqueue(() =>
		{
			var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "IncrementalStatus");
			if (status is not null)
			{
				AccessibilityHelper.Announce(
					status,
					$"Items loaded: {_incrementalItems.Count}; batches: {_incrementalItems.BatchCount}");
			}
		});
	}

	private T GetRequiredChild<T>(string name) where T : FrameworkElement
		=> SamplePageLayoutRoot.GetSampleChild<T>(Design.Fluent, name)
			?? throw new InvalidOperationException($"ItemsRepeater sample child '{name}' is not loaded.");

	private sealed class LocalIncrementalSource : ObservableCollection<string>, ISupportIncrementalLoading
	{
		private const int MaximumItems = 60;
		private readonly DispatcherQueue _dispatcherQueue;

		public LocalIncrementalSource(DispatcherQueue dispatcherQueue)
		{
			_dispatcherQueue = dispatcherQueue;
			for (var index = 1; index <= 12; index++)
			{
				Add($"Offline item {index}");
			}
		}

		public event EventHandler? BatchLoaded;

		public int BatchCount { get; private set; }

		public bool HasMoreItems => Count < MaximumItems;

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
			=> AsyncInfo.Run(cancellationToken => LoadMoreItemsCoreAsync(count, cancellationToken));

		private Task<LoadMoreItemsResult> LoadMoreItemsCoreAsync(uint requestedCount, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var completion = new TaskCompletionSource<LoadMoreItemsResult>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			if (!_dispatcherQueue.TryEnqueue(() =>
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					var batchSize = Math.Min(Math.Max(1, (int)requestedCount), Math.Min(6, MaximumItems - Count));
					for (var index = 0; index < batchSize; index++)
					{
						Add($"Offline item {Count + 1}");
					}

					BatchCount++;
					BatchLoaded?.Invoke(this, EventArgs.Empty);
					completion.TrySetResult(new LoadMoreItemsResult { Count = (uint)batchSize });
				}
				catch (OperationCanceledException)
				{
					completion.TrySetCanceled(cancellationToken);
				}
				catch (Exception error)
				{
					completion.TrySetException(error);
				}
			}))
			{
				throw new InvalidOperationException("Unable to enqueue incremental loading on the UI thread.");
			}

			return completion.Task;
		}
	}
}
