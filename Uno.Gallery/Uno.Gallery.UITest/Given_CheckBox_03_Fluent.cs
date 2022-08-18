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
	public class Given_CheckBox_03_Fluent : TestBase
	{
		     
        /* This function is to test the Unchecked option  in checkbox for Fluent. */         
		[Test]
		public void WhenFluentClick_01_Unchecked()
		{
			//Navigation to Checkbox Fluent control
			NavigateToSample("CheckBox", "Fluent");

			//Scrolling to Unchecked option
			App.ScrollDownTo("Fluent_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");			
			var FluentUncheckedBox = new QueryEx(x => x.All().Marked("Fluent_Unchecked"));
			Assert.AreEqual("UNCHECKED", FluentUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Unchecked Box and taking screenshot
			FluentUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Uncheckbox 
			Assert.IsTrue(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		     
        /* This function is to test the DisabledUnchecked option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_02_DisabledUnchecked()
		{
			//Navigation to Checkbox Fluent control
			NavigateToSample("CheckBox", "Fluent");

			//Scrolling to Disabled Unchecked option
			App.ScrollDownTo("Fluent_Disabled_Unchecked");

			//Initial Validation
			TakeScreenshot("Before Checked");			
			var fluentDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", fluentDisabledUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Disabled Unchecked Box and taking screenshot
			fluentDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Uncheckbox		
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		     
		 /* This function is to test the Checked option in checkbox for Fluent. */	 
		[Test]
		public void WhenFluentClick_03_Checked()
		{
			//Navigation to Checkbox Fluent control
			NavigateToSample("CheckBox", "Fluent");

			//Scrolling to Checked option
			App.ScrollDownTo("Fluent_Checked");

			//Initial Validation
			TakeScreenshot("Before UnChecked");			
			var fluentCheckedBox = new QueryEx(x => x.All().Marked("Fluent_Checked"));
			Assert.AreEqual("CHECKED", fluentCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Checked Box and taking screenshot
			fluentCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Checkbox 			
			Assert.IsFalse(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}		
		     
        /* This function is to test the DisabledChecked option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_04_DisabledChecked()
		{
			//Navigation to Checkbox Fluent control
			NavigateToSample("CheckBox", "Fluent");

			//Scrolling to Disabled Checked option
			App.ScrollDownTo("Fluent_Disabled_Checked");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var fluentDisabledCheckedBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", fluentDisabledCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled checked Box and taking screenshot
			fluentDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Disabled checkbox 						
			Assert.IsTrue(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		     
        /* This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_05_Indeterminate()
		{
			//Navigation to Checkbox Fluent control
			NavigateToSample("CheckBox", "Fluent");

			//Scrolling to Indeterminate option
			App.ScrollDownTo("Fluent_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before UnChecked");
			var fluentCheckedIndeterminateBox = new QueryEx(x => x.All().Marked("Fluent_Indeterminate"));
			Assert.AreEqual("INDETERMINATE", fluentCheckedIndeterminateBox.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Indeterminate Box and taking screenshot
			fluentCheckedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");

			//validation after clicking Indeterminate box.			
			Assert.IsFalse(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on indeterminate Box second time and taking screenshot
			fluentCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Indeterminate box.
			Assert.IsTrue(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Indeterminate Box second time and taking screenshot.
			fluentCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Indeterminate box.
			var isChecked = fluentCheckedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}	
		
		     
        /* This function is to test the DisabledIndeterminate option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_06_DisabledIndeterminate()
		{
			//Navigation to Checkbox Fluent control
			NavigateToSample("CheckBox", "Fluent");

			//Scrolling to Disabled Indeterminate option
			App.ScrollDownTo("Fluent_Disabled_Indeterminate");

			//Initial Validation
			TakeScreenshot("Before Checked");
			var fluentDisabledIndeterminateBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Indeterminate"));
			Assert.AreEqual("DISABLED INDETERMINATE", fluentDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(fluentDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Disabled Indeterminate Box and taking screenshot
			fluentDisabledIndeterminateBox.Tap();	
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Indeterminate box.
			Assert.IsFalse(fluentDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = fluentDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			
		}
	}
}
