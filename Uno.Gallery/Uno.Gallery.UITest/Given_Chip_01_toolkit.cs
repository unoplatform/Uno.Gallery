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
	public class Given_Chip_01_toolkit : TestBase
	{

		/*This function is to test Input Filled - Enabled condition for chip control. */
		[Test]
		public void WhenClick_01_InputFilledEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filled Input-Enabled Chip			
			App.ScrollDownTo("ChipFilledInput_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledInput_Enabled"));
			Assert.IsTrue(chipFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.AreEqual("Enabled", chipFilledEnabled.GetDependencyPropertyValue("Content"));

			//Tap/Click on Filled Input-Enabled Chip and taking screenshot	
			chipFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Input Filled - Disabled condition for chip control. */
		[Test]
		public void WhenClick_02_InputFilledDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filled Input-Disabled Chip			
			App.ScrollTo("ChipFilledInput_Disabled");

			//Initial Validation 
			TakeScreenshot("Before Click");
			var chipFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledInput_Disabled"));
			Assert.AreEqual("Disabled", chipFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filled Input-Disabled Chip and taking screenshot
			chipFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Input Filled - Selected condition for chip control. */
		[Test]
		public void WhenClick_03_InputFilledSelected()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filled Input-Selected Chip
			App.ScrollTo("ChipFilledInput_Selected");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilledSelected = new QueryEx(x => x.All().Marked("ChipFilledInput_Selected"));
			Assert.AreEqual("Selected", chipFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filled Input-Selected Chip and taking screenshot
			chipFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Header Text and the Input Outlined -Enabled condition for chip control. */
		[Test]
		public void WhenClick_04_InputOutlinedEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Outlined Input-Enabled Chip			
			App.ScrollDownTo("ChipOutlinedInput_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipOutlinedEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Enabled"));
			Assert.AreEqual("Enabled", chipOutlinedEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipOutlinedEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Outlined Input-Enabled Chip and taking screenshot
			chipOutlinedEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Input Outlined - Disabled condition for chip control. */
		[Test]
		public void WhenClick_05_InputOutlinedDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Outlined Input-Disabled Chip			
			App.ScrollTo("ChipOutlinedInput_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipOutlinedDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Disabled"));
			Assert.AreEqual("Disabled", chipOutlinedDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Outlined Input-Disabled Chip and taking screenshot
			chipOutlinedDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to the Input Outlined - Selected condition for chip control. */
		[Test]
		public void WhenClick_06_InputOutlinedSelected()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Outlined Input-Selected Chip			
			App.ScrollTo("ChipOutlinedInput_Selected");

			//Initial Validation
			TakeScreenshot("Before Click");
			var ChipFilledSelected = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Selected"));
			Assert.AreEqual("Selected", ChipFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(ChipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(ChipFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Outlined Input-Selected Chip and taking screenshot
			ChipFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(ChipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

		}
		/*This function is to test the Enabled Choice Filled condition for chip control. */
		[Test]
		public void WhenClick_07_ChoiceFilledEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Choice Filled-Enabled Chip			
			App.ScrollDownTo("ChipFilledChoice_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var ChipChoiceFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledChoice_Enabled"));
			Assert.AreEqual("Enabled", ChipChoiceFilledEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(ChipChoiceFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(ChipChoiceFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Choice Filled-Enabled Chip and taking screenshot
			ChipChoiceFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(ChipChoiceFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		/*This function is to test the Disabled Choice Filled condition for chip control. */
		[Test]
		public void WhenClick_08_ChoiceFilledDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Choice Filled-Disabled Chip			
			App.ScrollTo("ChipFilledChoice_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var ChipChoiceFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledChoice_Disabled"));
			Assert.AreEqual("Disabled", ChipChoiceFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(ChipChoiceFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(ChipChoiceFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Choice Filled-Disabled Chip and taking screenshot	
			ChipChoiceFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(ChipChoiceFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(ChipChoiceFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Selected Choice Filled condition for chip control. */
		[Test]
		public void WhenClick_09_ChoiceFilledSelected()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Choice Filled-Selected Chip			
			App.ScrollTo("ChipFilledChoice_Selected");

			//Initial Validation
			TakeScreenshot("Before Click");
			var ChipChoiceFilledSelected = new QueryEx(x => x.All().Marked("ChipFilledChoice_Selected"));
			Assert.AreEqual("Selected", ChipChoiceFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(ChipChoiceFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(ChipChoiceFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Choice Filled-Selected Chip and taking screenshot	
			ChipChoiceFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(ChipChoiceFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Enabled Choice Outlined condition for chip control. */
		[Test]
		public void WhenClick_10_ChoiceOutlinedEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Choice Outlined-Enabled Chip			
			App.ScrollDownTo("ChipOutlinedChoice_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipOutlinedChoiceEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Enabled"));
			Assert.AreEqual("Enabled", chipOutlinedChoiceEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipOutlinedChoiceEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedChoiceEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Choice Outlined-Enabled Chip and taking screenshot
			chipOutlinedChoiceEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipOutlinedChoiceEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Disabled Choice Outlined condition for chip control. */
		[Test]
		public void WhenClick_11_ChoiceOutlinedDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Choice Outlined-Disabled Chip			
			App.ScrollTo("ChipOutlinedChoice_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipOutlinedChoiceDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Disabled"));
			Assert.AreEqual("Disabled", chipOutlinedChoiceDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipOutlinedChoiceDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Choice Outlined-Disabled Chip and taking screenshot
			chipOutlinedChoiceDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipOutlinedChoiceDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Selected Choice Outlined condition for chip control. */
		[Test]
		public void WhenClick_12_ChoiceOutlinedSelected()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Choice Outlined-Selected Chip			
			App.ScrollTo("ChipOutlinedChoice_Selected");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipOutlinedChoiceSelected = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Selected"));
			Assert.AreEqual("Selected", chipOutlinedChoiceSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipOutlinedChoiceSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipOutlinedChoiceSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Choice Outlined-Selected Chip and taking screenshot
			chipOutlinedChoiceSelected.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipOutlinedChoiceSelected.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Enabled Filter Filled condition for chip control. */
		[Test]
		public void WhenClick_13_FilterFilledEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filter Filled-Enabled Chip			
			App.ScrollDownTo("ChipFilledFilter_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilterFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledFilter_Enabled"));
			Assert.AreEqual("Enabled", chipFilterFilledEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilterFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filter Filled-Enabled Chip and taking screenshot
			chipFilterFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipFilterFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Disabled Filter Filled condition for chip control. */
		[Test]
		public void WhenClick_14_FilterFilledDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filter Filled-Disabled Chip			
			App.ScrollTo("ChipFilledFilter_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilterFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledFilter_Disabled"));
			Assert.AreEqual("Disabled", chipFilterFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipFilterFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Tap/Click on Filter Filled-Disabled Chip and taking screenshot
			chipFilterFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipFilterFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(chipFilterFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/*This function is to test the Selected Filter Filled condition for chip control. */
		[Test]
		public void WhenClick_15_FilterFilledSelected()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filter Filled-Selected Chip			
			App.ScrollTo("ChipFilledFilter_Selected");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilterFilledSelected = new QueryEx(x => x.All().Marked("ChipFilledFilter_Selected"));
			Assert.AreEqual("Selected", chipFilterFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipFilterFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filter Filled-Disabled Chip and taking screenshot
			chipFilterFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validationn
			Assert.IsFalse(chipFilterFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Enabled Filter Outlined condition for chip control. */
		[Test]
		public void WhenClick_16_FilterOutlinedEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filter Outlined-Enabled Chip			
			App.ScrollDownTo("ChipOutlinedFilter_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilterOutlinedEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedFilter_Enabled"));
			Assert.AreEqual("Enabled", chipFilterOutlinedEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterOutlinedEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilterOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filter Outlined-Enabled Chip and taking screenshot
			chipFilterOutlinedEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipFilterOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Disabled Filter Outlined condition for chip control. */
		[Test]
		public void WhenClick_17_FilterOutlinedDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filter Outlined-Disabled Chip			
			App.ScrollTo("ChipOutlinedFilter_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilterOutlinedDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedFilter_Disabled"));
			Assert.AreEqual("Disabled", chipFilterOutlinedDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filter Outlined-Disabled Chip and taking screenshot
			chipFilterOutlinedDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/*This function is to test the Selected Filter Outlined condition for chip control. */
		[Test]
		public void WhenClick_18_FilterOutlinedSelected()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Filter Outlined-Selected Chip			
			App.ScrollTo("ChipOutlinedFilter_Selected");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipFilterOutlinedSelected = new QueryEx(x => x.All().Marked("ChipOutlinedFilter_Selected"));
			Assert.AreEqual("Selected", chipFilterOutlinedSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterOutlinedSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipFilterOutlinedSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Filter Outlined-Selected Chip and taking screenshot
			chipFilterOutlinedSelected.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipFilterOutlinedSelected.GetDependencyPropertyValue<bool>("IsChecked"));
		}
		/*This function is to test the Enabled Action Filled condition for chip control. */
		[Test]
		public void WhenClick_19_ActionFilledEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Action Filled-Enabled Chip			
			App.ScrollDownTo("ChipFilledAction_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipActionFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledAction_Enabled"));
			Assert.AreEqual("Enabled", chipActionFilledEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipActionFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipActionFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Action Filled-Enabled Chip and taking screenshot
			chipActionFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipActionFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Disabled Action Filled condition for chip control. */
		[Test]
		public void WhenClick_20_ActionFilledDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Action Filled-Disabled Chip
			App.ScrollTo("ChipFilledAction_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var ChipActionFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledAction_Disabled"));
			Assert.AreEqual("Disabled", ChipActionFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Action Filled-Disabled Chip and taking screenshot
			ChipActionFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/*This function is to test the Enabled Action Outlined condition for chip control. */
		[Test]
		public void WhenClick_21_ActionOutlinedEnabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Action Outlined-Enabled Chip			
			App.ScrollDownTo("ChipOutlinedAction_Enabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipActionOutlinedEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedAction_Enabled"));
			Assert.AreEqual("Enabled", chipActionOutlinedEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipActionOutlinedEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipActionOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on Action Outlined-Enabled Chip and taking screenshot
			chipActionOutlinedEnabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(chipActionOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Disabled Action Outlined condition for chip control. */
		[Test]
		public void WhenClick_22_ActionOutlinedDisabled()
		{
			//Navigation to Chip control
			NavigateToSample("Chip", "Material");

			//Scrolling to Action Outlined-Disabled Chip
			App.ScrollTo("ChipOutlinedAction_Disabled");

			//Initial Validation
			TakeScreenshot("Before Click");
			var chipActionOutlinedDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedAction_Disabled"));
			Assert.AreEqual("Disabled", chipActionOutlinedDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			//Tap/Click on FilterAction Outlined-Disabled Chip and taking screenshot
			chipActionOutlinedDisabled.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
		}
	}
}
