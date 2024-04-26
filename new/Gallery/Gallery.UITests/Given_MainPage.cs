﻿using System;
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
	}
}
