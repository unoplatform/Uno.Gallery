using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Configuration;
using Uno.Gallery.ExtensionsPatterns.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Configuration", SourceSdk.UnoExtensions,
	Description = "Deterministic embedded JSON configuration mapped explicitly to typed IOptions.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Configuration/ConfigurationOverview.html",
	Slug = "extensions-configuration",
	Tags = new[] { "extensions", "configuration", "options", "embedded", "offline", "aot", "optional-flavor" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Build with EnableExtensionsPatterns=true. Configuration is embedded in the assembly and uses no external files or services." },
	AccessibilityNotes = new[] { "Typed option names and values are exposed as text and reload/reset actions are keyboard reachable." },
	ResetBehavior = "Choose Reset to restore the typed options captured when the page was constructed.",
	Variants = new[] { "Embedded JSON reload", "Typed IOptions projection", "Reset to captured defaults" },
	SourceRepositoryPath = "Uno.Gallery.ExtensionsPatterns/ConfigurationPatternPage.xaml.cs",
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "extensions-storage", "extensions-localization", "extensions-validation" })]
public sealed partial class ConfigurationPatternPage : Page
{
	private readonly IOptions<ExtensionsPatternOptions> _defaults;

	public ConfigurationPatternPage()
	{
		InitializeComponent();
		_defaults = LoadOptions();
		Display(_defaults.Value);
	}

	private static IOptions<ExtensionsPatternOptions> LoadOptions()
	{
		var configuration = new ConfigurationBuilder()
			.AddEmbeddedConfigurationFile<ConfigurationPatternPage>("appsettings.extensionspatterns.json")
			.Build();
		return Options.Create(ExtensionsPatternOptions.FromValues(key => configuration[key]));
	}

	private void Display(ExtensionsPatternOptions options)
	{
		EnvironmentValue.Text = options.Environment;
		PageSizeValue.Text = options.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
		DiagnosticsValue.Text = options.DiagnosticsEnabled ? "Enabled" : "Disabled";
	}

	private void Reload_Click(object sender, RoutedEventArgs e)
	{
		Display(LoadOptions().Value);
		AccessibilityHelper.Announce(ConfigurationStatus, "Embedded defaults reloaded.");
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		Display(_defaults.Value);
		AccessibilityHelper.Announce(ConfigurationStatus, "Typed options reset.");
	}
}
