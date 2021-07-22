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
	public class Given_CheckBox_03_Fluent : TestBase
	{
		     
        /* This function is to test the Unchecked option  in checkbox for Fluent. */         
		[Test]
		public void WhenFluentClick_01_Unchecked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");			
			var FluentUncheckedBox = new QueryEx(x => x.All().Marked("Fluent_Unchecked"));
			App.ScrollDownTo("Fluent_Unchecked");			
			FluentUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		     
        /* This function is to test the DisabledUnchecked option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_02_DisabledUnchecked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");			
			var fluentDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Unchecked"));
			App.ScrollDownTo("Fluent_Disabled_Unchecked");
			fluentDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		     
		 /* This function is to test the Checked option in checkbox for Fluent. */	 
		[Test]
		public void WhenFluentClick_03_Checked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before UnChecked");			
			var fluentCheckedBox = new QueryEx(x => x.All().Marked("Fluent_Checked"));
			App.ScrollDownTo("Fluent_Checked");
			fluentCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.IsFalse(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
		}		
		     
        /* This function is to test the DisabledChecked option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_04_DisabledChecked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");
			var fluentDisabledCheckedBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Checked"));
			App.ScrollDownTo("Fluent_Disabled_Checked");
			fluentDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.IsFalse(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		     
        /* This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_05_Indeterminate()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before UnChecked");
			var fluentCheckedIndeterminateBox = new QueryEx(x => x.All().Marked("Fluent_Indeterminate"));
			App.ScrollDownTo("Fluent_Indeterminate");
			fluentCheckedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.IsFalse(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			fluentCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			fluentCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			var isChecked = fluentCheckedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}	
		
		     
        /* This function is to test the DisabledIndeterminate option  in checkbox for Fluent. */        
		[Test]
		public void WhenFluentClick_06_DisabledIndeterminate()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");

			var fluentDisabledIndeterminateBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Indeterminate"));
			App.ScrollDownTo("Fluent_Disabled_Indeterminate");
			fluentDisabledIndeterminateBox.Tap();
	
			TakeScreenshot("After Checked");
			Assert.IsFalse(fluentDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = fluentDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}
	}
}
