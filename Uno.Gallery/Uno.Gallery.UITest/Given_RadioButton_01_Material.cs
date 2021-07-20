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

		/*     
		* This function is to test the DisabledUnchecked option  in checkbox for material.
		
		[Test]
		public void WhenRadioButtonMaterialClick_03_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Checked");

			var materialDisabledUncheckedRadioButton = App.WaitThenTap("RadioButton_Material_Disabled_Unchecked").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/*     
		 * This function is to test the Checked option in checkbox for material.    
		 
		[Test]
		public void WhenRadioButtonMaterialClick_02_Checked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before UnChecked");

			var checkedRadioButton = App.WaitThenTap("RadioButton_Material_Checked").ToQueryEx();

			TakeScreenshot("After UnChecked");

			//Assert.IsFalse(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*     
        * This function is to test the DisabledChecked option  in checkbox for material, 
        
		[Test]
		public void WhenRadioButtonMaterialClick_04_DisabledChecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Checked");

			var materialDisabledCheckedRadioButton = App.WaitThenTap("Material_Disabled_Checked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}*/

		
		
		
	}
}
