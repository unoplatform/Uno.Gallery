using System;
using NUnit.Framework;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// Focused tests for the ShellNavigator abstraction: backdoor navigate with design,
	/// shell search exercises QuerySubmitted and loads target, and overview View button navigates.
	/// </summary>
	public class Given_ShellNavigator : TestBase
	{
		[Test]
		public void When_Backdoor_Navigate_With_Design_Shows_Sample()
		{
			NavigateToSample("Button", "Material");

			TakeScreenshot("AfterBackdoor");

			// Assert ButtonSamplePage content (material template) is displayed — not the nav item.
			App.WaitForElement("Material_FilledButton");
		}

		[Test]
		public void When_Search_Selects_Expands_And_Loads_Target()
		{
			// Deterministically reset to Material design before exercising search so prior test state
			// (e.g. a Cupertino-design navigation) does not leak into this test's assertions.
			NavigateToSample("Overview", "Material");

			OpenNavView();

			// Type "CheckBox" in the shell search box; wait for the "checkbox" suggestion item
			// (AutomationId bound to Sample.Slug) and tap it to trigger SuggestionChosen deterministically.
			// SuggestionChosen calls Navigator.NavigateTo(sample, ExpandCategory) — the canonical path
			// that expands the category and selects the nav item without relying on keyboard state.
			App.WaitThenTap("SamplesSearchBox");
			App.ClearText("SamplesSearchBox");
			App.EnterText("SamplesSearchBox", "CheckBox");
			App.WaitThenTap("checkbox");

			TakeScreenshot("AfterSearchNavigation");

			// Assert CheckBox sample page content (Material is the default design).
			App.WaitForElement("Material_Unchecked", timeout: TimeSpan.FromSeconds(60));
		}

		[Test]
		public void When_Overview_View_Button_Navigates_To_Sample()
		{
			NavigateToSample("Overview", "Material");

			TakeScreenshot("OverviewPage");

			// The Button OverviewSampleView's ViewButtonAutomationId DP is computed from SamplePageType+SampleDesign
			// and propagated to PART_ViewButton via TemplateBinding. Tapping it calls
			// ShellNavigator.NavigateTo which canonicalizes the locally-constructed Sample.
			App.WaitThenTap("ViewButton_ButtonSamplePage_Material");

			TakeScreenshot("AfterViewButtonClick");

			// Assert the Button sample page content (material template) loaded.
			App.WaitForElement("Material_FilledButton");
		}
	}
}
