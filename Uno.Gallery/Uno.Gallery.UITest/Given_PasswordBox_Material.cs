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
	public class Given_PasswordBox_Material : TestBase
	{

		[Test]
		public void WhenPasswordBoxDefault()
		{
			NavigateToSample("PasswordBox", "Material");

			TakeScreenshot("Before PasswordBox");

			var passwordBox_Defalut = new QueryEx(x => x.All().Marked("PasswordBox_Default"));
			App.ScrollDownTo(passwordBox_Defalut);
			passwordBox_Defalut.EnterText("Uno platform");


			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("Uno platform", passwordBox_Defalut.GetDependencyPropertyValue<string>("Password"));

		}

		//[Test]
		//public void WhenPasswordBoxDefaultDisabled()
		//{
		//	NavigateToSample("PasswordBox", "Material");

		//	TakeScreenshot("Before PasswordBox");

		//	var passwordBox_Defalut_Disbaled = new QueryEx(x => x.All().Marked("PasswordBox_Default_Disabled"));
		//	App.ScrollDownTo(passwordBox_Defalut_Disbaled);
		//	passwordBox_Defalut_Disbaled.EnterText("Uno platform");


		//	TakeScreenshot("After PasswordBox");
		//	Assert.AreEqual("", passwordBox_Defalut_Disbaled.GetDependencyPropertyValue<string>("Password"));

		//}

		[Test]
		[Ignore("Fails on CI")]
		public void WhenPasswordBoxOutlined()
		{
			NavigateToSample("PasswordBox", "Material");

			TakeScreenshot("Before PasswordBox");

			var passwordBox_outlined = new QueryEx(x => x.All().Marked("PasswordBox_Outlined"));
			App.ScrollDownTo(passwordBox_outlined);
			passwordBox_outlined.EnterText("Uno platform");


			TakeScreenshot("After PasswordBox");
			Assert.AreEqual("Uno platform", passwordBox_outlined.GetDependencyPropertyValue<string>("Password"));

		}

		//[Test]
		//public void WhenPasswordBoxOutlinedDisabled()
		//{
		//	NavigateToSample("PasswordBox", "Material");

		//	TakeScreenshot("Before PasswordBox");

		//	var passwordBox_outlined_disabled = new QueryEx(x => x.All().Marked("PasswordBox_Outlined_Disabled"));
		//	App.ScrollDownTo(passwordBox_outlined_disabled);
		//	passwordBox_outlined_disabled.EnterText("Uno platform");


		//	TakeScreenshot("After PasswordBox");
		//	Assert.AreEqual("", passwordBox_outlined_disabled.GetDependencyPropertyValue<string>("Password"));

		//}


	}
}
