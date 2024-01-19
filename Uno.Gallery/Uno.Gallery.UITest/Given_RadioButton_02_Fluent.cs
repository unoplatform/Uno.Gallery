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
		public void WhenRadioButtonFluentClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Fluent");

			TakeScreenshot("Before Tap");
			var FluentUncheckRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Unchecked"));

			App.ScrollDownTo("RadioButton_Fluent_Unchecked");

			FluentUncheckRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(FluentUncheckRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}
		[Test]

		public void WhenRadioButtonFluentClick_02_Checked()
		{
			NavigateToSample("RadioButton", "Fluent");

			TakeScreenshot("Before Tap");
			var FluentCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Checked"));

			App.ScrollDownTo("RadioButton_Fluent_Checked");

			FluentCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(FluentCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}
		[Test]
		public void WhenRadioButtonFluentClick_03_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Fluent");

			TakeScreenshot("Before Tap");
			var FluentDisabledUnCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Disabled_Unchecked"));

			App.ScrollDownTo("RadioButton_Fluent_Disabled_Unchecked");

			FluentDisabledUnCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(FluentDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(FluentDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}
		[Test]
		public void WhenRadioButtonFluentClick_04_DisabledChecked()
		{
			NavigateToSample("RadioButton", "Fluent");

			TakeScreenshot("Before Tap");
			var FluentDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Checked"));

			App.ScrollDownTo("RadioButton_Material_Disabled_Checked");

			FluentDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(FluentDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(FluentDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}
	}
}
