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

			// The handler sets Playing… synchronously before PlayAsync, so it must be
			// observable immediately after the tap (5-second Lottie animation).
			Assert.IsTrue(
				PollForDpValue("AnimatedVisualPlayer_Status", "Text", "Status: Playing\u2026", TimeSpan.FromSeconds(5)),
				"Status must become 'Status: Playing\u2026' after tapping Play");
			TakeScreenshot("During play");

			// Wait for the one-shot animation to complete.
			Assert.IsTrue(
				PollForDpValue("AnimatedVisualPlayer_Status", "Text", "Status: Stopped", TimeSpan.FromSeconds(15)),
				"Status must return to 'Status: Stopped' after animation completes");
			TakeScreenshot("After play completed");

			// PlayCount proves the full play→complete cycle, ruling out a same-state false positive.
			Assert.AreEqual("Completed plays: 1", playCount.GetDependencyPropertyValue<string>("Text"),
				"Play count must be 1 after one full animation cycle");
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
