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
		protected string[] Sections => new[] { "Overview" };

		[Test]
		public void When_SmokeTest()
		{
			NavigateToSection(Sections);

			TakeScreenshot("Start");

			App.WaitThenTap(q => q.All().Marked("MaterialContainedButton"));

			TakeScreenshot("Finish");
		}
	}
}
