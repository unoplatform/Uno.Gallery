//using Android.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using ShowMeTheXAML;
using System;
using System.Linq;
using System.Reflection;
using Uno.Extensions;
using Uno.Gallery.Entities;
using Uno.Gallery.Helpers;
using Uno.Gallery.RuntimeTests;
using Uno.Gallery.Views.GeneralPages;
using Uno.Gallery.Views.Samples;
using Uno.Logging;
using Uno.UI;
using Windows.ApplicationModel;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
using MUXC = Microsoft.UI.Xaml.Controls;
using MUXCP = Microsoft.UI.Xaml.Controls.Primitives;
using Window = Microsoft.UI.Xaml.Window;

namespace Uno.Gallery
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application
	{
		private readonly bool _exitAfterLaunching;

		public static App Instance { get; private set; }

		public Window MainWindow { get; private set; }

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
			: this(false)
		{
		}

		public App(bool exitAfterLaunching)
		{
			_exitAfterLaunching = exitAfterLaunching;
			Instance = this;

			ConfigureFeatureFlags();
			InitializeLogging();
			ConfigureXamlDisplay();

#if HAS_UNO
			global::Uno.UI.FeatureConfiguration.Font.DefaultTextFontFamily = "ms-appx:///Uno.Fonts.OpenSans/Fonts/OpenSans.ttf";
#endif

			this.InitializeComponent();

#if !WINDOWS
			this.Suspending += OnSuspending;
#endif

#if __WASM__
			_ = DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.Initialize());
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
			RuntimeTestModeDetector.Initialize(e.Arguments);
			OnLaunchedOrActivated();

			if (_exitAfterLaunching)
			{
				Exit();
			}
		}

		private void OnLaunchedOrActivated()
		{
			MainWindow = new Window();

#if DEBUG
			MainWindow.UseStudio();
#endif

			var isFirstLaunch = !(MainWindow.Content is Shell);

			if (isFirstLaunch)
			{
				if (RuntimeTestModeDetector.IsAutomatedRun)
				{
					// The runtime tests replace the window content once they start, so the shell isn't built
					MainWindow.Content = new RuntimeTestsSamplePage();
				}
				else
				{
					InitializeWindow(MainWindow);
				}
			}

			// Ensure the current window is active
			MainWindow.Activate();

#if __ANDROID__ || __IOS__
			MobileRuntimeTestsAutostart.StartIfRequested(MainWindow);
#endif
		}

		public void InitializeWindow(Window window)
		{
			window.Content = BuildShell();
		}

		private Shell GetWindowShell(Window window) =>
			window.Content as Shell ?? throw new InvalidOperationException("Window content is not a Shell.");

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

				var shell = App.Instance.GetWindowShell(App.Instance.MainWindow);
				(Application.Current as App)?.ShellNavigateTo(shell, sample);
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

		public void ShellNavigateTo(Shell shell, Sample sample) => ShellNavigateTo(shell, sample, trySynchronizeCurrentItem: true);

		private void ShellNavigateTo<TPage>(Shell shell, bool trySynchronizeCurrentItem = true) where TPage : Page
		{
			var pageType = typeof(TPage);
			var attribute = pageType.GetCustomAttribute<SamplePageAttribute>()
				?? throw new NotSupportedException($"{pageType} isn't tagged with [{nameof(SamplePageAttribute)}].");
			var sample = new Sample(attribute, pageType);

			ShellNavigateTo(shell, sample, trySynchronizeCurrentItem);
		}

		private void ShellNavigateTo(Shell shell, Sample sample, bool trySynchronizeCurrentItem)
		{
			var nv = shell.NavigationView;
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
				_ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.TrackView(sample?.Title ?? page.GetType().Name));
#endif

				shell.NavigationView.Content = page;
			}
		}

		public void SearchShellNavigateTo(Shell shell, Sample sample)
		{
			var nv = shell.NavigationView;
			if (nv.Content?.GetType() == sample.ViewType)
			{
				return;
			}

			MUXC.NavigationViewItem selectedItem = null;
			MUXC.NavigationViewItem selectedCategory = null;

			foreach (MUXC.NavigationViewItem category in nv.MenuItems)
			{
				selectedItem = category.MenuItems.OfType<MUXC.NavigationViewItem>()
					.FirstOrDefault(item => item.DataContext is Sample s && s.ViewType == sample.ViewType);

				if (selectedItem != null)
				{
					selectedCategory = category;
					break;
				}
			}

			if (selectedItem is null)
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
			_ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.TrackView(sample?.Title ?? page.GetType().Name));
