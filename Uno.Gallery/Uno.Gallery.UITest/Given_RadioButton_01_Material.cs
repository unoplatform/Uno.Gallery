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
	public class Given_RadioButton_01_Material : TestBase
	{
		    
		/* This function is to test the UncheckedRadioButton option  in Radiobutton for material*/
		
		[Test]
		public void WhenRadioButtonMaterialClick_01_Unchecked()
		{
			//Navigation to Radiobutton material control
			NavigateToSample("RadioButton", "Material");

			//Scrolling to Unchecked option
			//App.ScrollDownTo("RadioButton_Material_Unchecked");

			//Initial Declaration for checked and unchecked option.
			var RadioButton_Material_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Material_Unchecked"));
			var RadioButton_Material_Checked = new QueryEx(x => x.All().Marked("RadioButton_Material_Checked"));

			App.WaitForElement(RadioButton_Material_Unchecked);
			App.WaitForElement(RadioButton_Material_Checked);
			

			//Initial Validation for unchecked option
			Assert.AreEqual("UNCHECKED", RadioButton_Material_Unchecked.GetDependencyPropertyValue("Content"));
			var unCheckedResult1 = RadioButton_Material_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedResult1);

			//Tap/Click on Unchecked option
			App.Tap(RadioButton_Material_Unchecked);

			//validation after clicking Unchecked option
			var unCheckedResult2 = RadioButton_Material_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedResult2);

			//Scrolling to Checked option
			//App.ScrollDownTo("RadioButton_Material_Checked");

			//Initial Validation for checked option
			Assert.AreEqual("CHECKED", RadioButton_Material_Checked.GetDependencyPropertyValue("Content"));
			var checkedResult1 = RadioButton_Material_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedResult1);

			//Tap/Click on Checked option
			App.Tap(RadioButton_Material_Checked);

			//validation after clicking Checked option
			var checkedResult2 = RadioButton_Material_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedResult2);
			 			
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for material.*/
		
		[Test]
		public void WhenRadioButtonMaterialClick_02_DisabledUnchecked()
		{
			//Navigation to Radiobutton material control
			NavigateToSample("RadioButton", "Material");

			//Scrolling to Disabled Unchecked option
			//App.ScrollDownTo("RadioButton_Material_Disabled_Unchecked");

			//Initial Validation for Disabled Unchecked option
			TakeScreenshot("Before Checked");
			var materialDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Unchecked"));
			Assert.AreEqual("DISABLED UNCHECKED", materialDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Unchecked option
			materialDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Unchecked option
			Assert.IsFalse(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Scrolling to Disabled Checked option
			//App.ScrollDownTo("RadioButton_Material_Disabled_Checked");

			//Initial Validation for Disabled Checked option
			TakeScreenshot("Before Checked");
			var materialDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Checked"));
			Assert.AreEqual("DISABLED CHECKED", materialDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));

			//Tap/Click on Disabled Checked option
			materialDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");

			//validation after clicking Disabled Checked option
			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
