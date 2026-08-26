using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	public class Given_AnimatedVisualPlayer : TestBase
	{
		[Test]
		public void WhenAnimatedVisualPlayerPage_Loads()
		{
			NavigateToSample("AnimatedVisualPlayer");
			TakeScreenshot("PageLoaded");

			App.WaitForElement("AnimatedVisualPlayer_PlayBtn");
			App.WaitForElement("AnimatedVisualPlayer_AutoPlay");
		}

		[Test]
		public void WhenPlayButtonTapped_StatusTransitionsToStopped()
		{
			NavigateToSample("AnimatedVisualPlayer");
			App.WaitForElement("AnimatedVisualPlayer_PlayBtn");
			App.WaitForElement("AnimatedVisualPlayer_Status");

			var status = new QueryEx(q => q.All().Marked("AnimatedVisualPlayer_Status"));
			Assert.AreEqual("Status: Stopped", status.GetDependencyPropertyValue<string>("Text"),
				"Initial status must be Stopped before play");

			TakeScreenshot("Before play");
			App.Tap("AnimatedVisualPlayer_PlayBtn");
			TakeScreenshot("After play tapped");

			// Wait for the one-shot animation to reach its terminal state
			App.Wait(System.TimeSpan.FromSeconds(5));
			Assert.AreEqual("Status: Stopped", status.GetDependencyPropertyValue<string>("Text"),
				"Status must return to Stopped after animation completes");
		}
	}
}
