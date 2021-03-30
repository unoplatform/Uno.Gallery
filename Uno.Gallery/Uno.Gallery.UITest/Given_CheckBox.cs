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
	public class Given_CheckBox : TestBase
	{
		[Test]
		/*
		 * This function is to test the Material- Checked option on the CheckBox page.
		 * 
		 * */
		public void When_ClickCheckedMaterial()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before UnChecked");

			var checkedBox = App.WaitThenTap("Material_Checked").ToQueryEx();

			TakeScreenshot("After UnChecked");

			Assert.IsFalse(checkedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		} 

		[Test]
		public void When_ClickIndeterminateMaterial()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before UnChecked");

			var checkedIndeterminateBox = App.WaitThenTap("Material_Indeterminate").ToQueryEx();

			TakeScreenshot("After UnChecked");

			Assert.IsFalse(checkedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var recheckIndeterminateBox = App.WaitThenTap("Material_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(recheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			var reUncheckIndeterminateBox = App.WaitThenTap("Material_Indeterminate").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(reUncheckIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		
		[Test]
		public void When_ClickMaterial()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var uncheckedBox = App.WaitThenTap("Material_Unchecked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		
		[Test]
		public void When_MaterialDisabledChecked()
		{
			NavigateToSample("CheckBox");
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var materialDisabledCheckedBox = App.WaitThenTap("Material_Disabled_Checked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsFalse(materialDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}
