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
	public class Given_CheckBox : TestBase
	{
		protected string[] Sections => new[] { "Components", "CheckBox" };

		[Test]
		public void When_ClickMaterial()
		{
			NavigateToSection(Sections);
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var uncheckedBox = App.WaitThenTap("Material_Unchecked").ToQueryEx();
			
			TakeScreenshot("After Checked");

			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		[Test]
		public void When_ClickFluent()
		{
			NavigateToSection(Sections);
			ShowFluentTheme();

			TakeScreenshot("Before Checked");

			var uncheckedBox = App.WaitThenTap("Fluent_Unchecked").ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
