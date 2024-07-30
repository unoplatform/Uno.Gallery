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
	public class Given_RatingControl : TestBase
	{
		[Test]
		[Ignore("Flaky on CI. But is passing locally")]
		public void RatingControl()
		{
			NavigateToSample("RatingControl", "Fluent");
			var Rating = new QueryEx(x => x.All().Marked("Rating_Fluent"));
			var Minus_Button = new QueryEx(x => x.All().Marked("Minus_Fluent"));
			var Value = new QueryEx(x => x.All().Marked("Value_Fluent"));
			var Plus_Butoon = new QueryEx(x => x.All().Marked("Plus_Fluent"));
			var slider = new QueryEx(x => x.All().Marked("Slider_Fluent"));
			var Read = new QueryEx(x => x.All().Marked("ReadOnly_Fluent"));
			var Drag = new QueryEx(x => x.All().Marked("CanDrag_Fluent"));
			var Clear = new QueryEx(x => x.All().Marked("CLearOnly_Fluent"));
			var Caption = new QueryEx(x => x.All().Marked("Caption_Fluent"));
			var Rate = new QueryEx(x => x.All().Marked("Rating_Control"));
			var RatingControl_Clear = new QueryEx(x => x.All().Marked("Clear_Fluent"));



			Minus_Button.Tap();
  			Assert.AreEqual(2, Rate.GetDependencyPropertyValue<double>("Value"));
			Plus_Butoon.Tap();
     		Assert.AreEqual(3, Rate.GetDependencyPropertyValue<double>("Value"));
			Plus_Butoon.Tap();
			Assert.AreEqual(4, Rate.GetDependencyPropertyValue<double>("Value"));
			Plus_Butoon.Tap();
			Assert.AreEqual(5, Rate.GetDependencyPropertyValue<double>("Value"));
			Minus_Button.Tap();
			Assert.AreEqual(4, Rate.GetDependencyPropertyValue<double>("Value"));
			Plus_Butoon.Tap();
			Assert.AreEqual(5, Rate.GetDependencyPropertyValue<double>("Value"));
			RatingControl_Clear.Tap();
			Assert.AreEqual(1, Rate.GetDependencyPropertyValue<double>("Value"));
			Read.Tap();
			Assert.IsTrue(Read.GetDependencyPropertyValue<bool>("IsChecked"));
			Drag.Tap();
			Assert.IsTrue(Drag.GetDependencyPropertyValue<bool>("IsChecked"));
			Clear.Tap();
			Assert.IsTrue(Clear.GetDependencyPropertyValue<bool>("IsChecked"));
			Caption.EnterText("Darsh");
			Clear.Tap();
			Assert.AreEqual("Darsh", Rate.GetDependencyPropertyValue<string>("Caption"));




		}
	}
}
