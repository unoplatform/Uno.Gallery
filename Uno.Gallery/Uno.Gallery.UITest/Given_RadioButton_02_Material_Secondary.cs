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
			Query RadioButton_Material_Secondary_Unchecked = q => q.Marked("RadioButton_Material_Secondary_Unchecked");
			Query RadioButton_Material_Secondary_Checked = q => q.Marked("RadioButton_Material_Secondary_Checked");

			App.ScrollDownTo("RadioButton_Material_Secondary_Unchecked");

			App.WaitForElement(RadioButton_Material_Secondary_Unchecked);
			App.WaitForElement(RadioButton_Material_Secondary_Checked);

			var unCheckedSecondaryResult1 = App.Query(q => RadioButton_Material_Secondary_Unchecked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsFalse(unCheckedSecondaryResult1);
			App.Tap(RadioButton_Material_Secondary_Unchecked);
			var unCheckedSecondaryResult2 = App.Query(q => RadioButton_Material_Secondary_Unchecked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsTrue(unCheckedSecondaryResult2);

			var checkedSecondaryResult1 = App.Query(q => RadioButton_Material_Secondary_Checked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsFalse(checkedSecondaryResult1);
			App.Tap(RadioButton_Material_Secondary_Checked);
			var checkedSecondaryResult2 = App.Query(q => RadioButton_Material_Secondary_Checked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsTrue(checkedSecondaryResult2);

			/*TakeScreenshot("Before Checked");
			var uncheckedRadioButton = App.WaitThenTap("RadioButton_Material_Unchecked").ToQueryEx();			
			TakeScreenshot("After Checked");			
			Assert.IsTrue(uncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));*/
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for material.*/
		
		[Test]
		public void WhenRadioButtonMaterialSecondaryClick_02_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");

			App.ScrollDownTo("RadioButton_Material_Secondary_Disabled_Unchecked");
			TakeScreenshot("Before Checked");
			var materialSecondaryDisabledUncheckedRadioButton = new QueryEx(x => x.Marked("RadioButton_Material_Secondary_Disabled_Unchecked"));
			materialSecondaryDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(materialSecondaryDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

			var materialSecondaryDisabledCheckedRadioButton = new QueryEx(x => x.Marked("RadioButton_Material_Secondary_Disabled_Checked"));
			materialSecondaryDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialSecondaryDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		
	}
}