#endif

			shell.NavigationView.Content = page;
		}

		private Shell BuildShell()
		{
			var shell = new Shell();
			AutomationProperties.SetAutomationId(shell, "AppShell");
			var nv = shell.NavigationView;
			AddNavigationItems(nv);
#if __WASM__
			if (!IsThereSampleFilteredByArgs(shell, nv))
#endif
			{
				// landing navigation
				ShellNavigateTo<OverviewPage>(
					shell
#if !WINDOWS
					// workaround for uno#5069: setting NavView.SelectedItem at launch bricks it
					, trySynchronizeCurrentItem: false
#endif
				);
			}

			// navigation + setting handler
			nv.ItemInvoked += OnNavigationItemInvoked;

			return shell;
		}
#if __WASM__
		private bool IsThereSampleFilteredByArgs(Shell shell, MUXC.NavigationView nv)
		{
			var argumentsHash = Wasm.FragmentNavigationHandler.CurrentFragment;
			if (argumentsHash.Contains("#"))
			{
				string searchTerm = (argumentsHash + string.Empty).Replace("#", string.Empty);

				foreach (MUXC.NavigationViewItem item in nv.MenuItems)
				{
					MUXC.NavigationViewItem? sampleItem = item.MenuItems
						.Cast<MUXC.NavigationViewItem>()
						.FirstOrDefault(i => i.Content?.ToString()?.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) == true);

					if (sampleItem != null)
					{
						ShellNavigateTo(
							shell,
							(Uno.Gallery.Sample)sampleItem.DataContext
							, trySynchronizeCurrentItem: false
						);
						return true;
					}
				}
				//If there is a Hash that is not valid, redirect it to the root of the site.
				Wasm.LocationHrefNavigationHandler.CurrentLocationHref = "/";
			}
			return false;
		}
#endif


		private void OnNavigationItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs e)
		{
			if (e.InvokedItemContainer.DataContext is Sample sample)
			{
				var shell = VisualTreeHelperEx.FindAncestor<Shell>(sender)
					?? throw new InvalidOperationException("NavigationView is not inside a Shell.");
				ShellNavigateTo(shell, sample, trySynchronizeCurrentItem: false);
			}
		}

		/// <summary>
		/// Gets the samples of this platform, without the runtime tests page unless it's a Debug build or the app was launched with `--rtt=1`.
		/// </summary>
		public static Sample[] GetSamples() => GetAllSamples()
			.Where(x => x.ViewType != typeof(RuntimeTestsSamplePage) || ShowRuntimeTests)
			.ToArray();

		private static bool ShowRuntimeTests =>
#if DEBUG
			true;
#else
			RuntimeTestModeDetector.IsEnabled || IsDebugMode;
