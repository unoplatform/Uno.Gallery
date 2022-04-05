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

		/*This function is to test the Unchecked option  in checkbox for material */
		[Test]
		public void When_01_OutlinedCardClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var outlinedCardClick = new QueryEx(x => x.All().Marked("Outlined_Card"));
			//App.ScrollDownTo("Outlined_Card");
			outlinedCardClick.Tap();
			TakeScreenshot("After Click");
			Assert.IsTrue(outlinedCardClick.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Outlined card", outlinedCardClick.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With title and subtitle only", outlinedCardClick.GetDependencyPropertyValue("SubHeaderContent"));
		}

		/* This function is to test the DisabledUnchecked option  in checkbox for material.	*/
		[Test]
		public void When_02_ElevatedCardClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var elevatedCardClick = new QueryEx(x => x.All().Marked("Elevated_Card"));
			App.ScrollDownTo("Elevated_Card");
			elevatedCardClick.Tap();
			TakeScreenshot("After Click");
			Assert.IsTrue(elevatedCardClick.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Elevated card", elevatedCardClick.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With supporting content", elevatedCardClick.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", elevatedCardClick.GetDependencyPropertyValue("SupportingContent"));
		}

		/* This function is to test the Checked option in checkbox for material.	*/
		[Test]
		public void When_03_OutlinedCardWithMediaContentClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var outlinedCardMediaContent = new QueryEx(x => x.All().Marked("Outlined_Card_Media_Content"));
			App.ScrollDownTo("Outlined_Card_Media_Content");
			outlinedCardMediaContent.Tap();
			TakeScreenshot("After Click");
			Assert.IsTrue(outlinedCardMediaContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Outlined card", outlinedCardMediaContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With media content", outlinedCardMediaContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", outlinedCardMediaContent.GetDependencyPropertyValue("MediaContent"));
		}

		/* This function is to test the DisabledChecked option  in checkbox for material */
		[Test]
		public void When_04_ElevatedCardWithMediaContentClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var elevatedCardMediaContent = new QueryEx(x => x.All().Marked("Elevated_Card_Media_Content"));
			App.ScrollDownTo("Elevated_Card_Media_Content");
			elevatedCardMediaContent.Tap();
			TakeScreenshot("After Click");
			Assert.IsTrue(elevatedCardMediaContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Elevated card", elevatedCardMediaContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With media and supporting content", elevatedCardMediaContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", elevatedCardMediaContent.GetDependencyPropertyValue("SupportingContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", elevatedCardMediaContent.GetDependencyPropertyValue("MediaContent"));
		}

		/* This function is to test the Indeterminate for uncheck, recheck and indeterminate option in checkbox for material   */
		[Test]
		public void When_05_OutlinedCardWithMediaSupportingContent_SupplementalActionButtonClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var outlinedCardSupportingContent = new QueryEx(x => x.All().Marked("Outlined_Card_Supplemental_Action_Button"));
			App.ScrollDownTo("Outlined_Card_Supplemental_Action_Button");
			outlinedCardSupportingContent.Tap();
			TakeScreenshot("After Click");
			Assert.IsTrue(outlinedCardSupportingContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Outlined card", outlinedCardSupportingContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With media, supporting content and supplemental action button", outlinedCardSupportingContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", outlinedCardSupportingContent.GetDependencyPropertyValue("SupportingContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", outlinedCardSupportingContent.GetDependencyPropertyValue("MediaContent"));

			TakeScreenshot("Before Click");
			var outlinedCardSupplementalActionButton1 = new QueryEx(x => x.All().Marked("Action_1_Button"));
			outlinedCardSupplementalActionButton1.Tap();
			TakeScreenshot("After Click");
			Assert.AreEqual("ACTION 1", outlinedCardSupplementalActionButton1.GetDependencyPropertyValue("Content"));

			TakeScreenshot("Before Click");
			var outlinedCardSupplementalActionButton2 = new QueryEx(x => x.All().Marked("Action_2_Button"));
			outlinedCardSupplementalActionButton2.Tap();
			TakeScreenshot("After Click");
			Assert.AreEqual("ACTION 2", outlinedCardSupplementalActionButton2.GetDependencyPropertyValue("Content"));
		}


		/* This function is to test the DisabledIndeterminate option  in checkbox for material.*/

		[Test]
		public void When_06_ElevatedCardWithSmallMedia_SupportingContentClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var elevatedCardSupportingContent = new QueryEx(x => x.All().Marked("Elevated_Card_Small_Media_Supporting_Content"));
			App.ScrollDownTo("Elevated_Card_Small_Media_Supporting_Content");
			elevatedCardSupportingContent.Tap();
			TakeScreenshot("After Click");
			Assert.IsTrue(elevatedCardSupportingContent.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Elevated card", elevatedCardSupportingContent.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With small media and supporting content", elevatedCardSupportingContent.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("ms-appx:///Assets/SmallMedia.png", elevatedCardSupportingContent.GetDependencyPropertyValue("MediaContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", elevatedCardSupportingContent.GetDependencyPropertyValue("SupportingContent"));
		}
		[Test]
		public void When_07_DisabledOutlinedCardClick()
		{
			NavigateToSample("Card", "Material");

			TakeScreenshot("Before Click");
			var disabledOutlinedCard = new QueryEx(x => x.All().Marked("Disabled_Outlined_Card"));
			App.ScrollDownTo("Disabled_Outlined_Card");
			disabledOutlinedCard.Tap();
			TakeScreenshot("After Click");
			Assert.IsFalse(disabledOutlinedCard.GetDependencyPropertyValue<bool>("IsEnabled"));
			Assert.AreEqual("Disabled outlined card", disabledOutlinedCard.GetDependencyPropertyValue("HeaderContent"));
			Assert.AreEqual("With avatar and other content", disabledOutlinedCard.GetDependencyPropertyValue("SubHeaderContent"));
			Assert.AreEqual("ms-appx:///Assets/Avatar.png", disabledOutlinedCard.GetDependencyPropertyValue("AvatarContent"));
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor.", disabledOutlinedCard.GetDependencyPropertyValue("SupportingContent"));
			Assert.AreEqual("ms-appx:///Assets/LargeMedia.png", disabledOutlinedCard.GetDependencyPropertyValue("MediaContent"));
		}
	}
}
