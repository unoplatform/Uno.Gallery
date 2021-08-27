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
	public class Given_ToggleSwitch_01_Material : TestBase
	{

		/*This function is to test the Off option  in toggleswitch for material */
		[Test]
		public void WhenToggleSwitchMaterialClick_01_Off()
		{
			NavigateToSample("ToggleSwitch", "Material");			

			TakeScreenshot("Before On");
			var toggleSwitchOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Off"));
			//App.ScrollDownTo("ToggleSwitch_Material_Off");
			Assert.IsFalse(toggleSwitchOff.GetDependencyPropertyValue<bool>("IsOn"));
			toggleSwitchOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("OFF", toggleSwitchOff.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(toggleSwitchOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchOff.GetDependencyPropertyValue<bool>("IsEnabled"));	   
		}

		/* This function is to test the On option  in toggleswitch for material.	*/
		[Test]
		public void WhenToggleSwitchMaterialClick_02_On()
		{
			NavigateToSample("ToggleSwitch", "Material");
			TakeScreenshot("Before Off");
			var toggleSwitchOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_On"));
			//App.ScrollDownTo("ToggleSwitch_Material_On");
			Assert.IsTrue(toggleSwitchOn.GetDependencyPropertyValue<bool>("IsOn"));
			toggleSwitchOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("ON", toggleSwitchOn.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(toggleSwitchOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchOn.GetDependencyPropertyValue<bool>("IsEnabled"));			
		}


		/* This function is to test the Disabled Off option  in toggleswitch for material */
		[Test]
		public void WhenToggleSwitchMaterialClick_03_DisabledOff()
		{
			NavigateToSample("ToggleSwitch", "Material");

			TakeScreenshot("Before On");
			var toggleSwitchDisabledOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Disabled_Off"));
			//App.ScrollDownTo("ToggleSwitch_Material_Disabled_Off");
			Assert.IsFalse(toggleSwitchDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			toggleSwitchDisabledOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("DISABLED OFF", toggleSwitchDisabledOff.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(toggleSwitchDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchDisabledOff.GetDependencyPropertyValue<bool>("IsEnabled"));					
		}

		/* This function is to test the Disabled On option  in toggleswitch for material   */
		[Test]
		public void WhenToggleSwitchMaterialClick_04_DisabledOn()
		{
			NavigateToSample("ToggleSwitch", "Material");

			TakeScreenshot("Before Off");
			var toggleSwitchDisabledOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Material_Disabled_On"));
			//App.ScrollDownTo("ToggleSwitch_Material_Disabled_On");
			Assert.IsTrue(toggleSwitchDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));
			toggleSwitchDisabledOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("DISABLED ON", toggleSwitchDisabledOn.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(toggleSwitchDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchDisabledOn.GetDependencyPropertyValue<bool>("IsEnabled"));
		}           
	}
}
