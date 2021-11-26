using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_Chip_Action_01_Material : TestBase
	{
		/*This function is to test the Checked option in Action Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_01_Checked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledAction_Enabled");
			TakeScreenshot("Before Checked");
			var uncheckedChip = new QueryEx(x => x.All().Marked("ChipFilledAction_Enabled"));
			uncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(uncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Disabled Unchecked option in Action Chip Filled for Material. */
		[Test]
		public void WhenMaterialClick_02_DisabledUnchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipFilledAction_Disabled");
			TakeScreenshot("Before Checked");
			var disabledUncheckedChip = new QueryEx(x => x.All().Marked("ChipFilledAction_Disabled"));
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsEnabled"));
			disabledUncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Checked option in Action Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_03_Checked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedAction_Enabled");
			TakeScreenshot("Before Checked");
			var uncheckedChip = new QueryEx(x => x.All().Marked("ChipOutlinedAction_Enabled"));
			uncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(uncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/* This function is to test the Disabled Unchecked option in Action Chip Outlined for Material. */
		[Test]
		public void WhenMaterialClick_04_DisabledUnchecked()
		{
			NavigateToSample("Chip", "Material");

			App.ScrollDownTo("ChipOutlinedAction_Disabled");
			TakeScreenshot("Before Checked");
			var disabledUncheckedChip = new QueryEx(x => x.All().Marked("ChipOutlinedAction_Disabled"));
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsEnabled"));
			disabledUncheckedChip.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(disabledUncheckedChip.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
