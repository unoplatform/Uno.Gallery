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
			
			var RadioButton_Material_Unchecked = new QueryEx(x => x.All().Marked("RadioButton_Material_Unchecked"));
			var RadioButton_Material_Checked = new QueryEx(x => x.All().Marked("RadioButton_Material_Checked"));

			App.WaitForElement(RadioButton_Material_Unchecked);
			App.WaitForElement(RadioButton_Material_Checked);
			
			//var unCheckedContent = RadioButton_Material_Unchecked.GetDependencyPropertyValue("Content");
			//Console.WriteLine("The value of unCheckedContent is "+ unCheckedContent.ToString());
			//Assert.AreEqual("UNCHECKED", unCheckedContent.ToString());
			
			var unCheckedResult1 = RadioButton_Material_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(unCheckedResult1);
			//App.ScrollDownTo("RadioButton_Material_Unchecked");
			App.Tap(RadioButton_Material_Unchecked);
			var unCheckedResult2 = RadioButton_Material_Unchecked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(unCheckedResult2);
			Assert.AreEqual("UNCHECKED", RadioButton_Material_Unchecked.GetDependencyPropertyValue("Content"));

			var checkedResult1 = RadioButton_Material_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsFalse(checkedResult1);
			//App.ScrollDownTo("RadioButton_Material_Checked");
			App.Tap(RadioButton_Material_Checked);
			var checkedResult2 = RadioButton_Material_Checked.GetDependencyPropertyValue<bool>("IsChecked");
			Assert.IsTrue(checkedResult2);
			Assert.AreEqual("CHECKED", RadioButton_Material_Checked.GetDependencyPropertyValue("Content")); 			
		}

		     
		/* This function is to test the DisabledRadiobutton option  in Radiobutton for material.*/
		
		[Test]
		public void WhenRadioButtonMaterialClick_02_DisabledUnchecked()
		{
			NavigateToSample("RadioButton", "Material");


			TakeScreenshot("Before Checked");
			var materialDisabledUncheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Unchecked"));
			//App.ScrollDownTo("RadioButton_Material_Disabled_Unchecked");			
			materialDisabledUncheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED UNCHECKED", materialDisabledUncheckedRadioButton.GetDependencyPropertyValue("Content"));			
			Assert.IsFalse(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledUncheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));

			var materialDisabledCheckedRadioButton = new QueryEx(x => x.All().Marked("RadioButton_Material_Disabled_Checked"));
			//App.ScrollDownTo("RadioButton_Material_Disabled_Checked");
			materialDisabledCheckedRadioButton.Tap();
			TakeScreenshot("After Checked");
			Assert.AreEqual("DISABLED CHECKED", materialDisabledCheckedRadioButton.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(materialDisabledCheckedRadioButton.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
		
	}
}
