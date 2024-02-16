using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using NUnit.Framework;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;

namespace Uno.Gallery.UITests
{
	public class Given_RatingControl : TestBase
	{
		[Test]
		public void RatingControl()
		{
			NavigateToSample("RatingControl", "Fluent");
			var Rating = new QueryEx(x => x.All().Marked("Rating_Fluent"));
			var Minus = new QueryEx(x => x.All().Marked("Minus_Fluent"));
			var value = new QueryEx(x => x.All().Marked("Value_Fluent"));
			var Plus = new QueryEx(x => x.All().Marked("Plus_Fluent"));


			Minus.Tap();
			Assert.AreEqual("Value:  2", value.GetDependencyPropertyValue<string>("Text"));
			var clear = new QueryEx(x => x.All().Marked("Clear_Fluent"));
			Plus.Tap();
			Assert.AreEqual("Value:  2", value.GetDependencyPropertyValue<string>("Text"));
			Minus.Tap();
			Assert.AreEqual("Value:  1", value.GetDependencyPropertyValue<string>("Text"));
			Minus.Tap();
			Assert.AreEqual("Value:  3", value.GetDependencyPropertyValue<string>("Text"));




		}
	}
}
