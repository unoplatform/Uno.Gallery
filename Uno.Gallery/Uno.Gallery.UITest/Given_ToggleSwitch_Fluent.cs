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

	public class Given_ToggleSwitch_Fluent : TestBase
	{

		[Test]
		public void WhenToggleSwitchOff()
		{
			NavigateToSample("ToggleSwitch", "Fluent");

			TakeScreenshot("Before On");
			var offToggleSwitch = new QueryEx(x => x.All().Marked("fluent_Off"));
			App.ScrollDownTo("fluent_Off");
			offToggleSwitch.Tap();
			TakeScreenshot("After On");
			Assert.IsTrue(offToggleSwitch.GetDependencyPropertyValue<bool>("IsOn"));





		}

	}
}
