using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions;
using Uno.Extensions.Hosting;
using Uno.Extensions.Storage.KeyValueStorage;
using Uno.Gallery.ExtensionsPatterns.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Storage", SourceSdk.UnoExtensions,
	Description = "Local IKeyValueStorage save, load, clear, reset, and visible error handling.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Storage/StorageOverview.html",
	Slug = "extensions-storage",
	Tags = new[] { "extensions", "storage", "settings", "offline", "optional-flavor" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Build with EnableExtensionsPatterns=true. Uses the named ApplicationData key-value provider with no credentials or remote storage." },
	AccessibilityNotes = new[] { "The note field is labeled, operations are keyboard reachable, and results and failures are announced through a polite text status." },
	ResetBehavior = "Choose Reset to clear the view only, or Clear to delete the persisted local note.",
	Variants = new[] { "Save", "Load", "Clear persisted data", "Reset view without deleting data" },
	KnownLimitations = new[] { "Data is local to the current app identity and is not synchronized between devices." },
	SourceRepositoryPath = "Uno.Gallery.ExtensionsPatterns/StoragePatternPage.xaml.cs",
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "extensions-configuration", "extensions-validation", "extensions-mvux-feedview" })]
public sealed partial class StoragePatternPage : Page
{
	private const string NoteKey = "UnoGallery.ExtensionsPatterns.Note";
	private IHost? _host;
	private IKeyValueStorage? _storage;

	public StoragePatternPage()
	{
		InitializeComponent();
		EnsureHost();
		ReportProvider();
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		EnsureHost();
		ReportProvider();
	}

	private void ReportProvider()
		=> AccessibilityHelper.Announce(
			StorageStatus,
			$"Provider: {_storage!.GetType().Name}; ready.");

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		_storage = null;
		_host?.Dispose();
		_host = null;
	}

	private IKeyValueStorage GetStorage()
	{
		EnsureHost();
		return _storage!;
	}

	private void EnsureHost()
	{
		if (_host is not null) return;
		_host = UnoHost
			.CreateDefaultBuilder(typeof(StoragePatternPage).Assembly)
			.ConfigureServices(services => services
				.AddSingleton<ISettings, PatternSettings>()
				.AddJsonTypeInfo(PatternJsonContext.Default.String))
			.Build();
		_storage = _host.Services.GetRequiredNamedService<IKeyValueStorage>("ApplicationData");
	}

	private async void Save_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			await GetStorage().SetAsync(NoteKey, NoteInput.Text ?? "", default);
			AccessibilityHelper.Announce(StorageStatus, "Saved to configured local storage.");
		}
		catch (Exception error)
		{
			Console.Error.WriteLine(error);
			AccessibilityHelper.Announce(StorageStatus, $"Save failed: {error.Message}");
		}
	}

	private async void Load_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			var note = await GetStorage().GetAsync<string>(NoteKey, default);
			NoteInput.Text = note ?? "";
			AccessibilityHelper.Announce(
				StorageStatus,
				note is null ? "No saved note." : "Loaded from configured local storage.");
		}
		catch (Exception error)
		{
			Console.Error.WriteLine(error);
			AccessibilityHelper.Announce(StorageStatus, $"Load failed: {error.Message}");
		}
	}

	private async void Clear_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			await GetStorage().ClearAsync(NoteKey, default);
			NoteInput.Text = "";
			AccessibilityHelper.Announce(StorageStatus, "Local note cleared.");
		}
		catch (Exception error)
		{
			Console.Error.WriteLine(error);
			AccessibilityHelper.Announce(StorageStatus, $"Clear failed: {error.Message}");
		}
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		NoteInput.Text = "";
		AccessibilityHelper.Announce(
			StorageStatus,
			"View reset; persisted data is unchanged. Use Clear to remove it.");
	}
}
