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
	public class Given_RadioButton_02_Fluent : TestBase
	{
		/* This one is used to test the Unchecked option which we have in fluent */

		[Test]
		[Ignore("Failing in CI")]
		public void WhenRadioButtonFluentClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Fluent");

			var fluentUncheckRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Unchecked"));

			App.ScrollDownTo("RadioButton_Fluent_Unchecked");

			fluentUncheckRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(fluentUncheckRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonFluentClick_02_Checked()
		{
			NavigateToSample("RadioButton", "Fluent");

			TakeScreenshot("Before Tap");
			var fluentCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Checked"));

			App.ScrollDownTo("RadioButton_Fluent_Checked");

			fluentCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(fluentCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonFluentClick_03_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Fluent");

			TakeScreenshot("Before Tap");
			var fluentDisabledUnCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Disabled_Unchecked"));

			App.ScrollDownTo("RadioButton_Fluent_Disabled_Unchecked");


			App.ScrollDownTo("RadioButton_Material_Disabled_Checked");

			fluentDisabledUnCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(fluentDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(fluentDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
