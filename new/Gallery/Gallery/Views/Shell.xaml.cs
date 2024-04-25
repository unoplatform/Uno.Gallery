using System;
using System.Linq;
using Uno.Extensions;
using Uno.Extensions.Specialized;
using Uno.Gallery.Helpers;
using Uno.Logging;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MUXC = Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.System;
using SystemThemeHelper = Uno.Gallery.Helpers.SystemThemeHelper;

namespace Uno.Gallery
{
	public sealed partial class Shell : UserControl
	{
		public Shell()
		{
			this.InitializeComponent();

#if !WINDOWS
			InitializeSafeArea();
			SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) => e.Handled = BackNavigateFromNestedSample();
#endif

			this.Loaded += OnLoaded;

			NestedSampleFrame.RegisterPropertyChangedCallback(ContentControl.ContentProperty, OnNestedSampleFrameChanged);
		}

		public static Shell GetForCurrentView() => (Shell)App.Instance.MainWindow.Content;

		public MUXC.NavigationView NavigationView => NavigationViewControl;

		public string CurrentSampleBackdoor
		{
			get { return (string)GetValue(CurrentSampleBackdoorProperty); }
			set { SetValue(CurrentSampleBackdoorProperty, value); }
		}

		public static readonly DependencyProperty CurrentSampleBackdoorProperty =
			DependencyProperty.Register(nameof(CurrentSampleBackdoor), typeof(string), typeof(Shell), new PropertyMetadata(null));

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			SetDarkLightToggleInitialState();

#if (__IOS__ || __ANDROID__) && !NET6_0_OR_GREATER
			this.Log().Debug("Loaded Shell.");
			Uno.Gallery.Deeplinking.BranchService.Instance.SetIsAppReady();
#endif
		}

		private void SetDarkLightToggleInitialState()
		{
			// Initialize the toggle to the current theme.
			var root = App.Instance.MainWindow.Content as FrameworkElement;

			switch (root.ActualTheme)
			{
				case ElementTheme.Default:
					DarkLightModeToggle.IsChecked = SystemThemeHelper.GetSystemApplicationTheme() == ApplicationTheme.Dark;
					break;
				case ElementTheme.Light:
					DarkLightModeToggle.IsChecked = false;
					break;
				case ElementTheme.Dark:
					DarkLightModeToggle.IsChecked = true;
					break;
			}
		}

#if !WINDOWS
		/// <summary>
		/// This method handles the top padding for phones like iPhone X.
		/// </summary>
		private void InitializeSafeArea()
		{
			ApplicationView.GetForCurrentView().VisibleBoundsChanged += (s, e) => Adjust();

			Adjust();

			void Adjust()
			{
                var full = App.Instance.MainWindow.Bounds;
                var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
				var topPadding = Math.Abs(full.Top - bounds.Top);

				if (topPadding > 0)
				{
					TopPaddingRow.Height = new GridLength(topPadding);
				}
			}
		}
