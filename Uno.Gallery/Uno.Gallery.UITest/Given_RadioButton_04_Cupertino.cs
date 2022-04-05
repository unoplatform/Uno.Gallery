using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;
using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;

namespace Uno.Gallery.UITests
{
	public class Given_RadioButton_04_Cupertino : TestBase
	{
		    
		/* This function is to test the UncheckedRadioButton option  in Radiobutton for Cupertino*/
		
		[Test]
		public void WhenRadioButtonCupertinoClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Cupertino");

			var RadioButton_Cupertino_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Unchecked"));
			var RadioButton_Cupertino_Checked = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Checked"));

			App.ScrollDownTo("RadioButton_Cupertino_Unchecked");
			App.WaitForElement(RadioButton_Cupertino_Unchecked);
			App.WaitForElement(RadioButton_Cupertino_Checked);

			var unCheckedCupertinoResult1 = RadioButton_Cupertino_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedCupertinoResult1);			
			App.Tap(RadioButton_Cupertino_Unchecked);
			var unCheckedCupertinoResult2 = RadioButton_Cupertino_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedCupertinoResult2);
			Assert.AreEqual("UNCHECKED", RadioButton_Cupertino_Unchecked.GetDependencyPropertyValue("Content"));

			var checkedCupertinoResult1 = RadioButton_Cupertino_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedCupertinoResult1);
			//App.ScrollDownTo("RadioButton_Cupertino_Checked");
			App.Tap(RadioButton_Cupertino_Checked);
			var checkedCupertinoResult2 = RadioButton_Cupertino_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedCupertinoResult2);
			Assert.AreEqual("CHECKED", RadioButton_Cupertino_Checked.GetDependencyPropertyValue("Content"));			
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for Cupertino.*/
		
		[Test]
		public void WhenRadioButtonCupertinoClick_02_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Cupertino");

			TakeScreenshot("Before Checked");
			var cupertinoDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Disabled_Unchecked"));
			//App.ScrollDownTo("RadioButton_Cupertino_Disabled_Unchecked");
			cupertinoDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", cupertinoDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(cupertinoDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			

			var cupertinoDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Disabled_Checked"));
			//App.ScrollDownTo("RadioButton_Cupertino_Disabled_Checked");
			cupertinoDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED CHECKED", cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
