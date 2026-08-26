using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Uno.Gallery.Entities.Data;
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
	Status = SampleStatus.Stable)]
public sealed partial class ItemsRepeaterSamplePage : Page
{
	private readonly LocalIncrementalSource _incrementalItems = new();

	public ItemsRepeaterSamplePage()
	{
		this.InitializeComponent();
		_incrementalItems.BatchLoaded += OnBatchLoaded;
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
		var repeater = SamplePageLayoutRoot.GetSampleChild<ItemsRepeater>(Design.Fluent, "SelectionRepeater");
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Fluent, "SelectionStatus");
		if (repeater is null || status is null || repeater.ItemsSourceView.Count == 0)
		{
			return;
		}

		var next = (ItemsRepeaterExtensions.GetSelectedIndex(repeater) + 1) % repeater.ItemsSourceView.Count;
		ItemsRepeaterExtensions.SetSelectedIndex(repeater, next);
		status.Text = $"Selected index: {ItemsRepeaterExtensions.GetSelectedIndex(repeater)}";
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
				status.Text = $"Items loaded: {_incrementalItems.Count}; batches: {_incrementalItems.BatchCount}";
			}
		});
	}

	private sealed class LocalIncrementalSource : ObservableCollection<string>, ISupportIncrementalLoading
	{
		private const int MaximumItems = 60;

		public LocalIncrementalSource()
		{
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
			var batchSize = Math.Min(Math.Max(1, (int)requestedCount), Math.Min(6, MaximumItems - Count));
			for (var index = 0; index < batchSize; index++)
			{
				Add($"Offline item {Count + 1}");
			}

			BatchCount++;
			BatchLoaded?.Invoke(this, EventArgs.Empty);
			return Task.FromResult(new LoadMoreItemsResult { Count = (uint)batchSize });
		}
	}
}