#endif

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			// Set theme for window root.
			if (App.Instance.MainWindow.Content is FrameworkElement root)
			{
				switch (root.ActualTheme)
				{
					case ElementTheme.Default:
						if (SystemThemeHelper.GetSystemApplicationTheme() == ApplicationTheme.Dark)
						{
							root.RequestedTheme = ElementTheme.Light;
						}
						else
						{
							root.RequestedTheme = ElementTheme.Dark;
						}
						break;
					case ElementTheme.Light:
						root.RequestedTheme = ElementTheme.Dark;
						break;
					case ElementTheme.Dark:
						root.RequestedTheme = ElementTheme.Light;
						break;
				}
			}
		}

		private void OnNestedSampleFrameChanged(DependencyObject sender, DependencyProperty dp)
		{
			var isInsideNestedSample = NestedSampleFrame.Content != null;

			NavViewToggleButton.Visibility = isInsideNestedSample
				? Visibility.Collapsed
				: Visibility.Visible;

			// prevent empty frame from blocking the content (nav-view) behind it
			NestedSampleFrame.Visibility = isInsideNestedSample
				? Visibility.Visible
				: Visibility.Collapsed;

#if !WINDOWS
			// toggle built-in back button for wasm (from browser)
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = isInsideNestedSample
				? AppViewBackButtonVisibility.Visible
				: AppViewBackButtonVisibility.Collapsed;
#endif
		}

		public void ShowNestedSample<TPage>(bool? clearStack = null) where TPage : Page
		{
			var wasFrameEmpty = NestedSampleFrame.Content == null;
			if (NestedSampleFrame.Navigate(typeof(TPage)) && (clearStack ?? wasFrameEmpty))
			{
				NestedSampleFrame.BackStack.Clear();
			}
		}

		public bool BackNavigateFromNestedSample()
		{
			if (NestedSampleFrame.Content == null)
			{
				return false;
			}

			if (NestedSampleFrame.CanGoBack)
			{
				NestedSampleFrame.GoBack();
			}
			else
			{
				NestedSampleFrame.Content = null;

#if __IOS__
				// This will force reset the UINavigationController, to prevent the back button from appearing when the stack is supposedly empty.
				// note: Merely setting the Frame.Content to null, doesn't fully reset the stack.
				// When revisiting the page1 again, the previous page1 is still in the UINavigationController stack
				// causing a back button to appear that takes us back to the previous page1
				NestedSampleFrame.BackStack.Add(default);
				NestedSampleFrame.BackStack.Clear();
#endif
			}

			return true;
		}

		private void NavViewToggleButton_Click(object sender, RoutedEventArgs e)
		{
			if (NavigationViewControl.PaneDisplayMode == MUXC.NavigationViewPaneDisplayMode.LeftMinimal)
			{
				NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
			}
			else if (NavigationViewControl.PaneDisplayMode == MUXC.NavigationViewPaneDisplayMode.Left)
			{
				NavigationViewControl.IsPaneVisible = !NavigationViewControl.IsPaneVisible;
				NavigationViewControl.IsPaneOpen = NavigationViewControl.IsPaneVisible;
			}
		}

		private void NavigationViewControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// This could be done using VisualState with Adaptive triggers, but an issue prevents this currently - https://github.com/unoplatform/uno/issues/5168
			var desktopWidth = (double)Application.Current.Resources["DesktopAdaptiveThresholdWidth"];
			if (e.NewSize.Width >= desktopWidth && NavigationViewControl.PaneDisplayMode != MUXC.NavigationViewPaneDisplayMode.Left)
			{
				NavigationViewControl.PaneDisplayMode = MUXC.NavigationViewPaneDisplayMode.Left;
				NavigationViewControl.IsPaneOpen = true;
			}
			else if (e.NewSize.Width < desktopWidth && NavigationViewControl.PaneDisplayMode != MUXC.NavigationViewPaneDisplayMode.LeftMinimal)
			{
				NavigationViewControl.IsPaneVisible = true;
				NavigationViewControl.PaneDisplayMode = MUXC.NavigationViewPaneDisplayMode.LeftMinimal;
			}
		}

		private void SamplesSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			//This check can be removed when https://github.com/unoplatform/uno/issues/11635 is fixed
#if !__ANDROID__ && !__IOS__
			if(args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
			{
				return;
			}
#endif

			if (string.IsNullOrEmpty(sender.Text))
			{
				sender.ItemsSource = null;
				return;
			}

			var splitText = sender.Text.ToLower().ToLower().Split(" ");

			var samples = App.GetSamples()
				.OrderByDescending(x => x.SortOrder.HasValue)
				.ThenBy(x => x.SortOrder)
				.ThenBy(x => x.Title)
				.Where(cat => splitText.All(key => cat.Title.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0));

			sender.ItemsSource = samples;
		}

		private void SamplesSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			(Application.Current as App)?.SearchShellNavigateTo(args.SelectedItem as Sample);
		}

		private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			SamplesSearchBox.Focus(FocusState.Programmatic);
		}

		private async void OnAppBarButtonClick(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement { Tag: string { Length: >0 } url })
			{
				await Launcher.LaunchUriAsync(new Uri(url));
			}
		}
	}
}
