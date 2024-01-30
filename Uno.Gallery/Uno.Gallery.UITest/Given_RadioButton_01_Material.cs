using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;

namespace Uno.Gallery.UITests
{
	//[Ignore("WIP: M3 Migration")]
	public class Given_RadioButton_01_Material : TestBase
	{
		/*     
		* This function is to test the Unchecked option  in checkbox for material, 
		*/
		[Test]
		public void WhenRadioButtonMaterialClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Checked");
			App.ScrollDownTo("RadioButton_Material_Unchecked");
			var uncheckedRadioButton = App.WaitThenTap("RadioButton_Material_Unchecked").ToQueryEx();
			TakeScreenshot("After Checked");

			//Assert.IsFalse(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(uncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}


		[Test]
		public void WhenRadioButtonMaterialClick_02_Checked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Tap");

			App.ScrollDownTo("RadioButton_material_Checked");
			var materialCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Checked"));

			materialCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(materialCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonMaterialClick_03_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Tap");
			var materialDisabledUnCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Unchecked"));

			App.ScrollDownTo("RadioButton_material_Disabled_Unchecked");

			materialDisabledUnCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(materialDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(materialDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonMaterialClick_04_DisabledChecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Tap");
			var materialDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Checked"));

			App.ScrollDownTo("RadioButton_Material_Disabled_Checked");

			materialDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}



	}
}
