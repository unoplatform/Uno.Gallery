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
	public class Given_CheckBox_04_Cupertino : TestBase
	{
		     
        /* This function is to test the Unchecked option  in checkbox for Cupertino. */        
		[Test]
		public void WhenCupertinoClick_01_Unchecked()
		{
			//Navigation to Checkbox Cupertino control
			NavigateToSample("CheckBox", "Cupertino");

			//Scrolling to Unchecked option
			App.ScrollDownTo("Cupertino_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var cupertinoUncheckedBox = new QueryEx(x => x.Marked("Cupertino_Unchecked"));
			Assert.AreEqual("UNCHECKED", cupertinoUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(cupertinoUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Unchecked Box and taking screenshot
			cupertinoUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Uncheckbox 			
			Assert.IsTrue(cupertinoUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
        /* This function is to test the DisabledChecked option  in checkbox for Cupertino. */        
		[Test]
		public void WhenCupertinoClick_02_DisabledUnChecked()
		{
			//Navigation to Checkbox Cupertino control
			NavigateToSample("CheckBox", "Cupertino");

			//Scrolling to Disabled Unchecked option
			App.ScrollDownTo("Cupertino_Disabled_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var cupertinoDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Cupertino_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", cupertinoDisabledUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(cupertinoDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled Unchecked Box and taking screenshot
			cupertinoDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Uncheckbox						
			Assert.IsFalse(cupertinoDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
		 /* This function is to test the Checked option in checkbox for Cupertino.*/		 
		[Test]
		public void WhenCupertinoClick_03_Checked()
		{
			//Navigation to Checkbox Cupertino control
			NavigateToSample("CheckBox", "Cupertino");

			//Scrolling to Checked option
			App.ScrollDownTo("Cupertino_Checked");

			//Initial Validation	
			TakeScreenshot("Before UnChecked");
			var cupertinoCheckedBox = new QueryEx(x => x.All().Marked("Cupertino_Checked"));
			Assert.AreEqual("CHECKED", cupertinoCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(cupertinoCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Checked Box and taking screenshot
			cupertinoCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Checkbox			
			Assert.IsFalse(cupertinoCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		     
        /* This function is to test the DisabledUnchecked option  in checkbox for Cupertino.*/        
		[Test]
		public void WhenCupertinoClick_04_DisabledChecked()
		{
			//Navigation to Checkbox Cupertino control
			NavigateToSample("CheckBox", "Cupertino");

			//Scrolling to Disabled Checked option
			App.ScrollDownTo("Cupertino_Disabled_Checked");

			//Initial Validation
			TakeScreenshot("Before UnChecked");
			var cupertinoDisabledCheckedBox = new QueryEx(x => x.All().Marked("Cupertino_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", cupertinoDisabledCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(cupertinoDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled checkedBox and taking screenshot
			cupertinoDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking on Disabled checkedBox 						
			Assert.IsTrue(cupertinoDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		     
        /* This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for Cupertino.*/        
		[Test]
		public void WhenCupertinoClick_05_Indeterminate()
		{
			//Navigation to Checkbox Cupertino control
			NavigateToSample("CheckBox", "Cupertino");

			//Scrolling to Indeterminate option
			App.ScrollDownTo("Cupertino_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before UnChecked");
			var cupertinoCheckedIndeterminateBox = new QueryEx(x => x.All().Marked("Cupertino_Indeterminate"));
			Assert.AreEqual("INDETERMINATE", cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Indeterminate Box and taking screenshot
			cupertinoCheckedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Indeterminate box.			
			Assert.IsFalse(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on indeterminate Box second time and taking screenshot
			cupertinoCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Indeterminate box.
			Assert.IsTrue(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Indeterminate Box second time and taking screenshot.
			cupertinoCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Indeterminate box.
			var isChecked = cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}
		
		     
        /* This function is to test the DisabledIndeterminate option  in checkbox for Cupertino. */        
		[Test]
		public void WhenCupertinoClick_06_DisabledIndeterminate()
		{
			//Navigation to Checkbox Cupertino control
			NavigateToSample("CheckBox", "Cupertino");

			//Scrolling to Disabled Indeterminate option
			App.ScrollDownTo("Cupertino_Disabled_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var cupertinoDisabledIndeterminateBox = new QueryEx(x => x.All().Marked("Cupertino_Disabled_Indeterminate"));
			Assert.AreEqual("DISABLED INDETERMINATE", cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled Indeterminate Box and taking screenshot
			cupertinoDisabledIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Indeterminate box.
			Assert.IsFalse(cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			
		}
	}
}
