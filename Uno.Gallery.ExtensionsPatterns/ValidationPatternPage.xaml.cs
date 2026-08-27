using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions;
using Uno.Extensions.Validation;
using Uno.Gallery.ExtensionsPatterns.Core;

namespace Uno.Gallery.ExtensionsPatterns;

[SamplePage(SampleCategory.AppPatterns, "Extensions Validation", SourceSdk.UnoExtensions,
	Description = "A local form using the Uno.Extensions validator with real DataAnnotations rules and visible errors.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Validation/ValidationOverview.html",
	Slug = "extensions-validation",
	Tags = new[] { "extensions", "validation", "form", "data-annotations", "offline", "optional-flavor" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "extensions-configuration", "extensions-storage" })]
public sealed partial class ValidationPatternPage : Page
{
	private readonly IHost _host;
	private readonly IValidator _validator;

	public ValidationPatternPage()
	{
		InitializeComponent();
		_host = new HostBuilder().UseValidation().Build();
		_validator = _host.Services.GetRequiredService<IValidator>();
		Unloaded += (_, _) => _host.Dispose();
	}

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
			var errors = (await _validator.ValidateAsync(form)).ToArray();
			ValidationStatus.Text = errors.Length == 0
				? "Valid: all local rules passed."
				: "Errors: " + string.Join(" ", errors.Select(error => error.ErrorMessage));
		}
		catch (Exception error)
		{
			ValidationStatus.Text = $"Validation failed: {error.Message}";
		}
	}

	private void ValidVariant_Click(object sender, RoutedEventArgs e)
	{
		NameInput.Text = "Ada";
		EmailInput.Text = "ada@example.test";
		AgeInput.Value = 37;
		ValidationStatus.Text = "Valid example loaded; choose Validate to run the rules.";
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		NameInput.Text = "";
		EmailInput.Text = "";
		AgeInput.Value = 18;
		ValidationStatus.Text = "Form reset.";
	}
}
