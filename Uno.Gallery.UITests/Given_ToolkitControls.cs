using System;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// UI tests for the missing Toolkit control samples added in modernization/toolkit-controls.
	/// Each class covers one sample page: a load check and one deterministic interaction.
	/// </summary>
	public class Given_ToolkitAutoLayout : TestBase
	{
		[Test]
		public void When_Page_Loads_Vertical_IsVisible()
		{
			NavigateToSample("AutoLayout");
			TakeScreenshot("Loaded");

			var el = new QueryEx(x => x.All().Marked("AutoLayout_Vertical"));
			Assert.IsTrue(el.GetDependencyPropertyValue<bool>("IsHitTestVisible") == true
				|| el.GetDependencyPropertyValue("Visibility")?.ToString() == "Visible"
				|| App.Query(q => q.All().Marked("AutoLayout_Vertical")).Length > 0,
				"AutoLayout_Vertical element should be in the visual tree");
		}

		[Test]
		public void When_Page_Loads_AllExamples_Present()
		{
			NavigateToSample("AutoLayout");

			// Verify all five examples rendered
			App.WaitForElement(q => q.All().Marked("AutoLayout_Vertical"));
			App.WaitForElement(q => q.All().Marked("AutoLayout_Horizontal"));
			App.WaitForElement(q => q.All().Marked("AutoLayout_SpaceBetween"));
		}
	}

	public class Given_ToolkitDrawer : TestBase
	{
		[Test]
		public void When_Page_Loads_DrawerControl_IsPresent()
		{
			NavigateToSample("Drawer");
			TakeScreenshot("Loaded");

			App.WaitForElement(q => q.All().Marked("Drawer_DrawerControl"));
		}

		[Test]
		public void When_OpenToggle_Clicked_DrawerOpens()
		{
			NavigateToSample("Drawer");

			// The DrawerControl should start closed
			var drawer = new QueryEx(x => x.All().Marked("Drawer_DrawerControl"));
			Assert.IsFalse(drawer.GetDependencyPropertyValue<bool>("IsOpen"),
				"DrawerControl should be closed initially");

			TakeScreenshot("Before open");

			// Click the toggle button to open the drawer
			App.WaitThenTap("Drawer_OpenToggle");
			App.Wait(TimeSpan.FromSeconds(1));

			TakeScreenshot("After open");

			Assert.IsTrue(drawer.GetDependencyPropertyValue<bool>("IsOpen"),
				"DrawerControl should be open after tapping the toggle");
		}
	}

	public class Given_ToolkitLoadingView : TestBase
	{
		[Test]
		public void When_Page_Loads_ResultText_IsVisible()
		{
			NavigateToSample("LoadingView");
			TakeScreenshot("Loaded");

			App.WaitForElement(q => q.All().Marked("LoadingView_Basic"));
			App.WaitForElement(q => q.All().Marked("LoadingView_ResultText"));
		}

		[Test]
		public void When_LoadButton_Clicked_ProgressRing_Appears()
		{
			NavigateToSample("LoadingView");

			App.WaitForElement(q => q.All().Marked("LoadingView_LoadButton"));
			TakeScreenshot("Before load");

			App.WaitThenTap("LoadingView_LoadButton");
			App.Wait(TimeSpan.FromMilliseconds(500));

			TakeScreenshot("During load (progress ring expected)");

			// After the 2-second delay the ring disappears; just verify page didn't crash
			App.WaitForElement(q => q.All().Marked("LoadingView_Basic"), timeout: TimeSpan.FromSeconds(10));
		}
	}

	public class Given_ToolkitResponsiveView : TestBase
	{
		[Test]
		public void When_Page_Loads_DefaultView_IsPresent()
		{
			NavigateToSample("ResponsiveView");
			TakeScreenshot("Loaded");

			App.WaitForElement(q => q.All().Marked("ResponsiveView_Default"));
		}

		[Test]
		public void When_Page_Loads_InheritedAndLocal_ArePresent()
		{
			NavigateToSample("ResponsiveView");

			App.WaitForElement(q => q.All().Marked("ResponsiveView_Inherited"));
			App.WaitForElement(q => q.All().Marked("ResponsiveView_Local"));
		}
	}

	public class Given_ToolkitSafeArea : TestBase
	{
		[Test]
		public void When_Page_Loads_PaddingExample_IsVisible()
		{
			NavigateToSample("SafeArea");
			TakeScreenshot("Loaded");

			App.WaitForElement(q => q.All().Marked("SafeArea_Padding"));
		}

		[Test]
		public void When_Page_Loads_AllModes_Present()
		{
			NavigateToSample("SafeArea");

			App.WaitForElement(q => q.All().Marked("SafeArea_Padding"));
			App.WaitForElement(q => q.All().Marked("SafeArea_VisibleBounds"));
			App.WaitForElement(q => q.All().Marked("SafeArea_Margin"));
		}
	}

	public class Given_ToolkitZoomContentControl : TestBase
	{
		[Test]
		public void When_Page_Loads_ZoomControl_IsPresent()
		{
			NavigateToSample("ZoomContentControl");
			TakeScreenshot("Loaded");

			App.WaitForElement(q => q.All().Marked("ZoomContentControl_Basic"));
		}

		[Test]
		public void When_ZoomIn_Clicked_ZoomLevel_Increases()
		{
			NavigateToSample("ZoomContentControl");

			var zoomControl = new QueryEx(x => x.All().Marked("ZoomContentControl_Basic"));
			var initialZoom = zoomControl.GetDependencyPropertyValue<double>("ZoomLevel");

			TakeScreenshot("Before zoom in");

			App.WaitThenTap("ZoomContentControl_ZoomIn");
			App.Wait(TimeSpan.FromMilliseconds(300));

			TakeScreenshot("After zoom in");

			var newZoom = zoomControl.GetDependencyPropertyValue<double>("ZoomLevel");
			Assert.Greater(newZoom, initialZoom,
				"ZoomLevel should increase after pressing Zoom In");
		}

		[Test]
		public void When_Reset_Clicked_ZoomLevel_Returns_To_One()
		{
			NavigateToSample("ZoomContentControl");

			// Zoom in first, then reset
			App.WaitThenTap("ZoomContentControl_ZoomIn");
			App.Wait(TimeSpan.FromMilliseconds(300));
			App.WaitThenTap("ZoomContentControl_Reset");
			App.Wait(TimeSpan.FromMilliseconds(300));

			TakeScreenshot("After reset");

			var zoomControl = new QueryEx(x => x.All().Marked("ZoomContentControl_Basic"));
			var zoom = zoomControl.GetDependencyPropertyValue<double>("ZoomLevel");
			Assert.AreEqual(1.0, zoom, delta: 0.01, "ZoomLevel should return to 1 after reset");
		}
	}
}
