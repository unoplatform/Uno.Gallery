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
			//Navigation to Radiobutton Cupertino control
			NavigateToSample("RadioButton", "Cupertino");

			//Scrolling to Unchecked option
			App.ScrollDownTo("RadioButton_Cupertino_Unchecked");

			//Initial Declaration for checked and unchecked option.
			var RadioButton_Cupertino_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Unchecked"));
			var RadioButton_Cupertino_Checked = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Checked"));
						
			App.WaitForElement(RadioButton_Cupertino_Unchecked);
			App.WaitForElement(RadioButton_Cupertino_Checked);

			//Initial Validation for unchecked option
			Assert.AreEqual("UNCHECKED", RadioButton_Cupertino_Unchecked.GetDependencyPropertyValue("Content"));
			var unCheckedCupertinoResult1 = RadioButton_Cupertino_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedCupertinoResult1);

			//Tap/Click on Unchecked option
			App.Tap(RadioButton_Cupertino_Unchecked);

			//validation after clicking Unchecked option
			var unCheckedCupertinoResult2 = RadioButton_Cupertino_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedCupertinoResult2);

			//Scrolling to Checked option
			//App.ScrollDownTo("RadioButton_Cupertino_Checked");

			//Initial Validation for checked option
			var checkedCupertinoResult1 = RadioButton_Cupertino_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedCupertinoResult1);
			Assert.AreEqual("CHECKED", RadioButton_Cupertino_Checked.GetDependencyPropertyValue("Content"));

			//Tap/Click on Checked option
			App.Tap(RadioButton_Cupertino_Checked);

			//validation after clicking Checked option
			var checkedCupertinoResult2 = RadioButton_Cupertino_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedCupertinoResult2);						
		}
		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for Cupertino.*/
		
		[Test]
		public void WhenRadioButtonCupertinoClick_02_DisabledUnchecked()
		{
			//Navigation to Radiobutton Cupertino control
			NavigateToSample("RadioButton", "Cupertino");

			//Scrolling to Unchecked option
			//App.ScrollDownTo("RadioButton_Cupertino_Disabled_Unchecked");

			//Initial Validation for Disabled Unchecked option
			TakeScreenshot("Before Checked");
			var cupertinoDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", cupertinoDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Unchecked option
			cupertinoDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Unchecked option
			Assert.IsFalse(cupertinoDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Scrolling to Disabled Checked option
			//App.ScrollDownTo("RadioButton_Cupertino_Disabled_Checked");

			//Initial Validation for Disabled Checked option
			TakeScreenshot("Before Checked");
			var cupertinoDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Checked option
			cupertinoDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Checked option
			Assert.IsTrue(cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
