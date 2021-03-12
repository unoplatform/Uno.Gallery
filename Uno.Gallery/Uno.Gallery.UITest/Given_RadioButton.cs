using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_RadioButton : TestBase
	{
		protected string[] Sections => new[] { "Components", "RadioButton" };

		[Test]
		public void When_ClickMaterial()
		{
			NavigateToSection(Sections);
			ShowMaterialTheme();

			TakeScreenshot("Before Checked");

			var uncheckedBox = App.WaitThenTap(q => q.All().Marked("Material_Unchecked")).ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		[Test]
		public void When_ClickFluent()
		{
			NavigateToSection(Sections);
			ShowFluentTheme();

			TakeScreenshot("Before Checked");

			var uncheckedBox = App.WaitThenTap(q => q.All().Marked("Fluent_Unchecked")).ToQueryEx();

			TakeScreenshot("After Checked");

			Assert.IsTrue(uncheckedBox.GetDependencyPropertyValue<bool>("IsChecked"));
		}
	}
}
