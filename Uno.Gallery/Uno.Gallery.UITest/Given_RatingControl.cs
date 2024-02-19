using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Uno.UI.Xaml;
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
			Assert.AreEqual("Value:  2", Value.GetDependencyPropertyValue<string>("Text"));
			Plus_Butoon.Tap();
			Assert.AreEqual("Value:  3", Value.GetDependencyPropertyValue<string>("Text"));
			Plus_Butoon.Tap();
			Assert.AreEqual("Value:  4", Value.GetDependencyPropertyValue<string>("Text"));
			Plus_Butoon.Tap();
			Assert.AreEqual("Value:  5", Value.GetDependencyPropertyValue<string>("Text"));
			Minus_Button.Tap();
			Assert.AreEqual("Value:  4", Value.GetDependencyPropertyValue<string>("Text"));
			Assert.AreEqual("4", Rate.GetDependencyPropertyValue<string>("Value"));
			Plus_Butoon.Tap();
			Assert.AreEqual("Value:  5", Value.GetDependencyPropertyValue<string>("Text"));
			Assert.AreEqual("5", Rate.GetDependencyPropertyValue<string>("Value"));
			RatingControl_Clear.Tap();
			Assert.AreEqual("Value:  1", Value.GetDependencyPropertyValue<string>("Text"));
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
