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
	public class Given_RadioButton_03_Cupertino : TestBase
	{

		[Test]
		public void WhenRadioButtonCupertinoClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Cupertino");

			TakeScreenshot("Before Tap");
			var cupertinoUncheckRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Unchecked"));

			App.ScrollDownTo(cupertinoUncheckRadioButton);

			cupertinoUncheckRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(cupertinoUncheckRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonCupertinoClick_02_Checked()
		{
			NavigateToSample("RadioButton", "Cupertino");

			TakeScreenshot("Before Tap");
			var cupertinoCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Checked"));

			App.ScrollDownTo(cupertinoCheckedRadioButton);

			cupertinoCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsTrue(cupertinoCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonCupertinoClick_03_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Cupertino");

			TakeScreenshot("Before Tap");
			var cupertinoDisabledUnCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Disabled_Unchecked"));

			App.ScrollDownTo(cupertinoDisabledUnCheckedRadioButton);

			cupertinoDisabledUnCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(cupertinoDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(cupertinoDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonCupertinoClick_04_DisabledChecked()
		{
			NavigateToSample("RadioButton", "Cupertino");

			TakeScreenshot("Before Tap");
			var cupertinoDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Cupertino_Disabled_Checked"));

			App.ScrollDownTo(cupertinoDisabledCheckedRadioButton);

			cupertinoDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(cupertinoDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}
	}
}
