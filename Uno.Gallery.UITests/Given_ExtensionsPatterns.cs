#if EXTENSIONS_PATTERNS
using System;
using System.Threading;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

[Category("ExtensionsPatterns")]
public class Given_ExtensionsPatterns : TestBase
{
	[Test]
	public void FeedView_variants_report_deterministic_states()
	{
		NavigateToSample("MVUX FeedView");
		App.WaitForElement("ExtensionsFeed_Status");
		var status = new QueryEx(query => query.All().Marked("ExtensionsFeed_Status"));

		App.WaitThenTap("ExtensionsFeed_Empty");
		WaitForText(status, "Scenario: Empty");
		App.WaitThenTap("ExtensionsFeed_Error");
		WaitForText(status, "Scenario: Error");
		App.WaitThenTap("ExtensionsFeed_Reset");
		WaitForText(status, "Scenario: Data");
	}

	[Test]
	public void Embedded_configuration_is_visible_and_resettable()
	{
		NavigateToSample("Extensions Configuration");
		var environment = new QueryEx(query => query.All().Marked("ExtensionsConfiguration_Environment"));
		WaitForText(environment, "Contained offline showcase");

		App.WaitThenTap("ExtensionsConfiguration_Reset");
		App.WaitForElement("ExtensionsConfiguration_Status");
	}

	[Test]
	public void Validation_shows_errors_then_accepts_valid_variant()
	{
		NavigateToSample("Extensions Validation");
		App.WaitThenTap("ExtensionsValidation_Validate");
		var status = new QueryEx(query => query.All().Marked("ExtensionsValidation_Status"));
		WaitForText(status, "Errors:");

		App.WaitThenTap("ExtensionsValidation_ValidVariant");
		App.WaitThenTap("ExtensionsValidation_Validate");
		WaitForText(status, "Valid: all local rules passed.");
	}

	private static void WaitForText(QueryEx query, string expected)
	{
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
		do
		{
			if (query.GetDependencyPropertyValue<string>("Text") is { } text &&
				text.Contains(expected, StringComparison.Ordinal))
			{
				return;
			}
			Thread.Sleep(100);
		}
		while (DateTime.UtcNow < deadline);

		Assert.Fail($"Timed out waiting for '{expected}'.");
	}
}
#endif
