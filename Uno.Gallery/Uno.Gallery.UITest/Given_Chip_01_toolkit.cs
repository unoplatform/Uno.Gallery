visusing System;
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

		/*This function is to test the Header Text and Input Filled - Enabled condition for chip control. */
		[Test]
		public void WhenClick_01_InputFilledEnabled()
		{
			NavigateToSample("Chip", "Material");
			//App.Repl();
			// To verifying the Header Text is correct as "Input -Filled"						
			var textBlock = new QueryEx(x => x.All().Marked("InputFilledLabel"));
			Assert.AreEqual("Input - Filled", textBlock.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledInput_Enabled"));
			App.ScrollDownTo("ChipFilledInput_Enabled");
			Assert.IsTrue(chipFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.AreEqual("Enabled", chipFilledEnabled.GetDependencyPropertyValue("Content"));

			// Tap on Enabled Filled Chip and taking screenshot	
			chipFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
		}

		/*This function is to test the Input Filled - Disabled condition for chip control. */
		[Test]
		public void WhenClick_02_InputFilledDisabled()
		{
			// Initial  Verification 
			TakeScreenshot("Before Click");
			var chipFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledInput_Disabled"));
			Assert.AreEqual("Disabled", chipFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));
			//App.ScrollDownTo("ChipFilledInput_Disabled");

			// Tap on Disabled Filled Chip and taking screenshot
			chipFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap Verification
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Input Filled - Selected condition for chip control. */
		[Test]
		public void WhenClick_03_InputFilledSelected()
		{
			// Initial  Verification 
			TakeScreenshot("Before Click");
			var chipFilledSelected = new QueryEx(x => x.All().Marked("ChipFilledInput_Selected"));
			//App.ScrollDownTo("ChipFilledInput_Selected");		
			Assert.AreEqual("Selected", chipFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on selected Filled Chip and taking screenshot
			chipFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After tap Verification
			Assert.IsFalse(chipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Header Text and the Input Outlined -Enabled condition for chip control. */
		[Test]
		public void WhenClick_04_InputOutlinedEnabled()
		{
			NavigateToSample("Chip", "Material");
			//App.Repl();	
			// To verifying the Header Text is correct as "Input Outlined"						
			var textBlock2 = new QueryEx(x => x.All().Marked("InputOutlinedLabel"));
			Assert.AreEqual("Input - Outlined", textBlock2.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var chipOutlinedEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Enabled"));
			App.ScrollDownTo("ChipOutlinedInput_Enabled");
			Assert.AreEqual("Enabled", chipOutlinedEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipOutlinedEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on Enabled Outlined Chip and taking screenshot
			chipOutlinedEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Input Outlined - Disabled condition for chip control. */
		[Test]
		public void WhenClick_05_InputOutlinedDisabled()
		{
			// Initial verification
			TakeScreenshot("Before Click");
			var chipOutlinedDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Disabled"));
			//App.ScrollDownTo("ChipOutlinedInput_Disabled");
			Assert.AreEqual("Disabled", chipOutlinedDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on Disabled Outlined Chip and taking screenshot
			chipOutlinedDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to the Input Outlined - Selected condition for chip control. */
		[Test]
		public void WhenClick_06_InputOutlinedSelected()
		{
			// Initial verification
			TakeScreenshot("Before Click");
			var ChipFilledSelected = new QueryEx(x => x.All().Marked("ChipOutlinedInput_Selected"));
			//App.ScrollDownTo("ChipOutlinedInput_Selected");
			Assert.AreEqual("Selected", ChipFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(ChipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
			Assert.IsTrue(ChipFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));

			// Tap on Outlined selected Chip and taking screenshot
			ChipFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(ChipFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
			
		}
		/*This function is to test the Enabled Choice Filled condition for chip control. */
		[Test]
		public void WhenClick_07_ChoiceFilledEnabled()
		{
			NavigateToSample("Chip", "Material");
			// To verifying the Header Text is correct as "Choice -Filled"
			var textBlock3 = new QueryEx(x => x.All().Marked("ChoiceFilledLabel"));
			Assert.AreEqual("Choice - Filled", textBlock3.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var ChipChoiceFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledChoice_Enabled"));
			App.ScrollDownTo("ChipFilledChoice_Enabled");
			Assert.AreEqual("Enabled", ChipChoiceFilledEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(ChipChoiceFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(ChipChoiceFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			ChipChoiceFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(ChipChoiceFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}
		/*This function is to test the Disabled Choice Filled condition for chip control. */
		[Test]
		public void WhenClick_08_ChoiceFilledDisabled()
		{
			//App.ScrollDownTo("ChipFilledChoice_Disabled");

			// Initial verification
			TakeScreenshot("Before Click");			
			var ChipChoiceFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledChoice_Disabled"));
			Assert.AreEqual("Disabled", ChipChoiceFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(ChipChoiceFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));				

			// Tap on  Chip and taking screenshot	
			ChipChoiceFilledDisabled.Tap();
			TakeScreenshot("After Click");
			//After tap verification
			Assert.IsFalse(ChipChoiceFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));				
		}

		/*This function is to test the Selected Choice Filled condition for chip control. */
		[Test]
		public void WhenClick_09_ChoiceFilledSelected()
		{
			//App.ScrollDownTo("ChipFilledChoice_Selected");
			// Initial verification
			TakeScreenshot("Before Click");
			var ChipChoiceFilledSelected = new QueryEx(x => x.All().Marked("ChipFilledChoice_Selected"));
			Assert.IsTrue(ChipChoiceFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Selected", ChipChoiceFilledSelected.GetDependencyPropertyValue("Content"));			
			Assert.IsTrue(ChipChoiceFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			ChipChoiceFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(ChipChoiceFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));
			
		}
		/*This function is to test the Enabled Choice Outlined condition for chip control. */
		[Test]
		public void WhenClick_10_ChoiceOutlinedEnabled()
		{
			NavigateToSample("Chip", "Material");

			// To verifying the Header Text is correct as "Choice Outlined"
			var textBlock4 = new QueryEx(x => x.All().Marked("ChoiceOutlinedLabel"));
			Assert.AreEqual("Choice - Outlined", textBlock4.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var chipOutlinedChoiceEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Enabled"));
			App.ScrollDownTo("ChipOutlinedChoice_Enabled");
			Assert.AreEqual("Enabled", chipOutlinedChoiceEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipOutlinedChoiceEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipOutlinedChoiceEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipOutlinedChoiceEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipOutlinedChoiceEnabled.GetDependencyPropertyValue<bool>("IsChecked"));
			
		}
		/*This function is to test the Disabled Choice Outlined condition for chip control. */
		[Test]
		public void WhenClick_11_ChoiceOutlinedDisabled()
		{
			//App.ScrollDownTo("ChipOutlinedChoice_Disabled");
			// Initial verification
			TakeScreenshot("Before Click");
			var chipOutlinedChoiceDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Disabled"));
			Assert.AreEqual("Disabled", chipOutlinedChoiceDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipOutlinedChoiceDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));

			// Tap on  Chip and taking screenshot
			chipOutlinedChoiceDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipOutlinedChoiceDisabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Selected Choice Outlined condition for chip control. */
		[Test]
		public void WhenClick_12_ChoiceOutlinedSelected()
		{
			//App.ScrollDownTo("ChipOutlinedChoice_Selected");
			// Initial verification
			TakeScreenshot("Before Click");
			var chipOutlinedChoiceSelected = new QueryEx(x => x.All().Marked("ChipOutlinedChoice_Selected"));
			Assert.AreEqual("Selected", chipOutlinedChoiceSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipOutlinedChoiceSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipOutlinedChoiceSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipOutlinedChoiceSelected.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipOutlinedChoiceSelected.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Enabled Filter Filled condition for chip control. */
		[Test]
		public void WhenClick_13_FilterFilledEnabled()
		{
			NavigateToSample("Chip", "Material");

			// To verifying the Header Text is correct as "Filter -Filled"
			var textBlock5 = new QueryEx(x => x.All().Marked("FilterFilledLabel"));
			Assert.AreEqual("Filter - Filled", textBlock5.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilterFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledFilter_Enabled"));
			App.ScrollDownTo("ChipFilledFilter_Enabled");
			Assert.AreEqual("Enabled", chipFilterFilledEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilterFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipFilterFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipFilterFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Disabled Filter Filled condition for chip control. */
		[Test]
		public void WhenClick_14_FilterFilledDisabled()
		{
			//App.ScrollDownTo("ChipFilledFilter_Disabled");

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilterFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledFilter_Disabled"));
			Assert.AreEqual("Disabled", chipFilterFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipFilterFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));

			// Tap on  Chip and taking screenshot
			chipFilterFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipFilterFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Selected Filter Filled condition for chip control. */
		[Test]
		public void WhenClick_15_FilterFilledSelected()
		{
			//App.ScrollDownTo("ChipFilledFilter_Selected");

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilterFilledSelected = new QueryEx(x => x.All().Marked("ChipFilledFilter_Selected"));
			Assert.AreEqual("Selected", chipFilterFilledSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterFilledSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipFilterFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipFilterFilledSelected.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipFilterFilledSelected.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Enabled Filter Outlined condition for chip control. */
		[Test]
		public void WhenClick_16_FilterOutlinedEnabled()
		{
			NavigateToSample("Chip", "Material");
			// To verifying the Header Text is correct as "Filter -Outlined"
			var textBlock6 = new QueryEx(x => x.All().Marked("FilterOutlinedLabel"));
			Assert.AreEqual("Filter - Outlined", textBlock6.GetDependencyPropertyValue("Text"));

			App.ScrollDownTo("ChipOutlinedFilter_Enabled");

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilterOutlinedEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedFilter_Enabled"));
			Assert.AreEqual("Enabled", chipFilterOutlinedEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterOutlinedEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilterOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipFilterOutlinedEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipFilterOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Disabled Filter Outlined condition for chip control. */
		[Test]
		public void WhenClick_17_FilterOutlinedDisabled()
		{
			//App.ScrollTo("ChipOutlinedFilter_Disabled");	

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilterOutlinedDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedFilter_Disabled"));
			Assert.AreEqual("Disabled", chipFilterOutlinedDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipFilterOutlinedDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipFilterOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Selected Filter Outlined condition for chip control. */
		[Test]
		public void WhenClick_18_FilterOutlinedSelected()
		{
			App.ScrollTo("ChipOutlinedFilter_Selected");

			// Initial verification
			TakeScreenshot("Before Click");
			var chipFilterOutlinedSelected = new QueryEx(x => x.All().Marked("ChipOutlinedFilter_Selected"));
			Assert.AreEqual("Selected", chipFilterOutlinedSelected.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipFilterOutlinedSelected.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsTrue(chipFilterOutlinedSelected.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipFilterOutlinedSelected.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipFilterOutlinedSelected.GetDependencyPropertyValue<bool>("IsChecked"));			
		}
		/*This function is to test the Enabled Action Filled condition for chip control. */
		[Test]
		public void WhenClick_19_ActionFilledEnabled()
		{
			NavigateToSample("Chip", "Material");
			App.ScrollDownTo("ChipFilledAction_Enabled");

			// To verifying the Header Text is correct as "Action - Filled"
			var textBlock7 = new QueryEx(x => x.All().Marked("ActionFilledLabel"));
			Assert.AreEqual("Action - Filled", textBlock7.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var chipActionFilledEnabled = new QueryEx(x => x.All().Marked("ChipFilledAction_Enabled"));
			Assert.AreEqual("Enabled", chipActionFilledEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipActionFilledEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipActionFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipActionFilledEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipActionFilledEnabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Disabled Action Filled condition for chip control. */
		[Test]
		public void WhenClick_20_ActionFilledDisabled()
		{
			//App.ScrollTo("ChipFilledAction_Disabled");
			// Initial verification
			TakeScreenshot("Before Click");
			var ChipActionFilledDisabled = new QueryEx(x => x.All().Marked("ChipFilledAction_Disabled"));
			Assert.AreEqual("Disabled", ChipActionFilledDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			ChipActionFilledDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(ChipActionFilledDisabled.GetDependencyPropertyValue<bool>("IsChecked"));						
		}

		/*This function is to test the Enabled Action Outlined condition for chip control. */
		[Test]
		public void WhenClick_21_ActionOutlinedEnabled()
		{
			NavigateToSample("Chip", "Material");
			App.ScrollDownTo("ChipOutlinedAction_Enabled");

			// To verifying the Header Text is correct as "Action - Outlined"
			var textBlock8 = new QueryEx(x => x.All().Marked("ActionOutlinedLabel"));
			Assert.AreEqual("Action - Outlined", textBlock8.GetDependencyPropertyValue("Text"));

			// Initial verification
			TakeScreenshot("Before Click");
			var chipActionOutlinedEnabled = new QueryEx(x => x.All().Marked("ChipOutlinedAction_Enabled"));
			Assert.AreEqual("Enabled", chipActionOutlinedEnabled.GetDependencyPropertyValue("Content"));
			Assert.IsTrue(chipActionOutlinedEnabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipActionOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipActionOutlinedEnabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsTrue(chipActionOutlinedEnabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}

		/*This function is to test the Disabled Action Outlined condition for chip control. */
		[Test]
		public void WhenClick_22_ActionOutlinedDisabled()
		{
			//App.ScrollTo("ChipOutlinedAction_Disabled");
				
			// Initial verification
			TakeScreenshot("Before Click");
			var chipActionOutlinedDisabled = new QueryEx(x => x.All().Marked("ChipOutlinedAction_Disabled"));
			Assert.AreEqual("Disabled", chipActionOutlinedDisabled.GetDependencyPropertyValue("Content"));
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));

			// Tap on  Chip and taking screenshot
			chipActionOutlinedDisabled.Tap();
			TakeScreenshot("After Click");

			//After tap verification
			Assert.IsFalse(chipActionOutlinedDisabled.GetDependencyPropertyValue<bool>("IsChecked"));			
		}
	}
}
