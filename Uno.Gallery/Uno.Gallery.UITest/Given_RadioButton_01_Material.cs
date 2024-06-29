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
	public class Given_RadioButton_01_Material : TestBase
	{
		
		[Test]
		public void WhenRadioButtonMaterialClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Checked");

			var uncheckedRadioButton = App.WaitThenTap("RadioButton_Material_Unchecked").ToQueryEx();
			App.ScrollDownTo(uncheckedRadioButton);
			uncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			Assert.IsTrue(uncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}


		[Test]
		public void WhenRadioButtonMaterialClick_02_Checked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Tap");

			var materialCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Checked"));
			App.ScrollDownTo(materialCheckedRadioButton);
			materialCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");

			Assert.IsTrue(materialCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonMaterialClick_03_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Tap");
			var materialDisabledUnCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Unchecked"));

			App.ScrollDownTo(materialDisabledUnCheckedRadioButton);

			materialDisabledUnCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse (materialDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(materialDisabledUnCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}

		[Test]
		public void WhenRadioButtonMaterialClick_04_DisabledChecked()
		{
			NavigateToSample("RadioButton", "Material");

			TakeScreenshot("Before Tap");
			var materialDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Checked"));

			App.ScrollDownTo(materialDisabledCheckedRadioButton);

			materialDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Tap");
			Assert.IsFalse(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

		}



	}
}
