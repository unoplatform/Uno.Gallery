using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_MainPage : TestBase
	{
		[Test]
		public void When_SmokeTest()
		{
			NavigateToSample("Overview", "Material");

			TakeScreenshot("Start");

			App.WaitThenTap("Material_FilledButton");

			TakeScreenshot("Finish");
		}

		[Test]
		public void When_BuildIdentityLabel_IsNonEmpty()
		{
			// Ensure the navigation pane is open so the PaneFooter is rendered.
			OpenNavView();

			App.WaitForElement("BuildIdentityLabel");

			var label = new QueryEx(x => x.All().Marked("BuildIdentityLabel"));
			var text = label.GetDependencyPropertyValue<string>("Text");

			TakeScreenshot("BuildIdentityLabel");

			Assert.IsNotEmpty(
				text,
				$"BuildIdentityLabel should display a non-empty build identity (e.g. '1.7.0-dev.42 | local | Native') but the actual value was: '{text}'");
		}
	}
}
