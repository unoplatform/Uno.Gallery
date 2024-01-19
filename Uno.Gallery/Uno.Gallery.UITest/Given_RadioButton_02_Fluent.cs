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

			TakeScreenshot("Before Checked");
			var FluentUncheckRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Fluent_Unchecked"));
			App.ScrollDownTo("RadioButton_Fluent_Unchecked");


			FluentUncheckRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.IsTrue(FluentUncheckRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}
		
	}
}
