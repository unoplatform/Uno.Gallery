using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions;
using Uno.Extensions.Hosting;
using Uno.Extensions.Validation;
using Uno.Gallery.ExtensionsPatterns.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Validation", SourceSdk.UnoExtensions,
	Description = "A local form using the Uno.Extensions validator with real DataAnnotations rules and visible errors.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Validation/ValidationOverview.html",
	Slug = "extensions-validation",
	Tags = new[] { "extensions", "validation", "form", "data-annotations", "offline", "optional-flavor" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Build with EnableExtensionsPatterns=true. Validation uses local DataAnnotations and performs no server roundtrip." },
	AccessibilityNotes = new[] { "Every field has a header, focus follows form order, and validation errors are summarized in a polite text status." },
	ResetBehavior = "Choose Reset to clear the form and restore the default age.",
	Variants = new[] { "Required-field errors", "Email and range errors", "Pre-filled valid form" },
	SourceRepositoryPath = "Uno.Gallery.ExtensionsPatterns/ValidationPatternPage.xaml.cs",
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "extensions-configuration", "extensions-storage" })]
public sealed partial class ValidationPatternPage : Page
{
	private IHost? _host;
	private IValidator? _validator;

	public ValidationPatternPage()
	{
		InitializeComponent();
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e) => EnsureHost();

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		_validator = null;
		_host?.Dispose();
		_host = null;
	}

	private IValidator GetValidator()
	{
		EnsureHost();
		return _validator!;
	}

	private void EnsureHost()
	{
		if (_host is not null) return;
		_host = UnoHost
			.CreateDefaultBuilder(typeof(ValidationPatternPage).Assembly)
			.UseValidation()
			.Build();
		_validator = _host.Services.GetRequiredService<IValidator>();
	}

	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(RegistrationForm))]
	private async void Validate_Click(object sender, RoutedEventArgs e)
	{
		var form = new RegistrationForm
		{
			Name = NameInput.Text ?? "",
			Email = EmailInput.Text ?? "",
			Age = double.IsNaN(AgeInput.Value) ? 0 : (int)AgeInput.Value
		};

		try
		{
			var errors = (await GetValidator().ValidateAsync(form)).ToArray();
			AccessibilityHelper.Announce(
				ValidationStatus,
				errors.Length == 0
					? "Valid: all local rules passed."
					: "Errors: " + string.Join(" ", errors.Select(error => error.ErrorMessage)));
		}
		catch (Exception error)
		{
			AccessibilityHelper.Announce(ValidationStatus, $"Validation failed: {error.Message}");
		}
	}

	private void ValidVariant_Click(object sender, RoutedEventArgs e)
	{
		NameInput.Text = "Ada";
		EmailInput.Text = "ada@example.test";
		AgeInput.Value = 37;
		AccessibilityHelper.Announce(
			ValidationStatus,
			"Valid example loaded; choose Validate to run the rules.");
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		NameInput.Text = "";
		EmailInput.Text = "";
		AgeInput.Value = 18;
		AccessibilityHelper.Announce(ValidationStatus, "Form reset.");
	}
}
