using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;

namespace Uno.Gallery.UITests
{
	public class Given_CalendarView : TestBase
	{
		/// <summary>
		/// CalendarView/Picker samples are not stable on non-Browser platforms.
		/// Skips the calling test unless the current platform is Browser (WASM).
		/// </summary>
		private static void SkipOnMobilePlatforms()
		{
			if (AppInitializer.GetLocalPlatform() != Platform.Browser)
			{
				Assert.Ignore(
					"CalendarView/Picker samples are not stable on non-Browser platforms. " +
					"https://github.com/unoplatform/Uno.Gallery/issues/1117 | review-date: 2026-11-23");
			}
		}

		[Test]
		public void When_CalendarViewMaterial()
		{
			SkipOnMobilePlatforms();
			NavigateToSample("CalendarView", "Material");

			TakeScreenshot("Material CalendarView");

			App.WaitForElement(q => q.All().Marked("Material_CalendarView"));
		}

		[Test]
		public void When_CalendarViewCupertino()
		{
			SkipOnMobilePlatforms();
			NavigateToSample("CalendarView", "Cupertino");

			TakeScreenshot("Cupertino CalendarView");

			App.WaitForElement(q => q.All().Marked("Cupertino_CalendarView"));
		}

		[Test]
		public void When_CalendarViewFluent()
		{
			SkipOnMobilePlatforms();
			NavigateToSample("CalendarView", "Fluent");

			TakeScreenshot("Fluent CalendarView");

			App.WaitForElement(q => q.All().Marked("Fluent_CalendarView"));
		}
	}
}
