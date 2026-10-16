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
		private static readonly Dictionary<Type, Func<bool>> _sampleVisibilityRuleSet = new()
		{
			[typeof(RuntimeTestsSamplePage)] = CanShowRuntimeTests,
			[typeof(CanarySamplePage)] = CanShowCanarySamples,
			// always true if omitted
		};

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

		public void ShellNavigateTo(Shell shell, Sample sample) =>
			ShellNavigateTo(shell, sample, trySynchronizeCurrentItem: true);

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
				if (trySynchronizeCurrentItem)
				{
					if (FindSampleNVI(nv, sample.ViewType) is { } nvi)
					{
						// note: only the direct parent is expanded, expand every ancestor if deeper nesting gets added
						if (FindParentNVI(nv, nvi) is { } parentNVI)
						{
							parentNVI.IsExpanded = true;
							nv.UpdateLayout();
						}

						nv.SelectedItem = nvi;
					}
					else
					{
						nv.SelectedItem = null;
					}
				}

				var page = (Page)Activator.CreateInstance(sample.ViewType)!;
				page.DataContext = sample;

#if __WASM__
				_ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(DispatcherQueuePriority.Low, () => AnalyticsService.TrackView(sample?.Title ?? page.GetType().Name));
#endif

				shell.NavigationView.Content = page;
			}
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
				var searchTerm = (argumentsHash + string.Empty).Replace("#", string.Empty);
				var matchedNVI = FindSampleNVI(nv, sample => sample.Title.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
				if (matchedNVI is { })
				{
					ShellNavigateTo(
						shell,
						(Uno.Gallery.Sample)matchedNVI.DataContext
						, trySynchronizeCurrentItem: false
					);
					return true;
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
		/// Gets the samples of this platform that are currently visible, see <see cref="IsSampleVisible(Sample)"/>.
		/// </summary>
		public static Sample[] GetSamples() => GetAllSamples()
			.Where(IsSampleVisible)
			.ToArray();

		/// <summary>
		/// Gets whether the debug mode was entered, see <see cref="EnterDebugMode(Shell)"/>.
		/// </summary>
		public static bool IsDebugMode { get; private set; }

		/// <summary>
		/// Enters the debug mode, which reveals the navigation items gated by it (eg: runtime tests, diagnostics) and the debug panel of the shell.
		/// </summary>
		/// <remarks>This is safe to call again, eg: for a shell of another window opened before entering the debug mode.</remarks>
		internal void EnterDebugMode(Shell shell)
		{
			IsDebugMode = true;

			UpdateRuleBasedNavigationItems(shell.NavigationView);
			shell.ActivateDebugPanel();
		}

		/// <summary>
		/// Re-evaluates the visibility of every navigation item that has a rule in <see cref="_sampleVisibilityRuleSet"/>.
		/// </summary>
		private static void UpdateRuleBasedNavigationItems(MUXC.NavigationView nv)
		{
			foreach (var (pageType, rule) in _sampleVisibilityRuleSet)
			{
				UpdateNavigationItemVisibilityAndCascade(
					nv,
					FindSampleNVI(nv, pageType, respectVisibility: false),
					rule() ? Visibility.Visible : Visibility.Collapsed);
			}
		}

		/// <summary>
		/// Shows or hides the navigation item, then updates its parents: a parent is visible if it is navigable or if any of its children is visible.
		/// </summary>
		/// <remarks>If the selected item becomes hidden, the selection is cleared, but the current page is kept.</remarks>
		private static void UpdateNavigationItemVisibilityAndCascade(MUXC.NavigationView nv, MUXC.NavigationViewItem? item, Visibility visibility)
		{
			if (item is null ||
				item.Visibility == visibility)
			{
				return;
			}

			item.Visibility = visibility;

			if (visibility == Visibility.Collapsed &&
				nv.SelectedItem is MUXC.NavigationViewItem selected &&
				item.Flatten(x => x.MenuItems.OfType<MUXC.NavigationViewItem>()).Any(x => x == selected))
			{
				nv.SelectedItem = null;
			}

			if (FindParentNVI(nv, item) is { } parent)
			{
				var isParentVisible =
					parent.DataContext is Sample ||
					parent.MenuItems.OfType<MUXC.NavigationViewItem>().Any(x => x.Visibility == Visibility.Visible);
				UpdateNavigationItemVisibilityAndCascade(nv, parent, isParentVisible ? Visibility.Visible : Visibility.Collapsed);
			}
		}

		private void AddNavigationItems(MUXC.NavigationView nv)
		{
			// note: every sample is added, the hidden ones are collapsed, see IsSampleVisible
			var categories = GetAllSamples()
				.OrderByDescending(x => x.SortOrder.HasValue)
				.ThenBy(x => x.SortOrder)
				.ThenBy(x => x.Title)
				.GroupBy(x => x.Category);

			foreach (var category in categories.OrderBy(x => x.Key))
			{
				var tier = 1;

				// if the grouping has a category, create a parent item for it,
				// otherwise the samples are added directly to the root of the navigation view
				var parentItem = default(MUXC.NavigationViewItem);
				if (category.Key != SampleCategory.None)
				{
					parentItem = CreateCategoryNavigationItem(category.Key, tier++);
				}

				foreach (var sample in category)
				{
					var item = CreateSampleNavigationItem(sample, tier);
					if (!IsSampleVisible(sample))
					{
						item.Visibility = Visibility.Collapsed;
					}

					(parentItem?.MenuItems ?? nv.MenuItems).Add(item);
				}

				if (parentItem is { })
				{
					parentItem.Visibility = parentItem.MenuItems
						.OfType<MUXC.NavigationViewItem>()
						.Any(x => x.Visibility == Visibility.Visible)
							? Visibility.Visible
							: Visibility.Collapsed;

					nv.MenuItems.Add(parentItem);
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

		private static bool IsSampleVisible(Sample sample)
		{
			if (_sampleVisibilityRuleSet.TryGetValue(sample.ViewType, out var rule))
			{
				return rule();
			}

			return true;
		}

		private static bool CanShowCanarySamples()
		{
#if (AOT_PROFILE_GEN || IS_CANARY_BUILD || DEBUG)
			return true;
#else
			return App.IsDebugMode;
#endif
		}

		private static bool CanShowRuntimeTests()
		{
#if (IS_CANARY_BUILD || DEBUG)
			return true;
#else
			return App.IsDebugMode || RuntimeTestModeDetector.IsEnabled;
#endif
		}

		private static MUXC.NavigationViewItem? FindSampleNVI(NavigationView nv, Type type, bool respectVisibility = true) =>
			FindSampleNVI(nv, sample => sample.ViewType == type, respectVisibility);

		private static MUXC.NavigationViewItem? FindSampleNVI(NavigationView nv, Func<Sample, bool> predicate, bool respectVisibility = true)
		{
			return nv.MenuItems.OfType<MUXC.NavigationViewItem>()
				.Flatten(x => x.MenuItems.OfType<MUXC.NavigationViewItem>())
				.Where(x => !respectVisibility || x.Visibility == Visibility.Visible)
				.FirstOrDefault(x => (x.DataContext as Sample) is { } sample && predicate(sample));
		}

		private static MUXC.NavigationViewItem? FindParentNVI(NavigationView nv, MUXC.NavigationViewItem child)
		{
			return nv.MenuItems.OfType<MUXC.NavigationViewItem>()
				.Flatten(x => x.MenuItems.OfType<MUXC.NavigationViewItem>())
				.FirstOrDefault(x => x.MenuItems.OfType<MUXC.NavigationViewItem>().Any(y => y == child));
		}
	}
}
