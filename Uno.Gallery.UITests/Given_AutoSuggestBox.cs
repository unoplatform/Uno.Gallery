using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using NUnit.Framework;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;
using Uno.UITests.Helpers;


namespace Uno.Gallery.UITests
{
	public class Given_AutoSuggestBox : TestBase
	{
		[Test]
		public void AutoSuggestBox_Default()
		{
			NavigateToSample("AutoSuggestBox", "Fluent");
			//TakeScreenshot("Before Text");
			var AutoSuggest = new QueryEx(x => x.All().Marked("fluent_AutoSuggestBox"));
			
			AutoSuggest.EnterText("PasswordBox");
			bool isSuggestionListOpen = AutoSuggest.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.AreEqual("PasswordBox", AutoSuggest.GetDependencyPropertyValue<string>("Text"));
			Assert.IsTrue(isSuggestionListOpen, "Suggestion list is open");

			AutoSuggest.ClearText();
			AutoSuggest.EnterText("Button");
			isSuggestionListOpen = AutoSuggest.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.AreEqual("Button", AutoSuggest.GetDependencyPropertyValue<string>("Text"));
			Assert.IsTrue(isSuggestionListOpen, "Suggestion list is open");

			AutoSuggest.ClearText();
			AutoSuggest.EnterText("RadioButton");
			isSuggestionListOpen = AutoSuggest.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.AreEqual("RadioButton", AutoSuggest.GetDependencyPropertyValue<string>("Text"));
			Assert.IsTrue(isSuggestionListOpen, "Suggestion list is open");

			AutoSuggest.ClearText();
			AutoSuggest.EnterText("xxx");
			isSuggestionListOpen=AutoSuggest.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.AreEqual("xxx", AutoSuggest.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(isSuggestionListOpen, "Suggestion list is open");


		}

		[Test]
		public void AutoSuggestBox_Default_Text()
		{
			NavigateToSample("AutoSuggestBox", "Fluent");
			var AutoSuggest_Text = new QueryEx(x => x.All().Marked("fluent_AutoSuggestBox_WithText"));

			AutoSuggest_Text.EnterText("PasswordBox");
			bool isSuggestionListOpen_Text = AutoSuggest_Text.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.IsTrue(isSuggestionListOpen_Text, "Suggestion list is open");
			Assert.AreEqual("PasswordBox", AutoSuggest_Text.GetDependencyPropertyValue<string>("Text"));

			AutoSuggest_Text.ClearText();
			AutoSuggest_Text.EnterText("Button");
			isSuggestionListOpen_Text = AutoSuggest_Text.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.IsTrue(isSuggestionListOpen_Text, "Suggestion list is open");
			Assert.AreEqual("Button", AutoSuggest_Text.GetDependencyPropertyValue<string>("Text"));

			AutoSuggest_Text.ClearText();
			AutoSuggest_Text.EnterText("RadioButton");
			isSuggestionListOpen_Text = AutoSuggest_Text.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.IsTrue(isSuggestionListOpen_Text, "Suggestion list is open");
			Assert.AreEqual("RadioButton", AutoSuggest_Text.GetDependencyPropertyValue<string>("Text"));

			AutoSuggest_Text.ClearText();
			AutoSuggest_Text.EnterText("xxx");
			isSuggestionListOpen_Text =AutoSuggest_Text.GetDependencyPropertyValue<bool>("IsSuggestionListOpen");
			Assert.AreEqual("xxx", AutoSuggest_Text.GetDependencyPropertyValue<string>("Text"));
			Assert.IsFalse(isSuggestionListOpen_Text, "Suggestion list is open");

		}
	}
	
}