#endif

		/// <summary>
		/// Gets whether the debug mode was entered, see <see cref="EnterDebugMode(Shell)"/>.
		/// </summary>
		public static bool IsDebugMode { get; private set; }

		/// <summary>
		/// Enters the debug mode, which reveals the runtime tests navigation item and the debug panel of the shell.
		/// </summary>
		/// <remarks>This is safe to call again, eg: for a shell of another window opened before entering the debug mode.</remarks>
		internal void EnterDebugMode(Shell shell)
		{
			IsDebugMode = true;

			AddRuntimeTestsNavigationItem(shell.NavigationView);
			shell.ActivateDebugPanel();
		}

		private void AddRuntimeTestsNavigationItem(MUXC.NavigationView nv)
		{
			var hasItem = nv.MenuItems
				.OfType<MUXC.NavigationViewItem>()
				.SelectMany(x => x.MenuItems.OfType<MUXC.NavigationViewItem>().Prepend(x))
				.Any(x => (x.DataContext as Sample)?.ViewType == typeof(RuntimeTestsSamplePage));
			if (hasItem)
			{
				return;
			}

			var sample = GetAllSamples().First(x => x.ViewType == typeof(RuntimeTestsSamplePage));
			var parentItem = nv.MenuItems
				.OfType<MUXC.NavigationViewItem>()
				.FirstOrDefault(x => x.Tag is SampleCategory category && category == sample.Category);
			if (parentItem is null)
			{
				// the canary category is the last one, so its section goes at the end
				parentItem = CreateCategoryNavigationItem(sample.Category, tier: 1);
				nv.MenuItems.Add(parentItem);
			}

			parentItem.MenuItems.Add(CreateSampleNavigationItem(sample, tier: 2));
		}

		private void AddNavigationItems(MUXC.NavigationView nv)
		{
			var categories = GetSamples()
				.OrderByDescending(x => x.SortOrder.HasValue)
				.ThenBy(x => x.SortOrder)
				.ThenBy(x => x.Title)
				.Where(x =>
#if AOT_PROFILE_GEN || IS_CANARY_BUILD || DEBUG
					true
#else
					// GetSamples() already decides whether the runtime tests page is listed
					x.Category != SampleCategory.Canary || x.ViewType == typeof(RuntimeTestsSamplePage)
#endif
				)
				.GroupBy(x => x.Category);

			foreach (var category in categories.OrderBy(x => x.Key))
			{
				var tier = 1;

				var parentItem = default(MUXC.NavigationViewItem);
				if (category.Key != SampleCategory.None)
				{
					parentItem = CreateCategoryNavigationItem(category.Key, tier++);

					nv.MenuItems.Add(parentItem);
				}

				foreach (var sample in category)
				{
					var item = CreateSampleNavigationItem(sample, tier);

					(parentItem?.MenuItems ?? nv.MenuItems).Add(item);
				}
			}
		}

		private MUXC.NavigationViewItem CreateCategoryNavigationItem(SampleCategory category, int tier)
		{
			var categoryInfo = category.GetAttribute<SampleCategoryInfoAttribute>();
			var item = new MUXC.NavigationViewItem
			{
				Icon = categoryInfo != null ? new FontIcon() { Glyph = categoryInfo.Glyph } : null,
				Content = categoryInfo != null ? categoryInfo.Caption : category.ToString(),
				SelectsOnInvoked = false,
				Tag = category,
				Style = (Style)Resources[$"T{tier}NavigationViewItemStyle"]
			}.Apply(NavViewItemVisualStateFix);
			AutomationProperties.SetAutomationId(item, "Section_" + item.Content);

			return item;
		}

		private MUXC.NavigationViewItem CreateSampleNavigationItem(Sample sample, int tier)
		{
			var item = new MUXC.NavigationViewItem
			{
				Content = sample.Title,
				Icon = !string.IsNullOrEmpty(sample.Glyph) ? new FontIcon() { Glyph = sample.Glyph } : null,
				DataContext = sample,
				Style = (Style)Resources[$"T{tier}NavigationViewItemStyle"]
			}.Apply(NavViewItemVisualStateFix);
			AutomationProperties.SetAutomationId(item, "Section_" + item.Content);

			return item;
		}

		private static void NavViewItemVisualStateFix(MUXC.NavigationViewItem nvi)
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

		internal async Task NavigateToAllPages()
		{
			var shell = GetWindowShell(MainWindow);
			var samples = GetSamples();

			foreach (var sample in samples)
			{
				ShellNavigateTo(shell, sample);

				var tcs = new TaskCompletionSource();

				DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, () => tcs.TrySetResult());

				await tcs.Task;

				GC.WaitForPendingFinalizers();
			}

			ShellNavigateTo<CanarySamplePage>(shell);
		}

		/// <summary>
		/// Configures global Uno Platform logging
		/// </summary>
		internal static void InitializeLogging()
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
#if WINDOWS
			XamlDisplay.Init(GetType().Assembly);
#else   // !WINDOWS
			XamlDictionary.Init();
#endif  // WINDOWS
		}

		private void ConfigureFeatureFlags()
		{
#if !WINDOWS
			FeatureConfiguration.ApiInformation.NotImplementedLogLevel = Foundation.Logging.LogLevel.Debug; // Raise not implemented usages as Debug messages
			FeatureConfiguration.ToolTip.UseToolTips = true;
#endif
		}
	}
}
