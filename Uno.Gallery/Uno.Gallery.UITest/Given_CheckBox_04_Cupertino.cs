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
	public class Given_CheckBox_04_Cupertino : TestBase
	{
		     
        /* This function is to test the Unchecked option  in checkbox for Cupertino. */        
		[Test]
		public void WhenCupertinoClick_01_Unchecked()
		{
			NavigateToSample("CheckBox", "Cupertino");

			TakeScreenshot("Before Checked");
			var cupertinoUncheckedBox = new QueryEx(x => x.Marked("Cupertino_Unchecked"));
			App.ScrollDownTo("Cupertino_Unchecked");
			cupertinoUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("UNCHECKED", cupertinoUncheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(cupertinoUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
        /* This function is to test the DisabledChecked option  in checkbox for Cupertino. */        
		[Test]
		public void WhenCupertinoClick_02_DisabledUnChecked()
		{
			NavigateToSample("CheckBox", "Cupertino");

			TakeScreenshot("Before Checked");
			var cupertinoDisabledUncheckedBox = new QueryEx(x => x.All().Marked("Cupertino_Disabled_Unchecked"));
			App.ScrollDownTo("Cupertino_Disabled_Unchecked");
			cupertinoDisabledUncheckedBox.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", cupertinoDisabledUncheckedBox.GetDependencyPropertyValue("Content"));			
			Assert.IsFalse(cupertinoDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledUncheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		     
		 /* This function is to test the Checked option in checkbox for Cupertino.*/		 
		[Test]
		public void WhenCupertinoClick_03_Checked()
		{
			NavigateToSample("CheckBox", "Cupertino");


			TakeScreenshot("Before UnChecked");
			var cupertinoCheckedBox = new QueryEx(x => x.All().Marked("Cupertino_Checked"));
			App.ScrollDownTo("Cupertino_Checked");
			cupertinoCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("CHECKED", cupertinoCheckedBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(cupertinoCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		     
        /* This function is to test the DisabledUnchecked option  in checkbox for Cupertino.*/        
		[Test]
		public void WhenCupertinoClick_04_DisabledChecked()
		{
			NavigateToSample("CheckBox", "Cupertino");

			TakeScreenshot("Before Checked");
			var cupertinoDisabledCheckedBox = new QueryEx(x => x.All().Marked("Cupertino_Disabled_Checked"));
			App.ScrollDownTo("Cupertino_Disabled_Checked");
			cupertinoDisabledCheckedBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("DISABLED CHECKED", cupertinoDisabledCheckedBox.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(cupertinoDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledCheckedBox.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		     
        /* This function is to test the Indeterminate for uncheck, recheck and indeterminate option  in checkbox for Cupertino.*/        
		[Test]
		public void WhenCupertinoClick_05_Indeterminate()
		{
			NavigateToSample("CheckBox", "Cupertino");


			TakeScreenshot("Before UnChecked");
			var cupertinoCheckedIndeterminateBox = new QueryEx(x => x.All().Marked("Cupertino_Indeterminate"));
			App.ScrollDownTo("Cupertino_Indeterminate");
			cupertinoCheckedIndeterminateBox.Tap();
			TakeScreenshot("After UnChecked");
			Assert.AreEqual("INDETERMINATE", cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));

			cupertinoCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue<bool>("IsChecked"));

			cupertinoCheckedIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			var isChecked = cupertinoCheckedIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));			
		}
		
		     
        /* This function is to test the DisabledIndeterminate option  in checkbox for Cupertino. */        
		[Test]
		public void WhenCupertinoClick_06_DisabledIndeterminate()
		{
			NavigateToSample("CheckBox", "Cupertino");

			TakeScreenshot("Before Checked");
			var cupertinoDisabledIndeterminateBox = new QueryEx(x => x.All().Marked("Cupertino_Disabled_Indeterminate"));
			App.ScrollDownTo("Cupertino_Disabled_Indeterminate");
			cupertinoDisabledIndeterminateBox.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue<bool>("IsEnabled"));
			var isChecked = cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue("IsChecked");
			Assert.IsTrue(isChecked == null || string.IsNullOrWhiteSpace(isChecked as string));
			Assert.AreEqual("DISABLED INDETERMINATE", cupertinoDisabledIndeterminateBox.GetDependencyPropertyValue("Content"));
		}
	}
}
