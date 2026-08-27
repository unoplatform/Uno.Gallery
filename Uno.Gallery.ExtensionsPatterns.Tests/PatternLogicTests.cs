using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using Uno.Gallery.ExtensionsPatterns.Core;

namespace Uno.Gallery.ExtensionsPatterns.Tests;

public class PatternLogicTests
{
	[Test]
	public void Feed_scenarios_are_deterministic_and_resettable()
	{
		var controller = new FeedScenarioController();

		controller.CreateResult().Items.Should().HaveCount(3);
		controller.Select(FeedScenario.Empty);
		controller.CreateResult().Items.Should().BeEmpty();
		controller.Select(FeedScenario.Refresh);
		controller.CreateResult().Heading.Should().Be("Refreshed locally (1)");
		controller.CreateResult().Heading.Should().Be("Refreshed locally (2)");

		controller.Reset();
		controller.Scenario.Should().Be(FeedScenario.Data);
		controller.RefreshCount.Should().Be(0);
	}

	[Test]
	public void Feed_error_is_local_and_explicit()
	{
		var controller = new FeedScenarioController();
		controller.Select(FeedScenario.Error);

		controller.Invoking(x => x.CreateResult())
			.Should().Throw<InvalidOperationException>()
			.WithMessage("Deterministic offline feed failure.");
	}

	[Test]
	public void Configuration_maps_embedded_values_to_typed_options()
	{
		var values = new Dictionary<string, string?>
		{
			["AppPatterns:Environment"] = "Test",
			["AppPatterns:PageSize"] = "8",
			["AppPatterns:DiagnosticsEnabled"] = "true"
		};

		var options = ExtensionsPatternOptions.FromValues(key => values.GetValueOrDefault(key));

		options.Should().Be(new ExtensionsPatternOptions("Test", 8, true));
	}

	[Test]
	public void Localization_falls_back_to_English_for_unknown_culture()
	{
		var catalog = new PatternLocalizationCatalog();

		catalog.SupportedCultures.Should().Contain(culture => culture.Name == "fr-FR");
		catalog.Get(new CultureInfo("de-DE"), "Greeting")
			.Should().Be("Welcome to the offline app-pattern showcase.");
	}

	[Test]
	public void Registration_rules_return_real_field_errors()
	{
		var model = new RegistrationForm { Name = "A", Email = "invalid", Age = 12 };
		var results = new List<ValidationResult>();

		Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true)
			.Should().BeFalse();
		results.Should().HaveCount(3);
		results.Should().Contain(result => result.ErrorMessage == "Name must contain at least 2 characters.");
		results.Should().Contain(result => result.ErrorMessage == "Enter a valid email address.");
		results.Should().Contain(result => result.ErrorMessage == "Age must be between 18 and 120.");
	}
}
