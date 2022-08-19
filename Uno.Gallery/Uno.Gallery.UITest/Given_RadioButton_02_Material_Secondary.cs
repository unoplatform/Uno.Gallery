using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;
using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;

namespace Uno.Gallery.UITests
{
	//[Ignore("WIP: M3 Migration")]
	public class Given_RadioButton_02_Material_Secondary : TestBase
	{

		/* This function is to test the UncheckedRadioButton option in Radiobutton for material Secondary*/
		[Test]
		public void WhenRadioButtonMaterialSecondaryClick_01_Unchecked()
		{
			//Navigation to Radiobutton material control
			NavigateToSample("RadioButton", "Material");

			//Scrolling to SecondaryUnchecked option
			App.ScrollDownTo(x => x.All().Marked("RadioButton_Material_Secondary_Unchecked"));

			//Initial Declaration for checked and unchecked option.
			var RadioButton_Material_Secondary_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Unchecked"));
			var RadioButton_Material_Secondary_Checked = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Checked"));

			App.WaitForElement(RadioButton_Material_Secondary_Unchecked);
			App.WaitForElement(RadioButton_Material_Secondary_Checked);

			//Initial Validation for unchecked option
			Assert.AreEqual("UNCHECKED", RadioButton_Material_Secondary_Unchecked.GetDependencyPropertyValue("Content"));
			var unCheckedSecondaryResult1 = RadioButton_Material_Secondary_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedSecondaryResult1);

			//Tap/Click on Unchecked option
			App.Tap(RadioButton_Material_Secondary_Unchecked);

			//validation after clicking Unchecked option
			var unCheckedSecondaryResult2 = RadioButton_Material_Secondary_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedSecondaryResult2);

			//Scrolling to Checked option
			//App.ScrollDownTo(x => x.All().Marked("RadioButton_Material_Secondary_Checked"));

			//Initial Validation for checked option
			Assert.AreEqual("CHECKED", RadioButton_Material_Secondary_Checked.GetDependencyPropertyValue("Content"));
			var checkedSecondaryResult1 = RadioButton_Material_Secondary_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedSecondaryResult1);

			//Tap/Click on Checked option
			App.Tap(RadioButton_Material_Secondary_Checked);

			//validation after clicking Checked option
			var checkedSecondaryResult2 = RadioButton_Material_Secondary_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedSecondaryResult2);
		}

		/* This function is to test the DisabledRadiobutton option  in Radiobutton for material Secondary.*/

		[Test]
		public void WhenRadioButtonMaterialSecondaryClick_02_DisabledUnchecked()
		{
			//Navigation to Radiobutton material control
			NavigateToSample("RadioButton", "Material");

			//Scrolling to Disabled Unchecked option
			//App.ScrollDownTo(x => x.All().Marked("RadioButton_Material_Secondary_Disabled_Unchecked"));

			//Initial Validation for Disabled Unchecked option
			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Unchecked option
			materialSecondaryDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Unchecked option
			Assert.IsFalse(materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Scrolling to Disabled Checked option
			//App.ScrollDownTo(x => x.All().Marked("RadioButton_Material_Secondary_Disabled_Checked"));

			//Initial Validation for Disabled Checked option
			var materialSecondaryDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Checked option
			materialSecondaryDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Checked option
			Assert.IsTrue(materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

	}
}
