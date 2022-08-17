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
	public class Given_Card_01 : TestBase
	{

		/*This function is to test the Outlined card option in Card Control */
		[Test]
		public void When_01_OutlinedCardClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Outlined card
			//App.ScrollDownTo("Outlined_Card");

			//Initial Validation
			TakeScreenshot("Before Click");
			var outlinedCardClick = new QueryEx(x => x.All().Marked("Outlined_Card"));
			Assert.IsTrue(outlinedCardClick.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Outlined card", outlinedCardClick.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With title and subtitle only", outlinedCardClick.GetDependencyPropertyValue("SubHeaderContent"));

			//Tap/Click on Outlined card and taking screenshot
			outlinedCardClick.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(outlinedCardClick.GetDependencyPropertyValue<bool>("IsEnabled"));			
		}

		/* This function is to test the Elevated card option in Card Control.	*/
		[Test]
		public void When_02_ElevatedCardClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Elevated card
			App.ScrollDownTo("Elevated_Card");

			//Initial Validation
			TakeScreenshot("Before Click");
			var elevatedCardClick = new QueryEx(x => x.All().Marked("Elevated_Card"));
			Assert.IsTrue(elevatedCardClick.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Elevated card", elevatedCardClick.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With supporting content", elevatedCardClick.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", elevatedCardClick.GetDependencyPropertyValue("SupportingContent"));

			//Tap/Click on Outlined card and taking screenshot
			elevatedCardClick.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(elevatedCardClick.GetDependencyPropertyValue<bool>("IsEnabled"));				
		}

		/* This function is to test the Outlined card with Media content option in Card Control.	*/
		[Test]
		public void When_03_OutlinedCardWithMediaContentClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Outlined card media content 
			App.ScrollDownTo("Outlined_Card_Media_Content");

			//Initial Validation
			TakeScreenshot("Before Click");
			var outlinedCardMediaContent = new QueryEx(x => x.All().Marked("Outlined_Card_Media_Content"));
			Assert.IsTrue(outlinedCardMediaContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Outlined card", outlinedCardMediaContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With media content", outlinedCardMediaContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", outlinedCardMediaContent.GetDependencyPropertyValue("MediaContent"));

			//Tap/Click on Outlined card media content and taking screenshot
			outlinedCardMediaContent.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(outlinedCardMediaContent.GetDependencyPropertyValue<bool>("IsEnabled"));		
		}

		/* This function is to test the Elevated card with media content option in Card Control. */
		[Test]
		public void When_04_ElevatedCardWithMediaContentClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Elevated card with media content 
			App.ScrollDownTo("Elevated_Card_Media_Content");

			//Initial Validation
			TakeScreenshot("Before Click");
			var elevatedCardMediaContent = new QueryEx(x => x.All().Marked("Elevated_Card_Media_Content"));
			Assert.IsTrue(elevatedCardMediaContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Elevated card", elevatedCardMediaContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With media and supporting content", elevatedCardMediaContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", elevatedCardMediaContent.GetDependencyPropertyValue("SupportingContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", elevatedCardMediaContent.GetDependencyPropertyValue("MediaContent"));

			//Tap/Click on Elevated card with media content and taking screenshot
			elevatedCardMediaContent.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(elevatedCardMediaContent.GetDependencyPropertyValue<bool>("IsEnabled"));			
		}

		/* This function is to test the Outlined card with Media and supporting content option
		 * with Action 1 and Action 2 Button in Card Control. */
		[Test]
		public void When_05_OutlinedCardWithMediaSupportingContent_SupplementalActionButtonClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Outlined Card supporting content
			App.ScrollDownTo("Outlined_Card_Supplemental_Action_Button");

			//Initial Validation
			TakeScreenshot("Before Click");
			var outlinedCardSupportingContent = new QueryEx(x => x.All().Marked("Outlined_Card_Supplemental_Action_Button"));
			Assert.IsTrue(outlinedCardSupportingContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Outlined card", outlinedCardSupportingContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With media, supporting content and supplemental action button", outlinedCardSupportingContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", outlinedCardSupportingContent.GetDependencyPropertyValue("SupportingContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", outlinedCardSupportingContent.GetDependencyPropertyValue("MediaContent"));

			//Tap/Click on outlined Card supporting content and taking screenshot
			outlinedCardSupportingContent.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(outlinedCardSupportingContent.GetDependencyPropertyValue<bool>("IsEnabled"));

			//Initial Declration for Action 1 Button
			TakeScreenshot("Before Click");
			var outlinedCardSupplementalActionButton1 = new QueryEx(x => x.All().Marked("Action_1_Button"));

			//Tap/Click on Action 1 Button
			outlinedCardSupplementalActionButton1.Tap();
			TakeScreenshot("After Click");

			//validation after Tap/Click on Action 1 Button
			Assert.AreEqual("ACTION 1", outlinedCardSupplementalActionButton1.GetDependencyPropertyValue("Content"));

			//Initial Declration for Action 2 Button
			TakeScreenshot("Before Click");
			var outlinedCardSupplementalActionButton2 = new QueryEx(x => x.All().Marked("Action_2_Button"));

			//Tap/Click on Action 2 Button
			outlinedCardSupplementalActionButton2.Tap();
			TakeScreenshot("After Click");

			//validation after Tap/Click on Action 2 Button
			Assert.AreEqual("ACTION 2", outlinedCardSupplementalActionButton2.GetDependencyPropertyValue("Content"));
		}

		/* This function is to test the Elevated card with Small media and supporting content option in Card Control.*/
		[Test]
		public void When_06_ElevatedCardWithSmallMedia_SupportingContentClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Elevated card small media content
			App.ScrollDownTo("Elevated_Card_Small_Media_Supporting_Content");

			//Initial Validation
			TakeScreenshot("Before Click");
			var elevatedCardSupportingContent = new QueryEx(x => x.All().Marked("Elevated_Card_Small_Media_Supporting_Content"));
			Assert.IsTrue(elevatedCardSupportingContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Elevated card", elevatedCardSupportingContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With small media and supporting content", elevatedCardSupportingContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("ms-appx:///Assets/SmallMedia.png", elevatedCardSupportingContent.GetDependencyPropertyValue("MediaContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", elevatedCardSupportingContent.GetDependencyPropertyValue("SupportingContent"));

			//Tap/Click on outlined Card supporting content and taking screenshot
			elevatedCardSupportingContent.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsTrue(elevatedCardSupportingContent.GetDependencyPropertyValue<bool>("IsEnabled"));
		}

		/* This function is to test the Disabled Outlined card option in Card Control.*/
		[Test]
		public void When_07_DisabledOutlinedCardClick()
		{
			//Navigation to Card control
			NavigateToSample("Card", "Material");

			// Scrolling to Disabled Outlined card 
			App.ScrollDownTo("Disabled_Outlined_Card");

			//Initial Validation
			TakeScreenshot("Before Click");
			var disabledOutlinedCard = new QueryEx(x => x.All().Marked("Disabled_Outlined_Card"));
			Assert.IsFalse(disabledOutlinedCard.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Disabled outlined card", disabledOutlinedCard.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With avatar and other content", disabledOutlinedCard.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("ms-appx:///Assets/Avatar.png", disabledOutlinedCard.GetDependencyPropertyValue("AvatarContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", disabledOutlinedCard.GetDependencyPropertyValue("SupportingContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", disabledOutlinedCard.GetDependencyPropertyValue("MediaContent"));

			//Tap/Click on Disabled Outlined and taking screenshot
			disabledOutlinedCard.Tap();
			TakeScreenshot("After Click");

			//After Tap/Click validation
			Assert.IsFalse(disabledOutlinedCard.GetDependencyPropertyValue<bool>("IsEnabled"));			
		}
	}
}
