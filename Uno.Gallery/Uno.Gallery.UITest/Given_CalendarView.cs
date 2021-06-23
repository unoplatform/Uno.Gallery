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
		[Test]
		public void When_CalendarViewMaterial()
		{
			NavigateToSample("CalendarView", "Material");

			TakeScreenshot("Material CalendarView");

			App.WaitForElement(q => q.All().Marked("Material_CalendarView"));
		}

		[Test]
		public void When_CalendarViewCupertino()
		{
			NavigateToSample("CalendarView", "Cupertino");

			TakeScreenshot("Cupertino CalendarView");

			App.WaitForElement(q => q.All().Marked("Cupertino_CalendarView"));
		}

		[Test]
		public void When_CalendarViewFluent()
		{
			NavigateToSample("CalendarView", "Fluent");

			TakeScreenshot("Fluent CalendarView");

			App.WaitForElement(q => q.All().Marked("Fluent_CalendarView"));
		}
	}
}
