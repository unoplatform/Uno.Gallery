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
	public class Given_RadioButton_02_Material_Secondary : TestBase
	{
		    
		/* This function is to test the UncheckedRadioButton option  in Radiobutton for material Secondary*/
		
		[Test]
		public void WhenRadioButtonMaterialSecondaryClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Material");

			var RadioButton_Material_Secondary_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Unchecked"));
			var RadioButton_Material_Secondary_Checked = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Checked"));			

			App.ScrollDownTo("RadioButton_Material_Secondary_Unchecked");

			App.WaitForElement(RadioButton_Material_Secondary_Unchecked);
			App.WaitForElement(RadioButton_Material_Secondary_Checked);

			var unCheckedSecondaryResult1 = RadioButton_Material_Secondary_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedSecondaryResult1);
			//App.ScrollDownTo("RadioButton_Material_Secondary_Unchecked");
			App.Tap(RadioButton_Material_Secondary_Unchecked);
			var unCheckedSecondaryResult2 = RadioButton_Material_Secondary_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedSecondaryResult2);
			Assert.AreEqual("UNCHECKED", RadioButton_Material_Secondary_Unchecked.GetDependencyPropertyValue("Content"));

			var checkedSecondaryResult1 = RadioButton_Material_Secondary_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedSecondaryResult1);
			//App.ScrollDownTo("RadioButton_Material_Secondary_Checked");
			App.Tap(RadioButton_Material_Secondary_Checked);
			var checkedSecondaryResult2 = RadioButton_Material_Secondary_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedSecondaryResult2);
			Assert.AreEqual("CHECKED", RadioButton_Material_Secondary_Checked.GetDependencyPropertyValue("Content"));						
		}
		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for material Secondary.*/
		
		[Test]
		public void WhenRadioButtonMaterialSecondaryClick_02_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");			

			//App.ScrollDownTo("RadioButton_Material_Secondary_Disabled_Unchecked");
			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Disabled_Unchecked"));
			materialSecondaryDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));			
			Assert.IsFalse(materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));

			var materialSecondaryDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Secondary_Disabled_Checked"));
			//App.ScrollDownTo("RadioButton_Material_Secondary_Disabled_Checked");
			materialSecondaryDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED CHECKED", materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
