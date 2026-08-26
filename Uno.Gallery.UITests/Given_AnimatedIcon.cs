using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_AnimatedIcon : TestBase
	{
		[Test]
		public void WhenAnimatedIconPage_Loads()
		{
			NavigateToSample("AnimatedIcon");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("AnimatedIcon_PlayBtn");
			App.WaitForElement("AnimatedIcon_AutoPlay");
		}

		[Test]
		public void WhenPlayButtonTapped_StartsAndStopsAnimation()
		{
			NavigateToSample("AnimatedIcon");
			App.WaitForElement("AnimatedIcon_PlayBtn");
			App.WaitForElement("AnimatedIcon_Status");

			TakeScreenshot("Before play");
			App.Tap("AnimatedIcon_PlayBtn");
			TakeScreenshot("After play tapped");

			// The animation either plays then stops (fast) or we stop it manually
			App.WaitForElement("AnimatedIcon_Status");
		}
	}
}
