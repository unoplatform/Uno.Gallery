using System;
using NUnit.Framework;
using Uno.UITest.Helpers;
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
			var playCount = new QueryEx(q => q.All().Marked("AnimatedVisualPlayer_PlayCount"));

			Assert.AreEqual("Status: Stopped", status.GetDependencyPropertyValue<string>("Text"),
				"Initial status must be Stopped before play");
			Assert.AreEqual("Completed plays: 0", playCount.GetDependencyPropertyValue<string>("Text"),
				"Initial play count must be zero");

			TakeScreenshot("Before play");
			App.Tap("AnimatedVisualPlayer_PlayBtn");

			// PlayCount reaching 1 proves the full play→complete cycle without relying on the
			// transient Playing state, which may not be observable on all platforms (e.g. DOM).
			Assert.IsTrue(
				PollForDpValue("AnimatedVisualPlayer_PlayCount", "Text", "Completed plays: 1", TimeSpan.FromSeconds(20)),
				"Play count must become 1 after one full animation cycle");

			Assert.AreEqual("Status: Stopped", status.GetDependencyPropertyValue<string>("Text"),
				"Status must return to 'Status: Stopped' after animation completes");
			TakeScreenshot("After play completed");
		}

		private bool PollForDpValue(string automationId, string property, string expected, TimeSpan timeout)
		{
			var element = new QueryEx(q => q.All().Marked(automationId));
			var deadline = DateTime.UtcNow + timeout;
			while (DateTime.UtcNow < deadline)
			{
				if (element.GetDependencyPropertyValue<string>(property) == expected)
					return true;
				App.Wait(TimeSpan.FromMilliseconds(200));
			}
			return false;
		}
	}
}
