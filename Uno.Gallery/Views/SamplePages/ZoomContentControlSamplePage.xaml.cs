// Adapted from unoplatform/uno.toolkit.ui samples/Uno.Toolkit.Samples/Content/Controls/ZoomContentControlSamplePage.xaml.cs (MIT)
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;
using Uno.Toolkit.UI;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "ZoomContentControl",
		SourceSdk.UnoToolkit,
		Description = "A content control that enables pinch-to-zoom and pan gestures on all platforms.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/ZoomContentControl.html",
		Tags = new[] { "zoom", "pan", "gesture", "interaction" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		SortOrder = 60)]
	public sealed partial class ZoomContentControlSamplePage : Page
	{
		private ZoomContentControl? _zoomControl;
		private Button? _zoomInButton;
		private Button? _zoomOutButton;
		private Button? _resetButton;

		public ZoomContentControlSamplePage()
		{
			this.InitializeComponent();
			this.Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// SamplePageLayout is the x:Name given to the control in XAML; GetSampleChild
			// traverses the AgnosticContentPresenter to find named elements inside the DataTemplate.
			_zoomControl = SamplePageLayout.GetSampleChild<ZoomContentControl>(Design.Agnostic, "ZoomContent");
			_zoomInButton = SamplePageLayout.GetSampleChild<Button>(Design.Agnostic, "ZoomInButton");
			_zoomOutButton = SamplePageLayout.GetSampleChild<Button>(Design.Agnostic, "ZoomOutButton");
			_resetButton = SamplePageLayout.GetSampleChild<Button>(Design.Agnostic, "ResetButton");

			if (_zoomInButton is not null) _zoomInButton.Click += OnZoomIn;
			if (_zoomOutButton is not null) _zoomOutButton.Click += OnZoomOut;
			if (_resetButton is not null) _resetButton.Click += OnReset;
		}

		private void OnZoomIn(object sender, RoutedEventArgs e)
		{
			if (_zoomControl is null) return;
			_zoomControl.ZoomLevel = Math.Min(_zoomControl.MaxZoomLevel, _zoomControl.ZoomLevel + 0.25);
		}

		private void OnZoomOut(object sender, RoutedEventArgs e)
		{
			if (_zoomControl is null) return;
			_zoomControl.ZoomLevel = Math.Max(_zoomControl.MinZoomLevel, _zoomControl.ZoomLevel - 0.25);
		}

		private void OnReset(object sender, RoutedEventArgs e)
		{
			_zoomControl?.ResetViewport();
		}
	}
}
