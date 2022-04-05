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
	public class Given_CheckBox_02_Material_Secondary : TestBase
	{		    
        /*This function is to test the Unchecked option  in checkbox for material Secondary. */        
		[Test]
		public void WhenMaterialClickSecondary_01_Unchecked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			var materialSecondaryUncheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Unchecked"));
			App.ScrollDownTo("Material_Secondary_Unchecked");
			materialSecondaryUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("UNCHECKED", materialSecondaryUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(materialSecondaryUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the DisabledUnchecked option  in checkbox for material Secondary.*/
		[Test]
		public void WhenMaterialClickSecondary_02_DisabledUnchecked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Disabled_Unchecked"));
			App.ScrollDownTo("Material_Secondary_Disabled_Unchecked");
			materialSecondaryDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue("Content"));			
			Assert.IsFalse(materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the Checked option in checkbox for material Secondary. */
		[Test]
		public void WhenMaterialClickSecondary_03_Checked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before UnChecked");			
			var materialSecondaryCheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Checked"));
			App.ScrollDownTo("Material_Secondary_Checked");
			materialSecondaryCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("CHECKED", materialSecondaryCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialSecondaryCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the DisabledChecked option  in checkbox for material Secondary. */
		[Test]
		public void WhenMaterialClickSecondary_04_DisabledChecked()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledCheckedBox = new QueryEx(x => x.All().Marked("Material_Secondary_Disabled_Checked"));
			App.ScrollDownTo("Material_Secondary_Disabled_Checked");
			materialSecondaryDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("DISABLED CHECKED", materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
						
		}

		/* This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for material Secondary.*/
		[Test]
		public void WhenMaterialClickSecondary_05_Indeterminate()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before UnChecked");			
			var materialSecondaryCheckedIndeterminateBox = new QueryEx(x => x.All().Marked("Material_Secondary_Indeterminate"));
			App.ScrollDownTo("Material_Secondary_Indeterminate");
			materialSecondaryCheckedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("INDETERMINATE", materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			materialSecondaryCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			materialSecondaryCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			var isChecked = materialSecondaryCheckedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}

		/* This function is to test the DisabledIndeterminate option  in checkbox for material Secondary */
		[Test]
		public void WhenMaterialClickSecondary_06_DisabledIndeterminate()
		{
			NavigateToSample("CheckBox", "Material");

			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledIndeterminateBox = App.WaitThenTap("Material_Secondary_Disabled_Indeterminate").ToQueryEx();
			App.ScrollDownTo("Material_Secondary_Disabled_Indeterminate");
			TakeScreenshot("After Checked");
			Assert.IsFalse(materialSecondaryDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = materialSecondaryDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			Assert.AreEqual("DISABLED INDETERMINATE", materialSecondaryDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
		}
	}
}
