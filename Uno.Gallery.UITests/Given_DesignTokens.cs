using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// UITests for the Design Tokens Reference sample page.
	/// Verifies that density, spacing, shape, and typography tokens are
	/// visible after page load (construction-time token values) and that
	/// resolved token values are non-placeholder.
	/// </summary>
	public class Given_DesignTokens : TestBase
	{
		[Test]
		public void When_PageLoads_AllSectionsAreVisible()
		{
			NavigateToSample("Design Tokens", "Material");

			TakeScreenshot("PageLoaded");

			App.WaitForElement("Section_Density");
			App.WaitForElement("Section_Spacing");
			App.WaitForElement("Section_Shape");
			App.WaitForElement("Section_Typography");
		}

		[Test]
		public void When_PageLoads_DensityCardIsVisible()
		{
			NavigateToSample("Design Tokens", "Material");

			TakeScreenshot("DensityCard");

			App.WaitForElement("DensityInfoCard");
			App.WaitForElement("Token_Density_Compact");
			App.WaitForElement("Token_Density_Regular");
			App.WaitForElement("Token_Density_Comfy");
		}

		[Test]
		public void When_PageLoads_ActiveDensityTextIsNonEmpty()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("ActiveDensityText");

			TakeScreenshot("ActiveDensity");

			var text = new QueryEx(x => x.All().Marked("ActiveDensityText"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(text, Is.Not.Null.And.Not.Empty,
				"ActiveDensityText must be populated after page load");
		}

		[Test]
		public void When_PageLoads_SpacingTokensListIsPopulated()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("SpacingTokensList");

			TakeScreenshot("SpacingTokens");

			// At least Space400 must appear (always present in Regular density)
			App.ScrollDownTo("Token_Spacing_Space400");
			App.WaitForElement("Token_Spacing_Space400");
		}

		[Test]
		public void When_SpacingToken_Space300_HasNonPlaceholderValue()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("SpacingTokensList");
			App.ScrollDownTo("Token_Spacing_Space300");
			App.WaitForElement("Token_Spacing_Space300_Value");

			TakeScreenshot("Space300Value");

			var text = new QueryEx(x => x.All().Marked("Token_Spacing_Space300_Value"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(text, Is.Not.EqualTo("—"),
				"Space300 must resolve to a real value — check that Space300 key exists in active theme resources");
			Assert.That(text, Does.EndWith(" px"),
				"Space300 value must be a pixel measurement");
		}

		[Test]
		public void When_SpacingToken_Space400_HasNonPlaceholderValue()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("SpacingTokensList");
			App.ScrollDownTo("Token_Spacing_Space400");
			App.WaitForElement("Token_Spacing_Space400_Value");

			TakeScreenshot("Space400Value");

			var text = new QueryEx(x => x.All().Marked("Token_Spacing_Space400_Value"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(text, Is.Not.EqualTo("—"),
				"Space400 must resolve to a real value — check that Space400 key exists in active theme resources");
			Assert.That(text, Does.EndWith(" px"),
				"Space400 value must be a pixel measurement");
		}

		[Test]
		public void When_PageLoads_ShapeTokensListIsPopulated()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("ShapeTokensList");

			TakeScreenshot("ShapeTokens");

			App.ScrollDownTo("Token_Shape_Radius100CornerRadius");
			App.WaitForElement("Token_Shape_Radius100CornerRadius");
			App.ScrollDownTo("Token_Shape_RadiusFullCornerRadius");
			App.WaitForElement("Token_Shape_RadiusFullCornerRadius");
		}

		[Test]
		public void When_ShapeToken_Radius100CornerRadius_HasNonPlaceholderValue()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("ShapeTokensList");
			App.ScrollDownTo("Token_Shape_Radius100CornerRadius");
			App.WaitForElement("Token_Shape_Radius100CornerRadius_Value");

			TakeScreenshot("Radius100CornerRadiusValue");

			var text = new QueryEx(x => x.All().Marked("Token_Shape_Radius100CornerRadius_Value"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(text, Is.Not.EqualTo("—"),
				"Radius100CornerRadius must resolve — check that the key exists in active theme resources");
			Assert.That(text, Does.EndWith(" px"),
				"Radius100CornerRadius value must be a pixel measurement");
		}

		[Test]
		public void When_ShapeToken_RadiusFullCornerRadius_ShowsPillLabel()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("ShapeTokensList");
			App.ScrollDownTo("Token_Shape_RadiusFullCornerRadius");
			App.WaitForElement("Token_Shape_RadiusFullCornerRadius_Value");

			TakeScreenshot("RadiusFullCornerRadiusValue");

			var text = new QueryEx(x => x.All().Marked("Token_Shape_RadiusFullCornerRadius_Value"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(text, Is.EqualTo("∞ (pill)"),
				"RadiusFullCornerRadius must display as '∞ (pill)'");
		}

		[Test]
		public void When_PageLoads_TypographyTokensAreRendered()
		{
			NavigateToSample("Design Tokens", "Material");

			App.WaitForElement("TypographyTokensList");

			TakeScreenshot("TypographyTokens");

			App.ScrollDownTo("Token_DisplayLarge");
			App.WaitForElement("Token_DisplayLarge");
			App.ScrollDownTo("Token_BodyMedium");
			App.WaitForElement("Token_BodyMedium");
		}
	}
}
