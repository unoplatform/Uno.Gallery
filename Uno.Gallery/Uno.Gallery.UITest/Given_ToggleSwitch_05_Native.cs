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
	public class Given_ToggleSwitch_05_Native : TestBase
	{

		/*This function is to test the Off option  in toggleswitch for material Secondary */
		[Test]
		public void WhenToggleSwitchNativeClick_01_Off()
		{
			NavigateToSample("ToggleSwitch", "Native");			

			TakeScreenshot("Before On");
			var toggleSwitchNativeOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Native_Off"));
			App.ScrollDownTo("ToggleSwitch_Native_Off");
			Assert.IsFalse(toggleSwitchNativeOff.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchNativeOff.Tap();
			TakeScreenshot("After On");
			//Assert.AreEqual("Enabled", toggleSwitchNativeOff.GetDependencyPropertyValue("Text"));
			Assert.IsTrue(toggleSwitchNativeOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchNativeOff.GetDependencyPropertyValue<bool>("IsEnabled"));			
		   
		}

		/* This function is to test the On option  in toggleswitch for material Secondary	*/
		[Test]
		public void WhenToggleSwitchNativeClick_02_On()
		{
			NavigateToSample("ToggleSwitch", "Native");
			TakeScreenshot("Before Off");
			var toggleSwitchNativeOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Native_On"));
			//App.ScrollDownTo("ToggleSwitch_Native_On");
			Assert.IsTrue(toggleSwitchNativeOn.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchNativeOn.Tap();
			TakeScreenshot("After Off");		
			Assert.IsFalse(toggleSwitchNativeOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchNativeOn.GetDependencyPropertyValue<bool>("IsEnabled"));			
		}


		/* This function is to test the Disabled Off option  in toggleswitch for material Secondary*/
		[Test]
		public void WhenToggleSwitchNativeClick_03_DisabledOff()
		{
			NavigateToSample("ToggleSwitch", "Native");

			TakeScreenshot("Before On");
			var toggleSwitchNativeDisabledOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Native_Disabled_Off"));
			App.ScrollDownTo("ToggleSwitch_Native_Disabled_Off");
			//Assert.AreEqual("Disabled", toggleSwitchNativeDisabledOff.GetDependencyPropertyValue("?"));
			Assert.IsFalse(toggleSwitchNativeDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchNativeDisabledOff.Tap();
			TakeScreenshot("After On");			
			Assert.IsFalse(toggleSwitchNativeDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchNativeDisabledOff.GetDependencyPropertyValue<bool>("IsEnabled"));					
		}

		/* This function is to test the Disabled On option in toggleswitch for Fluent */
		[Test]
		public void WhenToggleSwitchNativeClick_04_DisabledOn()
		{
			NavigateToSample("ToggleSwitch", "Native");

			TakeScreenshot("Before Off");
			var toggleSwitchNativeDisabledOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Native_Disabled_On"));
			//App.ScrollDownTo("ToggleSwitch_Native_Disabled_On");
			Assert.IsTrue(toggleSwitchNativeDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchNativeDisabledOn.Tap();
			TakeScreenshot("After Off");			
			Assert.IsTrue(toggleSwitchNativeDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchNativeDisabledOn.GetDependencyPropertyValue<bool>("IsEnabled"));
		}           
	}
}
