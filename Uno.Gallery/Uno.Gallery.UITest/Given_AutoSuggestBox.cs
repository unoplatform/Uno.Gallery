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
	public class Given_AutoSuggestBox : TestBase
	{
		[Test]
		public void AutoSuggestBox_Default()
		{
			NavigateToSample("AutoSuggestBox", "Fluent");
			TakeScreenshot("Before Text");

		}

	}
}
