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
			
			




		}
	}
	
}
