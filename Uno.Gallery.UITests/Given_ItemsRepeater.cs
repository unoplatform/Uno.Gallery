using System;
using NUnit.Framework;
using Uno.UITest;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_ItemsRepeater : TestBase
{
	[Test]
	public void When_ItemsRepeater_Loads()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		TakeScreenshot("Loaded");
		App.WaitForElement("ItemsRepeater_Stack");
		App.WaitForElement("ItemsRepeater_Grid");
	}

	[Test]
	public void When_ItemsRepeater_Stack_CanScroll()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.WaitForElement("ItemsRepeater_Stack_ScrollDown");

		App.WaitThenTap("ItemsRepeater_Stack_ScrollDown");
		TakeScreenshot("After scroll button");

		var sv = new QueryEx(x => x.All().Marked("ItemsRepeater_Stack_ScrollViewer"));
		Assert.IsTrue(
			PollForPositiveDouble(sv, "VerticalOffset", TimeSpan.FromSeconds(5)),
			"Scrolling down must produce a positive VerticalOffset");
	}

	[Test]
	public void When_ItemsRepeater_Grid_IsVisible()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.ScrollDownTo("ItemsRepeater_Grid");
		App.WaitForElement("ItemsRepeater_Grid_The Great Gatsby");
		TakeScreenshot("Grid in view");

		var realizedTiles = App.Query(x => x.All().Marked("ItemsRepeater_Grid_The Great Gatsby"));
		Assert.AreEqual(1, realizedTiles.Length, "The first grid tile must be realized exactly once.");
		Assert.Greater(realizedTiles[0].Rect.Width, 0, "The realized grid tile must have a rendered width.");
		Assert.Greater(realizedTiles[0].Rect.Height, 0, "The realized grid tile must have a rendered height.");
	}

	[Test]
	public void When_ToolkitSelectionAdvances_SelectedIndexAndStatusAgree()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.ScrollDownTo("ItemsRepeater_Selection_Next");

		App.WaitThenTap("ItemsRepeater_Selection_Next");

		Assert.AreEqual(
			"Selected index: 1",
			new QueryEx(x => x.All().Marked("ItemsRepeater_Selection_Status")).GetDependencyPropertyValue<string>("Text"));
	}

	[Test]
	public void When_ToolkitSelectionItemIsTapped_SelectedIndexAndStatusAgree()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.ScrollDownTo("ItemsRepeater_Selection_To Kill a Mockingbird");

		App.WaitThenTap("ItemsRepeater_Selection_To Kill a Mockingbird");

		var status = new QueryEx(x => x.All().Marked("ItemsRepeater_Selection_Status"));
		Assert.IsTrue(
			PollForText(status, "Selected index: 1", TimeSpan.FromSeconds(5)),
			"Direct item selection must update the attached selected index and visible status.");
	}

	[Test]
	public void When_ToolkitSelectionItemUsesKeyboard_SelectedIndexAndStatusAgree()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.ScrollDownTo("ItemsRepeater_Selection_FocusSecond");

		App.WaitThenTap("ItemsRepeater_Selection_FocusSecond");
		App.PressEnter();

		var status = new QueryEx(x => x.All().Marked("ItemsRepeater_Selection_Status"));
		Assert.IsTrue(
			PollForText(status, "Selected index: 1", TimeSpan.FromSeconds(5)),
			"Keyboard activation must update Toolkit single selection and visible status.");
		var second = new QueryEx(x => x.All().Marked("ItemsRepeater_Selection_To Kill a Mockingbird"));
		Assert.IsTrue(second.GetDependencyPropertyValue<bool>("IsChecked"));
	}

	[Test]
	public void When_ScrolledToEnd_ToolkitLoadsAnotherOfflineBatch()
	{
		NavigateToSample("ItemsRepeater", "Fluent");
		App.ScrollDownTo("ItemsRepeater_Incremental_LoadNext");

		var status = new QueryEx(x => x.All().Marked("ItemsRepeater_Incremental_Status"));
		var before = status.GetDependencyPropertyValue<string>("Text");
		App.WaitThenTap("ItemsRepeater_Incremental_LoadNext");

		Assert.IsTrue(
			PollForChangedText(status, before, TimeSpan.FromSeconds(5)),
			"Scrolling to the end must cause SupportsIncrementalLoading to request a local batch.");
		StringAssert.Contains("batches: 1", status.GetDependencyPropertyValue<string>("Text"));
	}

	private bool PollForChangedText(QueryEx element, string before, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			if (element.GetDependencyPropertyValue<string>("Text") != before)
			{
				return true;
			}

			App.Wait(TimeSpan.FromMilliseconds(100));
		}

		return false;
	}

	private bool PollForText(QueryEx element, string expected, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			if (element.GetDependencyPropertyValue<string>("Text") == expected)
			{
				return true;
			}
			App.Wait(TimeSpan.FromMilliseconds(100));
		}
		return false;
	}

	private bool PollForPositiveDouble(QueryEx element, string property, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			if (element.GetDependencyPropertyValue<double>(property) > 0)
			{
				return true;
			}

			App.Wait(TimeSpan.FromMilliseconds(100));
		}

		return false;
	}
}
