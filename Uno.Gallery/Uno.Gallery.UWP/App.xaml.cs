using Microsoft.Extensions.Logging;
using ShowMeTheXAML;
using System;
using Uno.Gallery.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Uno.Gallery
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			ConfigureFilters(global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);
			ConfigureXamlDisplay();

			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
			var shell = GetShell();
			AddNavigationItems(shell);
			AddSettingsItem(shell);

			// Ensure the current window is active
			Windows.UI.Xaml.Window.Current.Activate();
		}

		private void AddSettingsItem(Shell shell)
		{
			var navigationView = (NavigationView)shell.FindName("NavigationViewControl");

#if WINDOWS_UWP
			navigationView.IsSettingsVisible = true;
			navigationView.Loaded += NavigationView_Loaded;

			void NavigationView_Loaded(object sender, RoutedEventArgs e)
			{
				// Change the settings text to toggle the theme
				var settingsItem = (NavigationViewItem)navigationView.SettingsItem;
				settingsItem.Content = "Toggle Light/Dark theme";
				navigationView.Loaded -= NavigationView_Loaded;
			}
#else
			navigationView.IsSettingsVisible = false;
#endif
		}

		private void AddNavigationItems(Shell shell)
		{
			var navigationView = shell.NavigationView;

			navigationView.MenuItems.Add(new NavigationViewItem()
			{
				Content = "Home",
				DataContext = NavigationItemType.Home
			});

			navigationView.MenuItems.Add(new NavigationViewItem()
			{
				Content = "Color Palette",
				DataContext = NavigationItemType.ColorPalette
			});

			navigationView.MenuItems.Add(new NavigationViewItemSeparator());

			foreach (var sample in SamplePageAttribute.GetAllSamples())
			{
				navigationView.MenuItems.Add(new NavigationViewItem()
				{
					Content = sample.Title,
					DataContext = sample
				});
			}

			navigationView.ItemInvoked += NavigationView_ItemInvoked;

			void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
			{
				if (args.IsSettingsInvoked)
				{
					ToggleTheme();
				}
				else
				{
					var selectedItem = args.InvokedItemContainer;
					if (selectedItem.DataContext is Sample sample)
					{
						var page = (Page)Activator.CreateInstance(sample.ViewType);
						page.DataContext = sample;

						sender.Content = page;
					}
					else if (selectedItem.DataContext is NavigationItemType itemType)
					{
						object page;
						switch(itemType)
						{
							case NavigationItemType.ColorPalette:
								page = new ColorPalettePage();
								break;
							case NavigationItemType.Home:
								page = new HomeSamplePage();
								break;
							default:
								throw new InvalidOperationException($"Value {itemType} is not supported for NavigationItemType.");
						}

						sender.Content = page;
					}
				}
			}

			void ToggleTheme()
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
		
		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}


		/// <summary>
		/// Configures global logging
		/// </summary>
		/// <param name="factory"></param>
		static void ConfigureFilters(ILoggerFactory factory)
		{
			factory
				.WithFilter(new FilterLoggerSettings
					{
						{ "Uno", LogLevel.Warning },
						{ "Windows", LogLevel.Warning },

						// Debug JS interop
						// { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

						// Generic Xaml events
						// { "Windows.UI.Xaml", LogLevel.Debug },
						// { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
						// { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

						// Layouter specific messages
						// { "Windows.UI.Xaml.Controls", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
						// { "Windows.Storage", LogLevel.Debug },

						// Binding related messages
						// { "Windows.UI.Xaml.Data", LogLevel.Debug },

						// DependencyObject memory references tracking
						// { "ReferenceHolder", LogLevel.Debug },

						// ListView-related messages
						// { "Windows.UI.Xaml.Controls.ListViewBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.ListView", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.GridView", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.VirtualizingPanelLayout", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.NativeListViewBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.ListViewBaseSource", LogLevel.Debug }, //iOS
						// { "Windows.UI.Xaml.Controls.ListViewBaseInternalContainer", LogLevel.Debug }, //iOS
						// { "Windows.UI.Xaml.Controls.NativeListViewBaseAdapter", LogLevel.Debug }, //Android
						// { "Windows.UI.Xaml.Controls.BufferViewCache", LogLevel.Debug }, //Android
						// { "Windows.UI.Xaml.Controls.VirtualizingPanelGenerator", LogLevel.Debug }, //WASM
					}
				)
#if DEBUG
				.AddConsole(LogLevel.Debug);
#else
				.AddConsole(LogLevel.Information);
#endif
		}

		private Shell GetShell()
		{
			if (Windows.UI.Xaml.Window.Current.Content is Shell shell)
			{
				return shell;
			}
			else
			{
				shell = new Shell();
				Windows.UI.Xaml.Window.Current.Content = shell;
				return shell;
			}
		}

		static void ConfigureXamlDisplay()
		{
			XamlDisplay.Init();
		}
	}
}
