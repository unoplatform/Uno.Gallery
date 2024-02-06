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
		public void WhenPasswordBoxNoHeaer() 
		{
			NavigateToSample("PasswordBox", "Fluent");

			TakeScreenshot("Before PasswordBox");

			var NoPasswordBox = new QueryEx(x => x.All().Marked("PasswordBox_NoHeader"));
			App.ScrollDownTo("PasswordBox_NoHeader");
			NoPasswordBox.EnterText("Uno platform");

			TakeScreenshot("After PasswordBox");
			Assert.IsTrue(NoPasswordBox.GetDependencyPropertyValue<string>("PasswordBox_NoHeader") == "Uno platform");






		}
	}
}
