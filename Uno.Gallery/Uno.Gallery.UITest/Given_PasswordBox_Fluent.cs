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
	//[Ignore("WIP: M3 Migration")]
	public class Given_PasswordBox_Fluent : TestBase
	{

		[Test]
		public void WhenPasswordBoxNoHeader() 
		{
			NavigateToSample("PasswordBox", "Fluent");

			TakeScreenshot("Before PasswordBox");

			var NoPasswordBox = new QueryEx(x => x.All().Marked("PasswordBox_NoHeader"));
			App.ScrollDownTo(NoPasswordBox);
			NoPasswordBox.EnterText("Uno platform");

			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("Uno platform", NoPasswordBox.GetDependencyPropertyValue<string>("Password"));

		}

		[Test]
		public void WhenPasswordBoxHeader()
		{
			NavigateToSample("PasswordBox", "Fluent");

			TakeScreenshot("Before PasswordBox");

			var PasswordBoxHeader = new QueryEx(x => x.All().Marked("PasswordBox_Header"));
			App.ScrollDownTo(PasswordBoxHeader);
			PasswordBoxHeader.EnterText("Uno platform");

			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("Uno platform", PasswordBoxHeader.GetDependencyPropertyValue<string>("Password"));

		}

		[Test]
		public void WhenPasswordBoxHeaderDisabled()
		{
			NavigateToSample("PasswordBox", "Fluent");

			TakeScreenshot("Before PasswordBox");

			var PasswordBoxDisabled = new QueryEx(x => x.All().Marked("PasswordBox_Disabled"));
			App.ScrollDownTo(PasswordBoxDisabled);
			PasswordBoxDisabled.EnterText("Uno platform");

			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("", PasswordBoxDisabled.GetDependencyPropertyValue<string>("Password"));

		}


	}
}
