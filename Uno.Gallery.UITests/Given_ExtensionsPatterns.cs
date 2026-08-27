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
	public void FeedView_empty_and_error_variants_render()
	{
		NavigateToSample("MVUX FeedView");
		App.WaitForElement("ExtensionsFeed_Status");
		var status = new QueryEx(query => query.All().Marked("ExtensionsFeed_Status"));

		App.WaitThenTap("ExtensionsFeed_Empty");
		WaitForText(status, "Scenario: Empty");
		App.WaitThenTap("ExtensionsFeed_Error");
		WaitForText(status, "Scenario: Error");
		App.WaitForElement("ExtensionsFeed_ErrorContent");
	}

	[Test]
	public void FeedView_repeated_refresh_uses_existing_subscription_without_throwing()
	{
		NavigateToSample("MVUX FeedView");
		App.WaitForElement("ExtensionsFeed_Status");
		var status = new QueryEx(query => query.All().Marked("ExtensionsFeed_Status"));

		App.WaitThenTap("ExtensionsFeed_Refresh");
		App.WaitThenTap("ExtensionsFeed_Refresh");
		WaitForText(status, "Scenario: Refresh; completed: 1");
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

		NavigateToSample("Extensions Configuration");
		App.WaitForElement("ExtensionsConfiguration_Status");
		NavigateToSample("Extensions Validation");
		App.WaitThenTap("ExtensionsValidation_Validate");
		status = new QueryEx(query => query.All().Marked("ExtensionsValidation_Status"));
		WaitForText(status, "Errors:");
	}

	[Test]
	public void Storage_uses_application_data_and_survives_page_reentry()
	{
		NavigateToSample("Extensions Storage");
		var status = new QueryEx(query => query.All().Marked("ExtensionsStorage_Status"));
		WaitForText(status, "Provider: ApplicationDataKeyValueStorage; ready.");
		App.WaitThenTap("ExtensionsStorage_Clear");
		WaitForText(status, "Local note cleared.");

		App.EnterText("ExtensionsStorage_Note", "persistent offline note");
		App.WaitThenTap("ExtensionsStorage_Save");
		WaitForText(status, "Saved to configured local storage.");

		NavigateToSample("Extensions Configuration");
		App.WaitForElement("ExtensionsConfiguration_Status");
		NavigateToSample("Extensions Storage");
		App.WaitThenTap("ExtensionsStorage_Load");
		status = new QueryEx(query => query.All().Marked("ExtensionsStorage_Status"));
		WaitForText(status, "Loaded from configured local storage.");
		var note = new QueryEx(query => query.All().Marked("ExtensionsStorage_Note"))
			.GetDependencyPropertyValue<string>("Text");
		Assert.AreEqual("persistent offline note", note);
	}

	[Test]
	public void Localization_service_switches_local_resource_catalog()
	{
		NavigateToSample("Extensions Localization");
		App.WaitThenTap("ExtensionsLocalization_SelectFrench");

		var greeting = new QueryEx(query => query.All().Marked("ExtensionsLocalization_Greeting"));
		WaitForText(greeting, "Bienvenue");
		var status = new QueryEx(query => query.All().Marked("ExtensionsLocalization_Status"));
		WaitForText(status, "fr-FR");
	}

	private static void WaitForText(QueryEx query, string expected)
	{
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
		var lastText = string.Empty;
		do
		{
			lastText = query.GetDependencyPropertyValue<string>("Text") ?? string.Empty;
			if (lastText.Contains(expected, StringComparison.Ordinal))
			{
				return;
			}
			Thread.Sleep(100);
		}
		while (DateTime.UtcNow < deadline);

		Assert.Fail($"Timed out waiting for '{expected}'. Last text: '{lastText}'.");
	}
}
#endif
