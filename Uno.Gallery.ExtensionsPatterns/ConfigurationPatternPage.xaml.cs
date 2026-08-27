using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Configuration;
using Uno.Gallery.ExtensionsPatterns.Core;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Configuration", SourceSdk.UnoExtensions,
	Description = "Deterministic embedded JSON configuration mapped explicitly to typed IOptions.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Configuration/ConfigurationOverview.html",
	Slug = "extensions-configuration",
	Tags = new[] { "extensions", "configuration", "options", "embedded", "offline", "aot", "optional-flavor" },
	Status = SampleStatus.Stable,
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
		ConfigurationStatus.Text = "Embedded defaults reloaded.";
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		Display(_defaults.Value);
		ConfigurationStatus.Text = "Typed options reset.";
	}
}
