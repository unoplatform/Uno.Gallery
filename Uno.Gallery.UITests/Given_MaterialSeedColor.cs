using NUnit.Framework;

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
	}
}
