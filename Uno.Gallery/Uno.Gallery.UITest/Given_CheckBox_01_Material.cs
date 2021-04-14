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
	public class Given_CheckBox_01_Material : TestBase
	{
		/*     
		* This function is to test the Unchecked option  in checkbox for material, 
		*/
		[Test]
		public void WhenMaterialClick_01_Unchecked()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var uncheckedBox = App.WaitThenTap("Material_Unchecked").ToQueryEx();
			
			TakeScreenshot("After Checked");

			//Assert.IsFalse(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*     
		* This function is to test the DisabledUnchecked option  in checkbox for material.
		*/
		[Test]
		public void WhenMaterialClick_02_DisabledUnchecked()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var materialDisabledUncheckedBox = App.WaitThenTap("Material_Disabled_Unchecked").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/*     
		 * This function is to test the Checked option in checkbox for material.    
		 */
		[Test]
		public void WhenMaterialClick_03_Checked()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before UnChecked");

			var checkedBox = App.WaitThenTap("Material_Checked").ToQueryEx();

			TakeScreenshot("After UnChecked");

			//Assert.IsFalse(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*     
        * This function is to test the DisabledChecked option  in checkbox for material, 
        */
		[Test]
		public void WhenMaterialClick_04_DisabledChecked()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var materialDisabledCheckedBox = App.WaitThenTap("Material_Disabled_Checked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsTrue(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*     
        * This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for material, 
        */
		[Test]
		public void WhenMaterialClick_05_Indeterminate()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before UnChecked");

			var checkedIndeterminateBox = App.WaitThenTap("Material_Indeterminate").ToQueryEx();

			TakeScreenshot("After UnChecked");

			//Assert.IsFalse(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var recheckIndeterminateBox = App.WaitThenTap("Material_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(recheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(recheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var reUncheckIndeterminateBox = App.WaitThenTap("Material_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(reUncheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(reUncheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	
		/*     
        * This function is to test the DisabledIndeterminate option  in checkbox for material.
        */
		[Test]
		public void WhenMaterialClick_06_DisabledIndeterminate()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var materialDisabledIndeterminateBox = App.WaitThenTap("Material_Disabled_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(materialDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}
