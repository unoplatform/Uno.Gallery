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
	public class Given_NumberBox : TestBase 
	{
		[Test]
		[Ignore("""
			On Wasm: CI, it produces NaN for unknown reason. Locally, it types 3^3 and produces 27
			On iOS and Android: Fails with 'Timed out waiting for keyboard'
			""")]
		public void NumberBox_Fluent()
		{
			NavigateToSample("NumberBox", "Fluent");

			var Numbox_Simple = new QueryEx(x => x.All().Marked("NumberBox_Simple"));
			Numbox_Simple.EnterText("15");
			App.PressEnter();
			Assert.AreEqual(15, Numbox_Simple.GetDependencyPropertyValue<double>("Value"));


			var Numbox_Exp = new QueryEx(x => x.All().Marked("NumberBox_Expression"));
			Numbox_Exp.EnterText("1+3^3");
			App.PressEnter();
			Assert.AreEqual(28.00, Numbox_Exp.GetDependencyPropertyValue<double>("Value"));

			var Round = new QueryEx(x => x.All().Marked("NumberBox_RoundOff"));
			Round.EnterText("1.01");
			App.PressEnter();
			Assert.AreEqual(1, Round.GetDependencyPropertyValue<double>("Value"));
		}

	}
}
