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
	public class Given_CheckBox : TestBase
	{
		[Test]
		public void When_ClickMaterial()
		{
			if (AppInitializer.GetLocalPlatform() == Platform.Android)
			{
				Assert.Ignore();
			}

			NavigateToSection("Components", "CheckBox");

			TakeScreenshot("Opened");

			var uncheckedBox = App.WaitThenTap(q => q.All().Marked("Material_Unchecked")).ToQueryEx();

			TakeScreenshot("Opened");

			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
