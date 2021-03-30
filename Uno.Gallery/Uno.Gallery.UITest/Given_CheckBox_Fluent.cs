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
	public class Given_CheckBox_Fluent : TestBase
	{
		/*     
		 * This function is to test the Checked option in checkbox for material.    
		 */
		[Test]
		public void When_ClickFluentChecked()
		{
			NavigateToSample("CheckBox");
			ShowFluentTheme();

			TakeScreenshot("Before UnChecked");

			var fluentCheckedBox = App.WaitThenTap("Fluent_Checked").ToQueryEx();

			TakeScreenshot("After UnChecked");

			Assert.IsFalse(fluentCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		/*     
        * This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for material, 
        */
		[Test]
		public void When_ClickFluentIndeterminate()
		{
			NavigateToSample("CheckBox");
			ShowFluentTheme();

			TakeScreenshot("Before UnChecked");

			var fluentCheckedIndeterminateBox = App.WaitThenTap("Fluent_Indeterminate").ToQueryEx();

			TakeScreenshot("After UnChecked");

			Assert.IsFalse(fluentCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var fluentRecheckIndeterminateBox = App.WaitThenTap("Fluent_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(fluentRecheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var fluentReuncheckIndeterminateBox = App.WaitThenTap("Fluent_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(fluentReuncheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		/*     
        * This function is to test the Unchecked option  in checkbox for material, 
        */
		[Test]
		public void When_ClickFluentUncheckedMaterial()
		{
			NavigateToSample("CheckBox");
			ShowFluentTheme();

			TakeScreenshot("Before Checked");

			var FluentUncheckedBox = App.WaitThenTap("Fluent_Unchecked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(FluentUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		/*     
        * This function is to test the DisabledChecked option  in checkbox for material, 
        */
		[Test]
		public void When_FluentDisabledChecked()
		{
			NavigateToSample("CheckBox");
			ShowFluentTheme();

			TakeScreenshot("Before Checked");

			var fluentDisabledCheckedBox = App.WaitThenTap("Fluent_Disabled_Checked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(fluentDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		/*     
        * This function is to test the DisabledUnchecked option  in checkbox for material.
        */
		[Test]
		public void When_FluentDisabledUnchecked()
		{
			NavigateToSample("CheckBox");
			ShowFluentTheme();

			TakeScreenshot("Before Checked");

			var fluentDisabledUncheckedBox = App.WaitThenTap("Fluent_Disabled_Unchecked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(fluentDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		/*     
        * This function is to test the DisabledIndeterminate option  in checkbox for material.
        */
		[Test]
		public void When_FluentDisabledIndeterminate()
		{
			NavigateToSample("CheckBox");
			ShowFluentTheme();

			TakeScreenshot("Before Checked");

			var fluentDisabledIndeterminateBox = App.WaitThenTap("Fluent_Disabled_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(fluentDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}
