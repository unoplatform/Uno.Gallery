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
	public class Given_ToggleSwitch_03_Fluent : TestBase
	{

		/*This function is to test the Off option  in toggleswitch for material Secondary */
		[Test]
		public void WhenToggleSwitchFluentClick_01_Off()
		{
			NavigateToSample("ToggleSwitch", "Fluent");		

			TakeScreenshot("Before On");
			var toggleSwitchFluentOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Fluent_Off"));
			App.ScrollDownTo("ToggleSwitch_Fluent_Off");
			Assert.IsFalse(toggleSwitchFluentOff.GetDependencyPropertyValue<bool>("IsOn"));

			App.WaitForElement("ToggleSwitch_Fluent_Off");
			toggleSwitchFluentOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("OFF", toggleSwitchFluentOff.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(toggleSwitchFluentOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchFluentOff.GetDependencyPropertyValue<bool>("IsEnabled"));	   
		}

		/* This function is to test the On option  in toggleswitch for material Secondary	*/
		[Test]
		public void WhenToggleSwitchFluentClick_02_On()
		{
			NavigateToSample("ToggleSwitch", "Fluent");
			TakeScreenshot("Before Off");
			var toggleSwitchFluentOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Fluent_On"));
			//App.ScrollDownTo("ToggleSwitch_Fluent_On");
			Assert.IsTrue(toggleSwitchFluentOn.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchFluentOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("ON", toggleSwitchFluentOn.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(toggleSwitchFluentOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsTrue(toggleSwitchFluentOn.GetDependencyPropertyValue<bool>("IsEnabled"));
			//Assert.IsTrue(toggleSwitchOn.GetDependencyPropertyValue("Header").Equals("ON"));
		}


		/* This function is to test the Disabled Off option  in toggleswitch for material Secondary*/
		[Test]
		public void WhenToggleSwitchFluentClick_03_DisabledOff()
		{
			NavigateToSample("ToggleSwitch", "Fluent");

			TakeScreenshot("Before On");
			var toggleSwitchFluentDisabledOff = new QueryEx(x => x.All().Marked("ToggleSwitch_Fluent_Disabled_Off"));
			//App.ScrollDownTo("ToggleSwitch_Fluent_Disabled_Off");
			Assert.IsFalse(toggleSwitchFluentDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchFluentDisabledOff.Tap();
			TakeScreenshot("After On");
			Assert.AreEqual("DISABLED OFF", toggleSwitchFluentDisabledOff.GetDependencyPropertyValue("Header"));
			Assert.IsFalse(toggleSwitchFluentDisabledOff.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchFluentDisabledOff.GetDependencyPropertyValue<bool>("IsEnabled"));					
		}

		/* This function is to test the Disabled On option in toggleswitch for Fluent */
		[Test]
		public void WhenToggleSwitchFluentClick_04_DisabledOn()
		{
			NavigateToSample("ToggleSwitch", "Fluent");

			TakeScreenshot("Before Off");
			var toggleSwitchFluentDisabledOn = new QueryEx(x => x.All().Marked("ToggleSwitch_Fluent_Disabled_On"));
			//App.ScrollDownTo("ToggleSwitch_Fluent_Disabled_On");
			Assert.IsTrue(toggleSwitchFluentDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));

			toggleSwitchFluentDisabledOn.Tap();
			TakeScreenshot("After Off");
			Assert.AreEqual("DISABLED ON", toggleSwitchFluentDisabledOn.GetDependencyPropertyValue("Header"));
			Assert.IsTrue(toggleSwitchFluentDisabledOn.GetDependencyPropertyValue<bool>("IsOn"));
			Assert.IsFalse(toggleSwitchFluentDisabledOn.GetDependencyPropertyValue<bool>("IsEnabled"));
		}           
	}
}
