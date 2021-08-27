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
	public class Given_ToggleSwitch_02_Material_Secondary : TestBase
	{

		/*This function is to test the Off option  in toggleswitch for material Secondary */
		[Test]
		public void WhenToggleSwitchMaterialClickSecondary_01_Off()
		{
			NavigateToSample("ToggleSwitch", "Material");			

			TakeScreenshot("Before On");
			var secondaryToggleSwitchOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Secondary_Off"));
			App.ScrollDownTo("ToggleSwitch_Material_Secondary_Off");
			Assert.IsFalse(secondaryToggleSwitchOff.GetDependencyPropertyValue<bool>("IsOn"));

			secondaryToggleSwitchOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("OFF", secondaryToggleSwitchOff.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(secondaryToggleSwitchOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(secondaryToggleSwitchOff.GetDependencyPropertyValue<bool>("IsEnabled"));		   
		}

		/* This function is to test the On option  in toggleswitch for material Secondary	*/
		[Test]
		public void WhenToggleSwitchMaterialClickSecondary_02_On()
		{
			NavigateToSample("ToggleSwitch", "Material");
			TakeScreenshot("Before Off");
			var secondaryToggleSwitchOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Secondary_On"));
			//App.ScrollDownTo("ToggleSwitch_Material_Secondary_On");
			Assert.IsTrue(secondaryToggleSwitchOn.GetDependencyPropertyValue<bool>("IsOn"));

			secondaryToggleSwitchOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("ON", secondaryToggleSwitchOn.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(secondaryToggleSwitchOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(secondaryToggleSwitchOn.GetDependencyPropertyValue<bool>("IsEnabled"));
			//Assert.IsTrue(toggleSwitchOn.GetDependencyPropertyValue("Header").Equals("ON"));
		}


		/* This function is to test the Disabled Off option  in toggleswitch for material Secondary*/
		[Test]
		public void WhenToggleSwitchMaterialClickSecondary_03_DisabledOff()
		{
			NavigateToSample("ToggleSwitch", "Material");

			TakeScreenshot("Before On");
			var secondaryToggleSwitchDisabledOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Secondary_Disabled_Off"));
			//App.ScrollDownTo("ToggleSwitch_Material_Secondary_Disabled_Off");
			Assert.IsFalse(secondaryToggleSwitchDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));

			secondaryToggleSwitchDisabledOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("DISABLED OFF", secondaryToggleSwitchDisabledOff.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(secondaryToggleSwitchDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(secondaryToggleSwitchDisabledOff.GetDependencyPropertyValue<bool>("IsEnabled"));					
		}

		/* This function is to test the Disabled On option  in toggleswitch for material Secondary  */
		[Test]
		public void WhenToggleSwitchMaterialClickSecondary_04_DisabledOn()
		{
			NavigateToSample("ToggleSwitch", "Material");

			TakeScreenshot("Before Off");
			var secondaryToggleSwitchDisabledOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Secondary_Disabled_On"));
			//App.ScrollDownTo("ToggleSwitch_Material_Secondary_Disabled_On");
			Assert.IsTrue(secondaryToggleSwitchDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));

			secondaryToggleSwitchDisabledOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("DISABLED ON", secondaryToggleSwitchDisabledOn.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(secondaryToggleSwitchDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(secondaryToggleSwitchDisabledOn.GetDependencyPropertyValue<bool>("IsEnabled"));
		}           
	}
}
