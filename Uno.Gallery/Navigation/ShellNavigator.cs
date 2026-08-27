using System.Reflection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Uno.Gallery.Helpers;
using Uno.Gallery.Views.GeneralPages;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery;

/// <summary>
/// Per-<see cref="Shell"/> implementation of <see cref="IGalleryNavigator"/>.
/// All methods must be called from the UI thread.
/// </summary>
internal sealed class ShellNavigator : IGalleryNavigator
{
	private readonly Shell _shell;

	public ShellNavigator(Shell shell)
	{
		_shell = shell ?? throw new ArgumentNullException(nameof(shell));
	}

	/// <inheritdoc/>
	public Sample? Current { get; private set; }

	/// <inheritdoc/>
	public event EventHandler<SampleNavigatedEventArgs>? Navigated;

	/// <inheritdoc/>
	public Sample? FindBySlug(string slug)
		=> _shell.Samples.FirstOrDefault(s =>
			string.Equals(s.Slug, slug, StringComparison.OrdinalIgnoreCase));

	/// <inheritdoc/>
	public Sample? FindByTitle(string title)
		=> _shell.Samples.FirstOrDefault(s =>
			string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));

	/// <inheritdoc/>
	public bool NavigateToSlug(string slug, NavigationOptions options = NavigationOptions.None)
	{
		var sample = FindBySlug(slug);
		if (sample is null) return false;
		NavigateTo(sample, options);
		return true;
	}

	/// <inheritdoc/>
	public void NavigateToOverview(NavigationOptions options = NavigationOptions.None)
	{
		var overview = _shell.Samples.FirstOrDefault(s => s.ViewType == typeof(OverviewPage));
		if (overview is null)
		{
			// Fallback: OverviewPage filtered from catalog in unusual build configurations.
			var attr = typeof(OverviewPage).GetCustomAttribute<SamplePageAttribute>()
				?? throw new InvalidOperationException("OverviewPage is missing SamplePageAttribute.");
			overview = new Sample(attr, typeof(OverviewPage));
		}
		NavigateTo(overview, options);
	}

	/// <inheritdoc/>
	public void NavigateTo(Sample sample, NavigationOptions options = NavigationOptions.None)
	{
		// Canonicalize: prefer the registered catalog instance for correct Data identity/metadata.
		// Falls back to the incoming instance for legacy or unregistered samples (Canary/Overview fallback).
		var canonical = _shell.Samples.FirstOrDefault(s => s.ViewType == sample.ViewType) ?? sample;

		var nv = _shell.NavigationView;

		// No-op: same page type is already displayed.
		if (nv.Content?.GetType() == canonical.ViewType)
		{
			return;
		}

		if (!options.HasFlag(NavigationOptions.SkipNavSync))
		{
			if (options.HasFlag(NavigationOptions.ExpandCategory))
			{
				// Search path: find nested item, expand parent category, then update layout.
				MUXC.NavigationViewItem? selectedItem = null;
				MUXC.NavigationViewItem? selectedCategory = null;

				foreach (MUXC.NavigationViewItem category in nv.MenuItems)
				{
					selectedItem = category.MenuItems
						.OfType<MUXC.NavigationViewItem>()
						.FirstOrDefault(item => item.DataContext is Sample s && s.ViewType == canonical.ViewType);

					if (selectedItem != null)
					{
						selectedCategory = category;
						break;
					}
				}

				if (selectedItem is null)
				{
					nv.SelectedItem = nv.MenuItems[0];
				}
				else
				{
					selectedCategory!.IsExpanded = true;
					nv.UpdateLayout();
					nv.SelectedItem = selectedItem;
				}
			}
			else
			{
				// Default path: top-level lookup only (covers SampleCategory.None items such as Overview).
				var selected = nv.MenuItems
					.OfType<MUXC.NavigationViewItem>()
					.FirstOrDefault(x => (x.DataContext as Sample)?.ViewType == canonical.ViewType);
				if (selected != null)
				{
					nv.SelectedItem = selected;
				}
			}
		}

		var page = canonical.CreatePage();
		page.DataContext = canonical;
#if VISUAL_REGRESSION
		page.Loaded += (_, _) =>
		{
			var renderedFrames = 0;
			EventHandler<object>? rendering = null;
			rendering = (_, _) =>
			{
				if (++renderedFrames < 4)
				{
					return;
				}
				CompositionTarget.Rendering -= rendering;
				PerformanceMarks.Record(PerformanceMarks.VisualReady);
			};
			CompositionTarget.Rendering += rendering;
		};
#endif
#if USE_UITESTS
		page.Loaded += (_, _) =>
		{
			if (VisualTreeHelperEx.GetFirstDescendant<SamplePageLayout>(page) is null)
			{
				_shell.UITestSampleHostLoadedState = canonical.Slug + "\n" + Design.Agnostic;
			}
		};
#endif

#if __WASM__
#if !VISUAL_REGRESSION
		_ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(DispatcherQueuePriority.Low,
			() => AnalyticsService.TrackView(canonical.Title ?? page.GetType().Name));
#endif
#endif

		var previous = Current;
		Current = canonical;
		nv.Content = page;
		Navigated?.Invoke(this, new SampleNavigatedEventArgs(previous, Current));

#if __WASM__
		if (!options.HasFlag(NavigationOptions.SkipHistory))
		{
			var slug = canonical.Slug; // "overview" for OverviewPage
			var design = SamplePageLayout.CurrentDesign.ToString();
			Wasm.BrowserHistoryHandler.PushState(slug, design);
		}
#endif
	}
}
