using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Extensions;
using Uno.Extensions.Specialized;
using Uno.Gallery.Helpers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery;

public sealed partial class Shell : UserControl
{
	private const string NoSuggestionsFoundText = "No suggestions found";

	private static IEnumerable<Sample> SearchSamples(string query)
		=> App.GetSamples()
			.OrderByDescending(x => x.SortOrder.HasValue)
			.ThenBy(x => x.SortOrder)
			.ThenBy(x => x.Title)
			.Where(sample => query.ToLower().Split(" ").All(key => sample.Title.Contains(key, StringComparison.OrdinalIgnoreCase)));

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
		if(args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
		{
			return;
		}

		if (string.IsNullOrEmpty(sender.Text))
		{
			sender.ItemsSource = null;
			return;
		}

		var filteredSamples = SearchSamples(sender.Text);

		if (!filteredSamples.Any())
		{
			sender.ItemsSource = new List<Sample>() { new(new SamplePageAttribute(SampleCategory.None, NoSuggestionsFoundText), null) };
		}
		else
		{
			sender.ItemsSource = filteredSamples;
		}
	}

	private void SamplesSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
	{
		var sample = args.SelectedItem as Sample;

		if (sample.Title.Contains(NoSuggestionsFoundText))
		{
			return;
		}

		(Application.Current as App)?.SearchShellNavigateTo(sample);
	}

	private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		SamplesSearchBox.Focus(FocusState.Programmatic);
	}

	private void SamplesSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
	{
		if (args.ChosenSuggestion is Sample sample)
		{
			// User selected an item, take an action
			(Application.Current as App)?.SearchShellNavigateTo(sample);
		}
		else if (!string.IsNullOrEmpty(args.QueryText))
		{
			//Do a fuzzy search based on the text
			var suggestions = SearchSamples(sender.Text);
			if (Enumerable.Count(suggestions) > 0)
			{
				(Application.Current as App)?.SearchShellNavigateTo(suggestions.FirstOrDefault());
			}
		}
	}

	private async void OnAppBarButtonClick(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement { Tag: string { Length: >0 } url })
		{
			await Launcher.LaunchUriAsync(new Uri(url));
		}
	}
}
