using Microsoft.Extensions.Logging;
using ShowMeTheXAML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Uno.Extensions;
using Uno.Gallery.Entities;
using Uno.Gallery.Helpers;
using Uno.Gallery.Views.GeneralPages;
using Uno.Logging;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;
using MUXCP = Microsoft.UI.Xaml.Controls.Primitives;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
using Microsoft.UI.Dispatching;

namespace Uno.Gallery
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application
	{
		public static App Instance { get; private set; }

		public Window MainWindow { get; private set; }

		private static Sample[] _samples;

		private Shell _shell;

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			Instance = this;
#if !WINDOWS
			Uno.UI.FeatureConfiguration.ApiInformation.NotImplementedLogLevel = Foundation.Logging.LogLevel.Debug; // Raise not implemented usages as Debug messages
#endif

			InitializeLogging();
			ConfigureXamlDisplay();

			this.InitializeComponent();

#if !WINDOWS
			this.Suspending += OnSuspending;
#endif

#if __WASM__
			_ = Window.Current.DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.Initialize());
#endif
		}


		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			base.OnLaunched(e);

			this.Log().Debug("Launched app.");
			OnLaunchedOrActivated();
		}

		private void OnLaunchedOrActivated()
		{
#if WINDOWS && !HAS_UNO
			MainWindow = new Window();
#else
			MainWindow = Microsoft.UI.Xaml.Window.Current;
#endif
			var isFirstLaunch = !(MainWindow.Content is Shell);

			if (isFirstLaunch)
			{
#if __IOS__ && USE_UITESTS
				// requires Xamarin Test Cloud Agent
				Xamarin.Calabash.Start();
#endif

				MainWindow.Content = _shell = BuildShell();
			}

			// Ensure the current window is active
			MainWindow.Activate();
		}

		/// <summary>
		/// This method is invoked from JavaScript within the branch.js file.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="design"></param>
		public static void TryNavigateToLaunchSample(string title, string design)
		{
			const string UndefinedValue = "undefined";

			if (!HasValue(title))
			{
				return;
			}

			var sample = GetSamples().FirstOrDefault(s => s.ViewType.Name.ToLowerInvariant() == title.ToLowerInvariant());
			if (sample != null)
			{
				if (HasValue(design) && Enum.TryParse<Design>(design, out var designType))
				{
					SamplePageLayout.SetPreferredDesign(designType);
				}

				(Application.Current as App)?.ShellNavigateTo(sample);
			}

			bool HasValue(string val) =>
				!string.IsNullOrWhiteSpace(val) && !string.Equals(UndefinedValue, val, StringComparison.OrdinalIgnoreCase);

		}

#if !WINDOWS
		/// <summary>
		/// Invoked when application execution is being suspended. Application state is saved
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
#endif

		public void ShellNavigateTo(Sample sample) => ShellNavigateTo(sample, trySynchronizeCurrentItem: true);

		private void ShellNavigateTo<TPage>(bool trySynchronizeCurrentItem = true) where TPage : Page
		{
			var pageType = typeof(TPage);
			var attribute = pageType.GetCustomAttribute<SamplePageAttribute>()
				?? throw new NotSupportedException($"{pageType} isn't tagged with [{nameof(SamplePageAttribute)}].");
			var sample = new Sample(attribute, pageType);

			ShellNavigateTo(sample, trySynchronizeCurrentItem);
		}

		private void ShellNavigateTo(Sample sample, bool trySynchronizeCurrentItem)
		{
			var nv = _shell.NavigationView;
			if (nv.Content?.GetType() != sample.ViewType)
			{
				var selected = trySynchronizeCurrentItem
					? nv.MenuItems
						.OfType<MUXC.NavigationViewItem>()
						.FirstOrDefault(x => (x.DataContext as Sample)?.ViewType == sample.ViewType)
					: default;
				if (selected != null)
				{
					nv.SelectedItem = selected;
				}

				var page = (Page)Activator.CreateInstance(sample.ViewType);
				page.DataContext = sample;

#if __WASM__
				_ = Window.Current.DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.TrackView(sample?.Title ?? page.GetType().Name));
#endif

				_shell.NavigationView.Content = page;
			}
		}

		public void SearchShellNavigateTo(Sample sample)
		{
			var nv = _shell.NavigationView;
			if(nv.Content?.GetType() == sample.ViewType)
			{
				return;
			}

			MUXC.NavigationViewItem selectedItem = null;
			MUXC.NavigationViewItem selectedCategory = null;

			foreach(MUXC.NavigationViewItem category in nv.MenuItems)
			{
				selectedItem = category.MenuItems.OfType<MUXC.NavigationViewItem>()
					.FirstOrDefault(item => item.DataContext is Sample s && s.ViewType == sample.ViewType);

				if (selectedItem != null)
				{
					selectedCategory = category;
					break;
				}
			}

			if(selectedItem is null)
			{
				nv.SelectedItem = nv.MenuItems[0];
			}
			else
			{
				selectedCategory.IsExpanded = true;
				nv.UpdateLayout();

				nv.SelectedItem = selectedItem;
			}

			var page = (Page)Activator.CreateInstance(sample.ViewType);
			page.DataContext = sample;

#if __WASM__
			_ = Window.Current.DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.TrackView(sample?.Title ?? page.GetType().Name));
