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
	[Ignore("WIP: M3 Migration")]
	public class Given_CheckBox_02_Material_Secondary : TestBase
	{		    
        /*This function is to test the Unchecked option  in checkbox for material Secondary. */        
		[Test]
		public void WhenMaterialClickSecondary_01_Unchecked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Secondary Unchecked option
			App.ScrollDownTo("Material_Secondary_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var materialSecondaryUncheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Unchecked"));
			Assert.AreEqual("UNCHECKED", materialSecondaryUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialSecondaryUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Secondary Unchecked Box and taking screenshot
			materialSecondaryUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Secondary Uncheckbox 			
			Assert.IsTrue(materialSecondaryUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the DisabledUnchecked option  in checkbox for material Secondary.*/
		[Test]
		public void WhenMaterialClickSecondary_02_DisabledUnchecked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Secondary Disabled Unchecked option
			App.ScrollDownTo("Material_Secondary_Disabled_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Secondary Disabled UncheckedBox and taking screenshot
			materialSecondaryDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Secondary Disabled Uncheckbox						
			Assert.IsFalse(materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the Checked option in checkbox for material Secondary. */
		[Test]
		public void WhenMaterialClickSecondary_03_Checked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Secondary Checked option
			App.ScrollDownTo("Material_Secondary_Checked");

			//Initial Validation
			TakeScreenshot("Before UnChecked");			
			var materialSecondaryCheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Checked"));
			Assert.AreEqual("CHECKED", materialSecondaryCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(materialSecondaryCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Secondary Checked Box and taking screenshot
			materialSecondaryCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Secondary Checkbox 			
			Assert.IsFalse(materialSecondaryCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the DisabledChecked option  in checkbox for material Secondary. */
		[Test]
		public void WhenMaterialClickSecondary_04_DisabledChecked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Disabled Checked option
			App.ScrollDownTo("Material_Secondary_Disabled_Checked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledCheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue("Content"));			
			Assert.IsFalse(materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Disabled Checked Box and taking screenshot
			materialSecondaryDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Secondary Disabledcheckbox 
			Assert.IsTrue(materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
						
		}

		/* This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for material Secondary.*/
		[Test]
		public void WhenMaterialClickSecondary_05_Indeterminate()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Secondary Indeterminate option
			App.ScrollDownTo("Material_Secondary_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before UnChecked");			
			var materialSecondaryCheckedIndeterminateBox = new QueryEx(x => x.All().Marked("Material_Secondary_Indeterminate"));
			Assert.AreEqual("INDETERMINATE", materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Secondary Indeterminate Box and taking screenshot
			materialSecondaryCheckedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Secondary Indeterminate box.
			Assert.IsFalse(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Secondary Indeterminate Box second time and taking screenshot
			materialSecondaryCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Secondary Indeterminate box.
			Assert.IsTrue(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Secondary Indeterminate Box second time and taking screenshot.
			materialSecondaryCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Secondary Indeterminate box.
			var isChecked = materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}

		/* This function is to test the DisabledIndeterminate option  in checkbox for material Secondary */
		[Test]
		public void WhenMaterialClickSecondary_06_DisabledIndeterminate()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Secondary Disabled Indeterminate option
			App.ScrollDownTo("Material_Secondary_Disabled_Indeterminate");

			//Taking screenshot before clicking disabled Indeterminate checkbox.
			TakeScreenshot("Before Checked");

			//Tap/Click on Disabled Indeterminate Box and taking screenshot
			var materialSecondaryDisabledIndeterminateBox = App.WaitThenTap("Material_Secondary_Disabled_Indeterminate").ToQueryEx();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Indeterminate box.
			Assert.AreEqual("DISABLED INDETERMINATE", materialSecondaryDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialSecondaryDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));			
			var isChecked = materialSecondaryDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			
		}
	}
}
