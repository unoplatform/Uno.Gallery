using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Localization;
using Uno.Gallery.ExtensionsPatterns.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Localization", SourceSdk.UnoExtensions,
	Description = "A contained ILocalizationService adapter backed by deterministic local resources and culture selection.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Localization/LocalizationOverview.html",
	Slug = "extensions-localization",
	Tags = new[] { "extensions", "localization", "resources", "culture", "offline", "optional-flavor" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Build with EnableExtensionsPatterns=true. The English and French resources are embedded and require no network." },
	AccessibilityNotes = new[] { "The culture selector has a visible label and the localized greeting and status are exposed as text." },
	ResetBehavior = "Choose Reset to select the first locally supported culture.",
	Variants = new[] { "English local resources", "French local resources", "Culture reset" },
	KnownLimitations = new[] { "This contained adapter demonstrates ILocalizationService without changing the core Gallery process culture." },
	SourceRepositoryPath = "Uno.Gallery.ExtensionsPatterns/LocalizationPatternPage.xaml.cs",
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "localization-rtl", "extensions-mvux-feedview", "extensions-configuration" })]
public sealed partial class LocalizationPatternPage : Page
{
	private readonly PatternLocalizationCatalog _catalog = new();
	private readonly ILocalizationService _localization;

	public LocalizationPatternPage()
	{
		_localization = new ContainedLocalizationService(_catalog);
		InitializeComponent();
		CulturePicker.ItemsSource = _localization.SupportedCultures;
		CulturePicker.SelectedItem = _localization.CurrentCulture;
		UpdateText();
	}

	private async void CulturePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CulturePicker.SelectedItem is CultureInfo culture)
		{
			await _localization.SetCurrentCultureAsync(culture);
			UpdateText();
		}
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
		=> CulturePicker.SelectedItem = _localization.SupportedCultures[0];

	private void SelectFrench_Click(object sender, RoutedEventArgs e)
		=> CulturePicker.SelectedItem =
			_localization.SupportedCultures.Single(culture => culture.Name == "fr-FR");

	private void UpdateText()
	{
		GreetingText.Text = _catalog.Get(_localization.CurrentCulture, "Greeting");
		AccessibilityHelper.Announce(
			LocalizationStatus,
			$"{_catalog.Get(_localization.CurrentCulture, "Saved")} ({_localization.CurrentCulture.Name})");
	}

	private sealed class ContainedLocalizationService(PatternLocalizationCatalog catalog) : ILocalizationService
	{
		public CultureInfo[] SupportedCultures { get; } = [.. catalog.SupportedCultures];
		public CultureInfo CurrentCulture { get; private set; } = catalog.SupportedCultures[0];

		public Task SetCurrentCultureAsync(CultureInfo newCulture)
		{
			CurrentCulture = newCulture;
			return Task.CompletedTask;
		}
	}
}
