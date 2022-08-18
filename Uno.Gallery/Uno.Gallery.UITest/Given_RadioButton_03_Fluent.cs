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
	public class Given_RadioButton_03_Fluent : TestBase
	{		    
		/* This function is to test the UncheckedRadioButton option  in Radiobutton for Fluent*/
		
		[Test]
		public void WhenRadioButtonFluentClick_01_Unchecked()
		{
			//Navigation to Radiobutton Fluent control
			NavigateToSample("RadioButton", "Fluent");

			//Scrolling to Fluent Unchecked option
			App.ScrollDownTo("RadioButton_Fluent_Unchecked");

			//Initial Declaration for checked and unchecked option.
			var RadioButton_Fluent_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Unchecked"));
			var RadioButton_Fluent_Checked = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Checked"));			

			App.WaitForElement(RadioButton_Fluent_Unchecked);
			App.WaitForElement(RadioButton_Fluent_Checked);

			//Initial Validation for unchecked option
			Assert.AreEqual("UNCHECKED", RadioButton_Fluent_Unchecked.GetDependencyPropertyValue("Content"));
			var unCheckedFluentResult1 = RadioButton_Fluent_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedFluentResult1);

			//Tap/Click on Unchecked option
			App.Tap(RadioButton_Fluent_Unchecked);

			//validation after clicking Unchecked option
			var unCheckedFluentResult2 = RadioButton_Fluent_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedFluentResult2);

			//Scrolling to Checked option
			//App.ScrollDownTo("RadioButton_Material_Checked");

			//Initial Validation for checked option
			Assert.AreEqual("CHECKED", RadioButton_Fluent_Checked.GetDependencyPropertyValue("Content"));
			var checkedFluentResult1 = RadioButton_Fluent_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedFluentResult1);

			//Tap/Click on Checked option
			App.Tap(RadioButton_Fluent_Checked);

			//validation after clicking Checked option
			var checkedFluentResult2 = RadioButton_Fluent_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedFluentResult2);						
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for Fluent.*/
		
		[Test]
		public void WhenRadioButtonFluentClick_02_DisabledUnchecked()
		{
			//Navigation to Radiobutton Fluent control
			NavigateToSample("RadioButton", "Fluent");

			//Scrolling to Disabled Unchecked option
			//App.ScrollDownTo("RadioButton_Fluent_Disabled_Unchecked");

			//Initial Validation for Disabled Unchecked option
			TakeScreenshot("Before Checked");
			var fluentDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", fluentDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Unchecked option
			fluentDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Unchecked option
			Assert.IsFalse(fluentDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(fluentDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Scrolling to Disabled Checked option
			//App.ScrollDownTo("RadioButton_Fluent_Disabled_Checked");

			//Initial Validation for Disabled Checked option
			TakeScreenshot("Before Checked");
			var fluentDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", fluentDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Checked option
			fluentDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Checked option
			Assert.IsTrue(fluentDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(fluentDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
