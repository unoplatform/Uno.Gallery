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
	public class Given_PasswordBox_Cupertino : TestBase
	{

		[Test]
		public void WhenPasswordBoxEnabled()
		{
			NavigateToSample("PasswordBox", "Cupertino");

			TakeScreenshot("Before PasswordBox");

			var passwordBoxCupertino = new QueryEx(x => x.All().Marked("PasswordBox_Cupertino"));
			App.ScrollDownTo(passwordBoxCupertino);
			passwordBoxCupertino.EnterText("Uno platform");

			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("Uno platform", passwordBoxCupertino.GetDependencyPropertyValue<string>("Password"));

		}

		[Test]
		public void WhenPasswordBoxDisabled()
		{
			NavigateToSample("PasswordBox", "Cupertino");

			TakeScreenshot("Before PasswordBox");

			var passwordBox_Disbaled = new QueryEx(x => x.All().Marked("PasswordBox_Cupertino_Disabled"));
			App.ScrollDownTo(passwordBox_Disbaled);
			passwordBox_Disbaled.EnterText("Uno platform");


			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("", passwordBox_Disbaled.GetDependencyPropertyValue<string>("Password"));

		}


	}
}