#endif

			_shell.NavigationView.Content = page;
		}

		private Shell BuildShell()
		{
			_shell = new Shell();
			AutomationProperties.SetAutomationId(_shell, "AppShell");
			_shell.RegisterPropertyChangedCallback(Shell.CurrentSampleBackdoorProperty, OnCurrentSampleBackdoorChanged);
			var nv = _shell.NavigationView;
			AddNavigationItems(nv);

			// landing navigation
			ShellNavigateTo<OverviewPage>(
#if !WINDOWS
				// workaround for uno#5069: setting NavView.SelectedItem at launch bricks it
				trySynchronizeCurrentItem: false
#endif
			);

			// navigation + setting handler
			nv.ItemInvoked += OnNavigationItemInvoked;

			return _shell;
		}

		private void OnCurrentSampleBackdoorChanged(DependencyObject sender, DependencyProperty dp)
		{
			var backdoorParts = _shell.CurrentSampleBackdoor.Split("-");
			var title = backdoorParts.FirstOrDefault();
			var designName = backdoorParts.Length > 1 ? backdoorParts[1] : string.Empty;

			var sample = GetSamples()
				.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));

			if (sample == null)
			{
				this.Log().Warn($"No SampleAttribute found with a Title that matches: {_shell.CurrentSampleBackdoor}");
				return;
			}

			if (Enum.TryParse<Design>(designName, out var design))
			{
				SamplePageLayout.SetPreferredDesign(design);
			}

			ShellNavigateTo<OverviewPage>();
			ShellNavigateTo(sample);
		}


		private void OnNavigationItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs e)
		{
			if (e.InvokedItemContainer.DataContext is Sample sample)
			{
				ShellNavigateTo(sample, trySynchronizeCurrentItem: false);
			}
		}

		private void AddNavigationItems(MUXC.NavigationView nv)
		{
			var categories = GetSamples()
				.OrderByDescending(x => x.SortOrder.HasValue)
				.ThenBy(x => x.SortOrder)
				.ThenBy(x => x.Title)
				.GroupBy(x => x.Category);

			foreach (var category in categories.OrderBy(x => x.Key))
			{
				var tier = 1;

				var parentItem = default(MUXC.NavigationViewItem);
				if (category.Key != SampleCategory.None)
				{
					var categoryInfo = category.Key.GetAttribute<SampleCategoryInfoAttribute>();
					parentItem = new MUXC.NavigationViewItem
					{
						Icon = categoryInfo != null ? new FontIcon() { Glyph = categoryInfo.Glyph } : null,
						Content = categoryInfo != null ? categoryInfo.Caption : category.Key.ToString(),
						SelectsOnInvoked = false,
						Style = (Style)Resources[$"T{tier++}NavigationViewItemStyle"]
					}.Apply(NavViewItemVisualStateFix);
					AutomationProperties.SetAutomationId(parentItem, "Section_" + parentItem.Content);

					nv.MenuItems.Add(parentItem);
				}

				foreach (var sample in category)
				{
					var item = new MUXC.NavigationViewItem
					{
						Content = sample.Title,
						Icon = !string.IsNullOrEmpty(sample.Glyph) ? new FontIcon() { Glyph = sample.Glyph } : null,
						DataContext = sample,
						Style = (Style)Resources[$"T{tier}NavigationViewItemStyle"]
					}.Apply(NavViewItemVisualStateFix);
					AutomationProperties.SetAutomationId(item, "Section_" + item.Content);

					(parentItem?.MenuItems ?? nv.MenuItems).Add(item);
				}
			}

			void NavViewItemVisualStateFix(MUXC.NavigationViewItem nvi)
			{
				// gallery#107: on uwp and uno, deselecting a NVI by selecting another NVI will leave the former in the "Selected" state
				// to workaround this, we force reset the visual state when IsSelected becomes false
				nvi.RegisterPropertyChangedCallback(MUXC.NavigationViewItemBase.IsSelectedProperty, (s, e) =>
				{
					if (!nvi.IsSelected)
					{
						// depending on the DisplayMode, a NVIP may or may not be used.
						var nvip = VisualTreeHelperEx.GetFirstDescendant<MUXCP.NavigationViewItemPresenter>(nvi, x => x.Name == "NavigationViewItemPresenter");
						VisualStateManager.GoToState((Control)nvip ?? nvi, "Normal", true);
					}
				});
			}
		}

		/// <summary>
		/// Configures global Uno Platform logging
		/// </summary>
		private static void InitializeLogging()
		{
#if true // Force enable logging for debugging CI // DEBUG || __IOS__
			// Logging is disabled by default for release builds, as it incurs a significant
			// initialization cost from Microsoft.Extensions.Logging setup. If startup performance
			// is a concern for your application, keep this disabled. If you're running on web or 
			// desktop targets, you can use url or command line parameters to enable it.
			//
			// For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

			var factory = LoggerFactory.Create(builder =>
			{
#if __WASM__
				builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
				builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif WINDOWS
				builder.AddDebug();
#else
				builder.AddConsole();
#endif

				// Exclude logs below this level
				builder.SetMinimumLevel(LogLevel.Information);

				// Default filters for Uno Platform namespaces
				builder.AddFilter("Uno", LogLevel.Warning);
				builder.AddFilter("Windows", LogLevel.Warning);
				builder.AddFilter("Microsoft", LogLevel.Warning);

				// Generic Xaml events
				// builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

				// Layouter specific messages
				// builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

				// builder.AddFilter("Windows.Storage", LogLevel.Debug );

				// Binding related messages
				// builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
				// builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

				// Binder memory references tracking
				// builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

				// RemoteControl and HotReload related
				// builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

				// Debug JS interop
				// builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
			});

			global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
			global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
		}

		private void ConfigureXamlDisplay()
		{
			XamlDisplay.Init(GetType().Assembly);
		}
	}
}
