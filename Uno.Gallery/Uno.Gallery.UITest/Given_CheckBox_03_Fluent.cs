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
		/*     
        * This function is to test the Unchecked option  in checkbox for material, 
        */
		[Test]
		public void WhenFluentClick_01_Unchecked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");			
			var FluentUncheckedBox = new QueryEx(x => x.Marked("Fluent_Unchecked"));
			FluentUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));

			/*var FluentUncheckedBox = App.WaitThenTap("Fluent_Unchecked").ToQueryEx();
			TakeScreenshot("After Checked");
			//Assert.IsFalse(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));*/
		}

		/*     
        * This function is to test the DisabledUnchecked option  in checkbox for material.
        */
		[Test]
		public void WhenFluentClick_02_DisabledUnchecked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");			
			var fluentDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Fluent_Disabled_Unchecked"));			
			fluentDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));

			/*var fluentDisabledUncheckedBox = App.WaitThenTap("Fluent_Disabled_Unchecked").ToQueryEx();
			TakeScreenshot("After Checked");
			//Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));*/
		}

		/*     
		 * This function is to test the Checked option in checkbox for material.    
		 */
		[Test]
		public void WhenFluentClick_03_Checked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before UnChecked");

			var fluentCheckedBox = App.WaitThenTap("Fluent_Checked").ToQueryEx();

			TakeScreenshot("After UnChecked");

			//Assert.IsFalse(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		
		/*     
        * This function is to test the DisabledChecked option  in checkbox for material, 
        */
		[Test]
		public void WhenFluentClick_04_DisabledChecked()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");

			var fluentDisabledCheckedBox = App.WaitThenTap("Fluent_Disabled_Checked").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*     
        * This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for material, 
        */
		[Test]
		public void WhenFluentClick_05_Indeterminate()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before UnChecked");

			var fluentCheckedIndeterminateBox = App.WaitThenTap("Fluent_Indeterminate").ToQueryEx();

			TakeScreenshot("After UnChecked");

			//Assert.IsFalse(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var fluentRecheckIndeterminateBox = App.WaitThenTap("Fluent_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(fluentRecheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentRecheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var fluentReuncheckIndeterminateBox = App.WaitThenTap("Fluent_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(fluentReuncheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(fluentReuncheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}	
		
		/*     
        * This function is to test the DisabledIndeterminate option  in checkbox for material.
        */
		[Test]
		public void WhenFluentClick_06_DisabledIndeterminate()
		{
			NavigateToSample("CheckBox", "Fluent");

			TakeScreenshot("Before Checked");

			var fluentDisabledIndeterminateBox = App.WaitThenTap("Fluent_Disabled_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			//Assert.IsFalse(fluentDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(fluentDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}
