using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_Chip_Input_01_Material : TestBase
	{
		/*This function is to test the Checked option in Input Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_01_Checked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledInput_Enabled");
			TakeScreenshot("Before Checked");
			var uncheckedChip = new QueryEx(x => x.All().Marked("ChipFilledInput_Enabled"));
			uncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(uncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Unchecked option in Input Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_02_Unchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledInput_Selected");
			TakeScreenshot("Before Unchecked");
			var checkedChip = new QueryEx(x => x.All().Marked("ChipFilledInput_Selected"));
			checkedChip.Tap();
			TakeScreenshot("After Unchecked");
			Assert.IsFalse(checkedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Disabled Unchecked option in Input Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_03_DisabledUnchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledInput_Disabled");
			TakeScreenshot("Before Checked");
			var disabledUncheckedChip = new QueryEx(x => x.All().Marked("ChipFilledInput_Disabled"));
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsEnabled"));
			disabledUncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Checked option in Input Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_04_Checked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedInput_Enabled");
			TakeScreenshot("Before Checked");
			var uncheckedChip = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Enabled"));
			uncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(uncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Unchecked option in Input Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_05_Unchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedInput_Selected");
			TakeScreenshot("Before Unchecked");
			var checkedChip = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Selected"));
			checkedChip.Tap();
			TakeScreenshot("After Unchecked");
			Assert.IsFalse(checkedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Disabled Unchecked option in Input Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_06_DisabledUnchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedInput_Disabled");
			TakeScreenshot("Before Checked");
			var disabledUncheckedChip = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Disabled"));
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsEnabled"));
			disabledUncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
