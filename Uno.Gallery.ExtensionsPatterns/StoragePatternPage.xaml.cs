using System;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions;
using Uno.Extensions.Storage.KeyValueStorage;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Storage", SourceSdk.UnoExtensions,
	Description = "Local IKeyValueStorage save, load, clear, reset, and visible error handling.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Storage/StorageOverview.html",
	Slug = "extensions-storage",
	Tags = new[] { "extensions", "storage", "settings", "offline", "optional-flavor" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "extensions-configuration", "extensions-validation", "extensions-mvux-feedview" })]
public sealed partial class StoragePatternPage : Page
{
	private const string NoteKey = "UnoGallery.ExtensionsPatterns.Note";
	private readonly IHost _host;
	private readonly IKeyValueStorage _storage;

	public StoragePatternPage()
	{
		InitializeComponent();
		_host = new HostBuilder().UseStorage().Build();
		_storage = _host.Services.GetRequiredService<IKeyValueStorage>();
		Unloaded += (_, _) => _host.Dispose();
	}

	private async void Save_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			await _storage.SetAsync(NoteKey, NoteInput.Text ?? "", default);
			StorageStatus.Text = "Saved to local app settings.";
		}
		catch (Exception error)
		{
			StorageStatus.Text = $"Save failed: {error.Message}";
		}
	}

	private async void Load_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			var note = await _storage.GetAsync<string>(NoteKey, default);
			NoteInput.Text = note ?? "";
			StorageStatus.Text = note is null ? "No saved note." : "Loaded from local app settings.";
		}
		catch (Exception error)
		{
			StorageStatus.Text = $"Load failed: {error.Message}";
		}
	}

	private async void Clear_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			await _storage.ClearAsync(NoteKey, default);
			NoteInput.Text = "";
			StorageStatus.Text = "Local note cleared.";
		}
		catch (Exception error)
		{
			StorageStatus.Text = $"Clear failed: {error.Message}";
		}
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		NoteInput.Text = "";
		StorageStatus.Text = "View reset; persisted data is unchanged. Use Clear to remove it.";
	}
}
