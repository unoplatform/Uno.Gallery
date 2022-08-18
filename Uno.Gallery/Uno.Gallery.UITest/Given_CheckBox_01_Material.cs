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
	public class Given_CheckBox_01_Material : TestBase
	{
		    
		/*This function is to test the Unchecked option in checkbox for material */ 		
		[Test]
		public void WhenMaterialClick_01_Unchecked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");
			
			//Scrolling to Unchecked option
			App.ScrollDownTo("Material_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");			
			var uncheckedBox = new QueryEx(x => x.All().Marked("Material_Unchecked"));
			Assert.AreEqual("UNCHECKED", uncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Unchecked Box and taking screenshot
			uncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Uncheckbox 
			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

		}
		     
		/* This function is to test the DisabledUnchecked option  in checkbox for material.	*/
		[Test]
		public void WhenMaterialClick_02_DisabledUnchecked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Disabled Unchecked option
			App.ScrollDownTo("Material_Disabled_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var materialDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Material_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", materialDisabledUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled Unchecked Box and taking screenshot
			materialDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Uncheckbox 			
			Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
			Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
		 /* This function is to test the Checked option in checkbox for material.	*/	 
		[Test]
		public void WhenMaterialClick_03_Checked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Checked option
			App.ScrollDownTo("Material_Checked");

			//Initial Validation
			TakeScreenshot("Before UnChecked");
			var checkedBox = new QueryEx(x => x.All().Marked("Material_Checked"));
			Assert.AreEqual("CHECKED", checkedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Checked Box and taking screenshot
			checkedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Checkbox 			
			Assert.IsFalse(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
        /* This function is to test the DisabledChecked option  in checkbox for material */       
		[Test]
		public void WhenMaterialClick_04_DisabledChecked()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Disabled Checked option
			App.ScrollDownTo("Material_Disabled_Checked");

			//Initial Validation
			TakeScreenshot("Before UnChecked");
			var materialDisabledCheckedBox = new QueryEx(x => x.All().Marked("Material_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", materialDisabledCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Unchecked Box and taking screenshot
			materialDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Disabled Checkbox 						
			Assert.IsTrue(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
        /* This function is to test the Indeterminate for uncheck, recheck and indeterminate option in checkbox for material   */     
		[Test]
		public void WhenMaterialClick_05_Indeterminate()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Indeterminate option
			App.ScrollDownTo("Material_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before UnChecked");
			var checkedIndeterminateBox = new QueryEx(x => x.All().Marked("Material_Indeterminate"));
			Assert.AreEqual("INDETERMINATE", checkedIndeterminateBox.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Indeterminate Box and taking screenshot
			checkedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Indeterminate box.						
			Assert.IsFalse(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on indeterminate Box second time and taking screenshot
			checkedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Indeterminate box.
			Assert.IsTrue(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Indeterminate Box second time and taking screenshot.
			checkedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Indeterminate box.
			var isChecked = checkedIndeterminateBox.GetDependencyPropertyValue("IsChecked");			
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}
		     
        /* This function is to test the DisabledIndeterminate option  in checkbox for material.*/
        
		[Test]
		public void WhenMaterialClick_06_DisabledIndeterminate()
		{
			//Navigation to Checkbox material control
			NavigateToSample("CheckBox", "Material");

			//Scrolling to Disabled Indeterminate option
			App.ScrollDownTo("Material_Disabled_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var materialDisabledIndeterminateBox = new QueryEx(x => x.All().Marked("Material_Disabled_Indeterminate"));
			Assert.AreEqual("DISABLED INDETERMINATE", materialDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled Indeterminate Box and taking screenshot
			materialDisabledIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Indeterminate box.
			Assert.IsFalse(materialDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = materialDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			
		}
	}
}
