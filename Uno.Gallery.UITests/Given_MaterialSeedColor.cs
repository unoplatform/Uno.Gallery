using System;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// UITests for the Material Seed Color Laboratory sample page.
	/// Verifies page load, seed color change, and seed reset behavior
	/// using stable Uno.Themes 7.0.3 SemanticThemeHelper API.
	/// </summary>
	public class Given_MaterialSeedColor : TestBase
	{
		[Test]
		public void When_PageLoads_ColorPickerAndSwatchesAreVisible()
		{
			NavigateToSample("Material Seed Color", "Material");

			TakeScreenshot("PageLoaded");

			App.WaitForElement("SeedColorPicker");
			App.WaitForElement("ResetSeedButton");
			App.WaitForElement("SeedColorRuntimeNote");
		}

		[Test]
		public void When_PageLoads_PrimarySwatchIsPresent()
		{
			NavigateToSample("Material Seed Color", "Material");

			App.WaitForElement("SeedColorPicker");

			TakeScreenshot("PrimarySwatchVisible");

			App.WaitForElement("Swatch_Primary");
		}

		[Test]
		public void When_ResetButtonClicked_PageStillLoaded()
		{
			NavigateToSample("Material Seed Color", "Material");

			App.WaitForElement("ResetSeedButton");

			TakeScreenshot("BeforeReset");
			App.Tap("ResetSeedButton");
			TakeScreenshot("AfterReset");

			// After reset the page must still be functional — picker and swatches visible.
			App.WaitForElement("SeedColorPicker");
			App.WaitForElement("Swatch_Primary");
		}

		[Test]
		public void When_SecondaryAndTertiarySwatchesAreVisible()
		{
			NavigateToSample("Material Seed Color", "Material");

			App.WaitForElement("SeedColorPicker");

			TakeScreenshot("AllSwatchesVisible");

			App.WaitForElement("Swatch_Secondary");
			App.WaitForElement("Swatch_Tertiary");
			App.WaitForElement("Swatch_Surface");
		}

		/// <summary>
		/// Validates that a short (3-character) hex value — e.g. "#F00" — does not mutate the
		/// applied seed when the Apply button is tapped.  The ViewModel's ApplySeedHex guard
		/// requires exactly 6 hex digits and silently ignores shorter strings.
		///
		/// Navigates away at the end so that RestoreOriginalSeed fires and no global seed
		/// mutation persists for subsequent tests.
		/// </summary>
		[Test]
		public void When_ShortHex_IsRejected()
		{
			NavigateToSample("Material Seed Color", "Material");
			App.WaitForElement("CurrentSeedHex");

			var initialHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			App.WaitThenTap("SeedHexTextBox");
			App.ClearText("SeedHexTextBox");
			App.EnterText("SeedHexTextBox", "#F00");
			App.Tap("ApplySeedHexButton");

			App.WaitForElement("CurrentSeedHex");

			var afterHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			Assert.That(afterHex, Is.EqualTo(initialHex),
				$"Short hex '#F00' must be rejected; CurrentSeedHex must remain '{initialHex}' but got '{afterHex}'.");

			TakeScreenshot("ShortHex_Rejected");

			// Navigate away to trigger RestoreOriginalSeed — no global seed mutation after this test.
			NavigateToSample("Button", "Material");
			App.WaitForElement("Material_FilledButton");
		}

		/// <summary>
		/// Validates that a hex string containing invalid characters — e.g. "#GGGGGG" — does not
		/// mutate the applied seed when the Apply button is tapped.  The ViewModel catches the
		/// FormatException from Convert.ToByte and silently ignores the input.
		///
		/// Navigates away at the end so that RestoreOriginalSeed fires and no global seed
		/// mutation persists for subsequent tests.
		/// </summary>
		[Test]
		public void When_InvalidCharacters_AreRejected()
		{
			NavigateToSample("Material Seed Color", "Material");
			App.WaitForElement("CurrentSeedHex");

			var initialHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			App.WaitThenTap("SeedHexTextBox");
			App.ClearText("SeedHexTextBox");
			App.EnterText("SeedHexTextBox", "#GGGGGG");
			App.Tap("ApplySeedHexButton");

			App.WaitForElement("CurrentSeedHex");

			var afterHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			Assert.That(afterHex, Is.EqualTo(initialHex),
				$"Invalid hex '#GGGGGG' must be rejected; CurrentSeedHex must remain '{initialHex}' but got '{afterHex}'.");

			TakeScreenshot("InvalidChars_Rejected");

			// Navigate away to trigger RestoreOriginalSeed — no global seed mutation after this test.
			NavigateToSample("Button", "Material");
			App.WaitForElement("Material_FilledButton");
		}

		/// <summary>
		/// Lifecycle restore test: verifies that navigating away from the seed page
		/// restores the original app seed so the rest of the gallery is unaffected.
		///
		/// Flow:
		///   1. Navigate to Material Seed Color — OnPageLoaded fires, CaptureOriginalSeed records the
		///      pre-visit app seed and CurrentSeedHex displays it.
		///   2. Enter a known test color (#FF0000) via the hex-entry TextBox and tap Apply.
		///      CurrentSeedHex must reflect the changed value.
		///   3. Navigate away to a Material Button sample — OnPageUnloaded fires, RestoreOriginalSeed
		///      restores SemanticThemeHelper.PrimarySeed to the captured original.
		///   4. Navigate back to Material Seed Color (fresh page instance) — OnPageLoaded fires,
		///      CaptureOriginalSeed captures the now-restored seed.
		///   5. CurrentSeedHex must equal the value recorded in step 1 (seed fully restored).
		/// </summary>
		[Test]
		public void When_SeedIsRestored_AfterNavigatingAway()
		{
			// Step 1: navigate to the seed page and record the initial seed.
			NavigateToSample("Material Seed Color", "Material");
			App.WaitForElement("CurrentSeedHex");

			var initialHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			Assert.That(initialHex, Is.Not.Null.And.Not.Empty,
				"CurrentSeedHex must be populated on page load");

			TakeScreenshot("Step1_InitialSeed");

			// Step 2: change the seed to a known test color via the hex quick-entry.
			App.WaitThenTap("SeedHexTextBox");
			App.ClearText("SeedHexTextBox");
			App.EnterText("SeedHexTextBox", "#FF0000");
			App.Tap("ApplySeedHexButton");

			App.WaitForElement("CurrentSeedHex");

			var changedHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			Assert.That(changedHex, Is.EqualTo("#FF0000"),
				"CurrentSeedHex must update to #FF0000 after ApplySeedHexButton is tapped");

			TakeScreenshot("Step2_SeedChanged");

			// Step 3: navigate away — OnPageUnloaded fires → RestoreOriginalSeed restores the seed.
			NavigateToSample("Button", "Material");
			App.WaitForElement("Material_FilledButton");

			TakeScreenshot("Step3_NavigatedAway");

			// Steps 4 + 5: navigate back — fresh page loads with the restored seed.
			NavigateToSample("Material Seed Color", "Material");
			App.WaitForElement("CurrentSeedHex");

			var restoredHex = new QueryEx(x => x.All().Marked("CurrentSeedHex"))
				.GetDependencyPropertyValue<string>("Text");

			TakeScreenshot("Step4_SeedRestored");

			Assert.That(restoredHex, Is.EqualTo(initialHex),
				$"Seed must be restored to the original ({initialHex}) after navigating away; got {restoredHex}. " +
				"This indicates OnPageUnloaded/RestoreOriginalSeed did not fire correctly.");
		}
	}
}
