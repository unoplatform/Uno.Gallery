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
	public class Given_BurgerMenuToggleButton : TestBase
	{
		/*This function is to test the Burger Menu Toggle Button */
		[Test]
		public void WhenBurgerMenu_01_Click()
		{
			NavigateToSample("Overview");
			if (AppInitializer.GetLocalPlatform() == Platform.Android
				|| AppInitializer.GetLocalPlatform() == Platform.iOS)
			{
				//App.Repl();
				NavigateToSample("NavigationViewControl");
				var NavigationViewControl1 = new QueryEx(x => x.All().Marked("NavigationViewControl"));
				Assert.IsFalse(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneOpen"));
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneVisible"));

				TakeScreenshot("Before Checked");
				var BurgerMenuToggleButton = new QueryEx(x => x.All().Marked("NavViewToggleButton"));				
				BurgerMenuToggleButton.Tap();
				TakeScreenshot("After Checked");				
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneOpen"));
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneVisible"));

				BurgerMenuToggleButton.Tap();
				TakeScreenshot("After Checked");
				Assert.IsFalse(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneOpen"));				
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneVisible"));
			}
			if (AppInitializer.GetLocalPlatform() == Platform.Browser)
			{
				NavigateToSample("NavigationViewControl");
				var NavigationViewControl1 = new QueryEx(x => x.All().Marked("NavigationViewControl"));				
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneOpen"));
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneVisible"));

				TakeScreenshot("Before Checked");
				var BurgerMenuToggleButton = new QueryEx(x => x.All().Marked("NavViewToggleButton"));				
				BurgerMenuToggleButton.Tap();
				TakeScreenshot("After Checked");
				Assert.IsFalse(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneOpen"));
				Assert.IsFalse(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneVisible"));				

				BurgerMenuToggleButton.Tap();
				TakeScreenshot("After Checked");
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneOpen"));
				Assert.IsTrue(NavigationViewControl1.GetDependencyPropertyValue<bool>("IsPaneVisible"));				
			}

		}
	}
}

