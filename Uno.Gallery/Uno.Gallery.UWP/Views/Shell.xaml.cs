using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;
using Windows.Gaming.Input;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation.Metadata;
using Windows.UI;

namespace Uno.Gallery
{
	public sealed partial class Shell : UserControl
	{
		public Shell()
		{
			this.InitializeComponent();

			InitializeSafeArea();
			this.Loaded += OnLoaded;
		}

		public NavigationView NavigationView => NavigationViewControl;

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// Initialize the toggle to the current theme.
			var root = Window.Current.Content as FrameworkElement;

			switch (root.RequestedTheme)
			{
				case ElementTheme.Default:
					DarkLightModeToggle.IsChecked = GetIsSystemDefaultDark();
					break;
				case ElementTheme.Light:
					DarkLightModeToggle.IsChecked = false;
					break;
				case ElementTheme.Dark:
					DarkLightModeToggle.IsChecked = true;
					break;
			}
		}

		/// <summary>
		/// This method handles the top padding for phones like iPhone X.
		/// </summary>
		private void InitializeSafeArea()
		{
			var full = Windows.UI.Xaml.Window.Current.Bounds;
			var bounds = ApplicationView.GetForCurrentView().VisibleBounds;

			var topPadding = Math.Abs(full.Top - bounds.Top);

			if (topPadding > 0)
			{
				TopPaddingRow.Height = new GridLength(topPadding);
			}
		}

		// Determine if system default is dark or light
		private bool GetIsSystemDefaultDark()
		{
#if WINDOWS_UWP
			var settings = new UISettings();
			var systemBackground = settings.GetColorValue(UIColorType.Background);
			var black = Color.FromArgb(255, 0, 0, 0);
			return systemBackground == black;
#else
			// TODO Test and/or find an implementation for iOS Android etc.
			return false;
#endif

		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
#if WINDOWS_UWP
			// Set theme for window root.
			if (Window.Current.Content is FrameworkElement frameworkElement)
			{
				if (frameworkElement.ActualTheme == ElementTheme.Dark)
				{
					frameworkElement.RequestedTheme = ElementTheme.Light;
				}
				else
				{
					frameworkElement.RequestedTheme = ElementTheme.Dark;
				}
			}
#endif
		}
	}
}
