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
	public class Given_RadioButton_01_Material : TestBase
	{
		    
		/* This function is to test the UncheckedRadioButton option  in Radiobutton for material*/
		
		[Test]
		public void WhenRadioButtonMaterialClick_01_Unchecked()
		{
			NavigateToSample("RadioButton", "Material");
			Query RadioButton_Material_Unchecked = q => q.Marked("RadioButton_Material_Unchecked");
			Query RadioButton_Material_Checked = q => q.Marked("RadioButton_Material_Checked");

			App.WaitForElement(RadioButton_Material_Unchecked);
			App.WaitForElement(RadioButton_Material_Checked);

			var unCheckedResult1 = App.Query(q => RadioButton_Material_Unchecked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsFalse(unCheckedResult1);
			App.Tap(RadioButton_Material_Unchecked);
			var unCheckedResult2 = App.Query(q => RadioButton_Material_Unchecked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsTrue(unCheckedResult2);

			var checkedResult1 = App.Query(q => RadioButton_Material_Checked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsFalse(checkedResult1);
			App.Tap(RadioButton_Material_Checked);
			var checkedResult2 = App.Query(q => RadioButton_Material_Checked(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
			Assert.IsTrue(checkedResult2);

			/*TakeScreenshot("Before Checked");
			var uncheckedRadioButton = App.WaitThenTap("RadioButton_Material_Unchecked").ToQueryEx();			
			TakeScreenshot("After Checked");			
			Assert.IsTrue(uncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));*/
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for material.*/
		
		[Test]
		public void WhenRadioButtonMaterialClick_02_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");
			TakeScreenshot("Before Checked");
			var materialDisabledUncheckedRadioButton = new QueryEx(x => x.Marked("RadioButton_Material_Disabled_Unchecked"));
			materialDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));

			var materialDisabledCheckedRadioButton = new QueryEx(x => x.Marked("RadioButton_Material_Disabled_Checked"));
			materialDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.IsFalse(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		
	}
}
