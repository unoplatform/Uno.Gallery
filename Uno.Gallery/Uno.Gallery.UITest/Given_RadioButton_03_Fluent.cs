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
			NavigateToSample("RadioButton", "Fluent");

			var RadioButton_Fluent_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Unchecked"));
			var RadioButton_Fluent_Checked = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Checked"));

			App.ScrollDownTo("RadioButton_Fluent_Unchecked");

			App.WaitForElement(RadioButton_Fluent_Unchecked);
			App.WaitForElement(RadioButton_Fluent_Checked);			

			var unCheckedFluentResult1 = RadioButton_Fluent_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedFluentResult1);			
			App.Tap(RadioButton_Fluent_Unchecked);
			var unCheckedFluentResult2 = RadioButton_Fluent_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedFluentResult2);
			Assert.AreEqual("UNCHECKED", RadioButton_Fluent_Unchecked.GetDependencyPropertyValue("Content"));

			var checkedFluentResult1 = RadioButton_Fluent_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedFluentResult1);
			//App.ScrollDownTo("RadioButton_Material_Checked");
			App.Tap(RadioButton_Fluent_Checked);
			var checkedFluentResult2 = RadioButton_Fluent_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedFluentResult2);
			Assert.AreEqual("CHECKED", RadioButton_Fluent_Checked.GetDependencyPropertyValue("Content"));			
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for Fluent.*/
		
		[Test]
		public void WhenRadioButtonFluentClick_02_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Fluent");			

			TakeScreenshot("Before Checked");
			var fluentDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Disabled_Unchecked"));
			//App.ScrollDownTo("RadioButton_Fluent_Disabled_Unchecked");
			fluentDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", fluentDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(fluentDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(fluentDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));			

			var fluentDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Disabled_Checked"));
			//App.ScrollDownTo("RadioButton_Fluent_Disabled_Checked");
			fluentDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED CHECKED", fluentDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(fluentDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(fluentDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
