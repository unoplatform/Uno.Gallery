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
		    
		/*This function is to test the Unchecked option  in checkbox for material */ 		
		[Test]
		public void WhenMaterialClick_01_Unchecked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			
			var uncheckedBox = new QueryEx(x => x.All().Marked("Material_Unchecked"));
			App.ScrollDownTo("Material_Unchecked");			
			uncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("UNCHECKED", uncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

		}
		     
		/* This function is to test the DisabledUnchecked option  in checkbox for material.	*/
		[Test]
		public void WhenMaterialClick_02_DisabledUnchecked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			var materialDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Material_Disabled_Unchecked"));
			App.ScrollDownTo("Material_Disabled_Unchecked");			
			materialDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", materialDisabledUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
			Assert.IsFalse(materialDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
		 /* This function is to test the Checked option in checkbox for material.	*/	 
		[Test]
		public void WhenMaterialClick_03_Checked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before UnChecked");
			var checkedBox = new QueryEx(x => x.All().Marked("Material_Checked"));
			App.ScrollDownTo("Material_Checked");
			checkedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("CHECKED", checkedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
        /* This function is to test the DisabledChecked option  in checkbox for material */       
		[Test]
		public void WhenMaterialClick_04_DisabledChecked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before UnChecked");
			var materialDisabledCheckedBox = new QueryEx(x => x.All().Marked("Material_Disabled_Checked"));
			App.ScrollDownTo("Material_Disabled_Checked");
			materialDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("DISABLED CHECKED", materialDisabledCheckedBox.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
        /* This function is to test the Indeterminate for uncheck, recheck and indeterminate option in checkbox for material   */     
		[Test]
		public void WhenMaterialClick_05_Indeterminate()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before UnChecked");
			var checkedIndeterminateBox = new QueryEx(x => x.All().Marked("Material_Indeterminate"));
			App.ScrollDownTo("Material_Indeterminate");
			checkedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("INDETERMINATE", checkedIndeterminateBox.GetDependencyPropertyValue("Content"));			
			Assert.IsFalse(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			checkedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			checkedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			var isChecked = checkedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}

		     
        /* This function is to test the DisabledIndeterminate option  in checkbox for material.*/
        
		[Test]
		public void WhenMaterialClick_06_DisabledIndeterminate()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			var materialDisabledIndeterminateBox = new QueryEx(x => x.All().Marked("Material_Disabled_Indeterminate"));
			App.ScrollDownTo("Material_Disabled_Indeterminate");
			materialDisabledIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			
			Assert.IsFalse(materialDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = materialDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			Assert.AreEqual("DISABLED INDETERMINATE", materialDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
		}
	}
}
