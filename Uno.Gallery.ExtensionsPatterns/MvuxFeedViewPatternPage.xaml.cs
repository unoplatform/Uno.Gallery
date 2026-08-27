using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions;
using Uno.Extensions.Reactive;
using Uno.Gallery.ExtensionsPatterns.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "MVUX FeedView", SourceSdk.UnoExtensions,
	Description = "Offline deterministic Feed and FeedView states: loading, data, empty, error, and refresh.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Mvux/Overview.html",
	Slug = "extensions-mvux-feedview",
	Tags = new[] { "extensions", "mvux", "feedview", "offline", "optional-flavor" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Build with EnableExtensionsPatterns=true. All feed data and delays are local and deterministic." },
	AccessibilityNotes = new[] { "Named scenario buttons are keyboard reachable and FeedView state changes are announced through a polite text status." },
	ResetBehavior = "Choose Reset to recreate the local Data feed and clear the refresh counter.",
	Variants = new[] { "Loading", "Data", "Empty", "Error", "Refresh" },
	KnownLimitations = new[] { "This contained sample does not demonstrate remote feeds, authentication, or live services." },
	SourceRepositoryPath = "Uno.Gallery.ExtensionsPatterns/MvuxFeedViewPatternPage.xaml.cs",
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "extensions-localization", "extensions-storage", "extensions-configuration", "extensions-validation" })]
public sealed partial class MvuxFeedViewPatternPage : Page
{
	private readonly FeedScenarioController _controller = new();

	public MvuxFeedViewPatternPage()
	{
		InitializeComponent();
		Show(FeedScenario.Data);
	}

	private IFeed<PatternFeedResult> CreateFeed()
		=> Feed<PatternFeedResult>.Async(
			new Uno.Extensions.AsyncFunc<Uno.Extensions.Option<PatternFeedResult>>(LoadAsync));

	private async ValueTask<Uno.Extensions.Option<PatternFeedResult>> LoadAsync(CancellationToken cancellationToken)
	{
		if (_controller.Scenario == FeedScenario.Loading)
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
		}

		await Task.Delay(_controller.Scenario == FeedScenario.Refresh ? 500 : 120, cancellationToken);
		var result = _controller.CreateResult();
		if (_controller.Scenario == FeedScenario.Refresh)
		{
			DispatcherQueue.TryEnqueue(() =>
				AccessibilityHelper.Announce(
					ScenarioStatus,
					$"Scenario: Refresh; completed: {_controller.RefreshCount}"));
		}
		return _controller.Scenario == FeedScenario.Empty
			? Uno.Extensions.Option.None<PatternFeedResult>()
			: Uno.Extensions.Option.Some(result);
	}

	private void Show(FeedScenario scenario)
	{
		_controller.Select(scenario);
		AccessibilityHelper.Announce(ScenarioStatus, $"Scenario: {scenario}");
		FeedHost.Source = CreateFeed();
	}

	private void Loading_Click(object sender, RoutedEventArgs e) => Show(FeedScenario.Loading);
	private void Data_Click(object sender, RoutedEventArgs e) => Show(FeedScenario.Data);
	private void Empty_Click(object sender, RoutedEventArgs e) => Show(FeedScenario.Empty);
	private void Error_Click(object sender, RoutedEventArgs e) => Show(FeedScenario.Error);
	private void Refresh_Click(object sender, RoutedEventArgs e)
	{
		_controller.Select(FeedScenario.Refresh);
		AccessibilityHelper.Announce(ScenarioStatus, "Scenario: Refresh; requested through FeedView.Refresh");
		if (!FeedHost.Refresh.CanExecute(null))
		{
			AccessibilityHelper.Announce(ScenarioStatus, "Scenario: Refresh; a refresh is already running.");
			return;
		}
		FeedHost.Refresh.Execute(null);
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		_controller.Reset();
		Show(FeedScenario.Data);
	}
}
