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
	public class Given_ToggleSwitch_04_Cupertino : TestBase
	{

		/*This function is to test the Off option  in toggleswitch for material Secondary */
		[Test]
		public void WhenToggleSwitchCupertinoClick_01_Off()
		{
			NavigateToSample("ToggleSwitch", "Cupertino");			

			TakeScreenshot("Before On");
			var toggleSwitchCupertinoOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Cupertino_Off"));
			App.ScrollDownTo("ToggleSwitch_Cupertino_Off");
			Assert.IsFalse(toggleSwitchCupertinoOff.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchCupertinoOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("OFF", toggleSwitchCupertinoOff.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(toggleSwitchCupertinoOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchCupertinoOff.GetDependencyPropertyValue<bool>("IsEnabled"));		   
		}

		/* This function is to test the On option  in toggleswitch for material Secondary	*/
		[Test]
		public void WhenToggleSwitchCupertinoClick_02_On()
		{
			NavigateToSample("ToggleSwitch", "Cupertino");
			TakeScreenshot("Before Off");
			var toggleSwitchCupertinoOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Cupertino_On"));
			App.ScrollDownTo("ToggleSwitch_Cupertino_On");
			Assert.IsTrue(toggleSwitchCupertinoOn.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchCupertinoOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("ON", toggleSwitchCupertinoOn.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(toggleSwitchCupertinoOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchCupertinoOn.GetDependencyPropertyValue<bool>("IsEnabled"));
			//Assert.IsTrue(toggleSwitchOn.GetDependencyPropertyValue("Header").Equals("ON"));
		}


		/* This function is to test the Disabled Off option  in toggleswitch for material Secondary*/
		[Test]
		public void WhenToggleSwitchCupertinoClick_03_DisabledOff()
		{
			NavigateToSample("ToggleSwitch", "Cupertino");

			TakeScreenshot("Before On");
			var toggleSwitchCupertinoDisabledOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Cupertino_Disabled_Off"));
			//App.ScrollDownTo("ToggleSwitch_Cupertino_Disabled_Off");
			Assert.IsFalse(toggleSwitchCupertinoDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			toggleSwitchCupertinoDisabledOff.Tap();

			TakeScreenshot("After On");
			Assert.AreEqual("DISABLED OFF", toggleSwitchCupertinoDisabledOff.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(toggleSwitchCupertinoDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchCupertinoDisabledOff.GetDependencyPropertyValue<bool>("IsEnabled"));					
		}

		/* This function is to test the Disabled On option in toggleswitch for Fluent */
		[Test]
		public void WhenToggleSwitchCupertinoClick_04_DisabledOn()
		{
			NavigateToSample("ToggleSwitch", "Cupertino");

			TakeScreenshot("Before Off");
			var toggleSwitchCupertinoDisabledOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Cupertino_Disabled_On"));
			//App.ScrollDownTo("ToggleSwitch_Cupertino_Disabled_On");
			toggleSwitchCupertinoDisabledOn.Tap();
			Assert.IsTrue(toggleSwitchCupertinoDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));

			TakeScreenshot("After Off");
			Assert.AreEqual("DISABLED ON", toggleSwitchCupertinoDisabledOn.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(toggleSwitchCupertinoDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchCupertinoDisabledOn.GetDependencyPropertyValue<bool>("IsEnabled"));
		}           
	}
}
