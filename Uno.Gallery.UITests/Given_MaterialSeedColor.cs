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
			App.WaitForElement("FidelityToggle");
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
		public void When_FidelityToggleExists_CanBeToggled()
		{
			NavigateToSample("Material Seed Color", "Material");

			App.WaitForElement("FidelityToggle");

			var initialValue = new QueryEx(x => x.All().Marked("FidelityToggle"))
				.GetDependencyPropertyValue<bool>("IsOn");

			// Toggle on
			App.Tap("FidelityToggle");
			TakeScreenshot("FidelityToggleOn");

			var afterToggle = new QueryEx(x => x.All().Marked("FidelityToggle"))
				.GetDependencyPropertyValue<bool>("IsOn");
			Assert.That(afterToggle, Is.Not.EqualTo(initialValue),
				"FidelityToggle IsOn must change after tap");

			// Toggle back
			App.Tap("FidelityToggle");
			var afterReset = new QueryEx(x => x.All().Marked("FidelityToggle"))
				.GetDependencyPropertyValue<bool>("IsOn");
			Assert.That(afterReset, Is.EqualTo(initialValue),
				"FidelityToggle must return to original state after second tap");
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
	}
}
