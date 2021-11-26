using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_Chip_Choice_01_Material : TestBase
	{
		/*This function is to test the Checked option in Choice Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_01_Checked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledChoice_Enabled");
			TakeScreenshot("Before Checked");
			var uncheckedChip = new QueryEx(x => x.All().Marked("ChipFilledChoice_Enabled"));
			uncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(uncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Unchecked option in Choice Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_02_Unchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledChoice_Selected");
			TakeScreenshot("Before Unchecked");
			var checkedChip = new QueryEx(x => x.All().Marked("ChipFilledChoice_Selected"));
			checkedChip.Tap();
			TakeScreenshot("After Unchecked");
			Assert.IsFalse(checkedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Disabled Unchecked option in Choice Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_03_DisabledUnchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledChoice_Disabled");
			TakeScreenshot("Before Checked");
			var disabledUncheckedChip = new QueryEx(x => x.All().Marked("ChipFilledChoice_Disabled"));
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsEnabled"));
			disabledUncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Checked option in Choice Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_04_Checked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedChoice_Enabled");
			TakeScreenshot("Before Checked");
			var uncheckedChip = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Enabled"));
			uncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(uncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Unchecked option in Choice Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_05_Unchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedChoice_Selected");
			TakeScreenshot("Before Unchecked");
			var checkedChip = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Selected"));
			checkedChip.Tap();
			TakeScreenshot("After Unchecked");
			Assert.IsFalse(checkedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Disabled Unchecked option in Choice Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_06_DisabledUnchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedChoice_Disabled");
			TakeScreenshot("Before Checked");
			var disabledUncheckedChip = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Disabled"));
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsEnabled"));
			disabledUncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
