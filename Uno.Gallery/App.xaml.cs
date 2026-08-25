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
#if __IOS__ && USE_UITESTS && !__MACCATALYST__ && !DEBUG
				// requires Xamarin Test Cloud Agent
				Xamarin.Calabash.Start();
#endif

				InitializeWindow(MainWindow);
			}

			// Ensure the current window is active
			MainWindow.Activate();
		}

		public void InitializeWindow(Window window)
		{
			window.Content = BuildShell();
		}

		private Shell GetWindowShell(Window window) =>
			window.Content as Shell ?? throw new InvalidOperationException("Window content is not a Shell.");

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

		public void ShellNavigateTo(Shell shell, Sample sample) => shell.Navigator?.NavigateTo(sample);

		private void ShellNavigateTo<TPage>(Shell shell, bool trySynchronizeCurrentItem = true) where TPage : Page, new()
		{
			var pageType = typeof(TPage);
			var sample = shell.Samples.FirstOrDefault(s => s.ViewType == pageType)
				?? CreateLegacySample<TPage>();
			shell.Navigator!.NavigateTo(sample,
				trySynchronizeCurrentItem ? NavigationOptions.None : NavigationOptions.SkipNavSync);
		}

		private static Sample CreateLegacySample<TPage>() where TPage : Page, new()
		{
			var pageType = typeof(TPage);
			var attribute = pageType.GetCustomAttribute<SamplePageAttribute>()
				?? throw new NotSupportedException($"{pageType} isn't tagged with [{nameof(SamplePageAttribute)}].");
			return new Sample(attribute, pageType, () => new TPage(), null);
		}

		public void SearchShellNavigateTo(Shell shell, Sample sample)
			=> shell.Navigator?.NavigateTo(sample, NavigationOptions.ExpandCategory);

		private Shell BuildShell()
		{
			var sortedSamples = GetSamples()
				.OrderByDescending(x => x.SortOrder.HasValue)
				.ThenBy(x => x.SortOrder)
				.ThenBy(x => x.Title)
#if !AOT_PROFILE_GEN && !IS_CANARY_BUILD && !DEBUG && !USE_UITESTS
					.Where(x => x.Category != SampleCategory.Canary)
#endif
					.ToArray();

			var shell = new Shell { Samples = sortedSamples };
			AutomationProperties.SetAutomationId(shell, "AppShell");
			var nv = shell.NavigationView;
			AddNavigationItems(nv, sortedSamples);

			// Navigator must be assigned before the backdoor callback fires and before initial navigation.
			var navigator = new ShellNavigator(shell);
			shell.Navigator = navigator;

			shell.RegisterPropertyChangedCallback(Shell.CurrentSampleBackdoorProperty, OnCurrentSampleBackdoorChanged);
#if __WASM__
			if (!IsThereSampleFilteredByArgs(shell))
			{
				navigator.NavigateToOverview(NavigationOptions.SkipNavSync | NavigationOptions.SkipHistory);
				Wasm.BrowserHistoryHandler.ReplaceState("overview", SamplePageLayout.CurrentDesign.ToString());
			}
			// Subscribe after initial navigation so back/forward callbacks don't fire during startup.
			var uiQueue = DispatcherQueue.GetForCurrentThread()
				?? throw new InvalidOperationException("BuildShell must run on the UI thread.");
			Wasm.BrowserHistoryHandler.Subscribe(state => OnBrowserNavigated(uiQueue, navigator, state));
#else
			navigator.NavigateToOverview(
#if !WINDOWS
				// workaround for uno#5069: setting NavView.SelectedItem at launch bricks it
				NavigationOptions.SkipNavSync
#endif
			);
#endif

			// navigation + setting handler
			nv.ItemInvoked += OnNavigationItemInvoked;

			return shell;
		}
#if __WASM__
		/// <summary>
		/// Reads the initial URL state (hash + design query), resolves the target sample via
		/// navigator lookup, navigates with <see cref="NavigationOptions.SkipHistory"/>, then
		/// canonicalizes the URL with <c>replaceState</c>.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> when a non-overview sample was navigated to (caller must not
		/// issue a separate NavigateToOverview call); <see langword="false"/> when the URL is
		/// empty, is the canonical overview hash, or contains an unknown fragment (caller
		/// should navigate to overview and canonicalize).
		/// </returns>
		private bool IsThereSampleFilteredByArgs(Shell shell)
		{
			var rawHash = Wasm.BrowserHistoryHandler.GetHash();
			var designStr = Wasm.BrowserHistoryHandler.GetDesign();

			// Apply design preference before the first page is created so SamplePageLayout picks it up.
			if (Enum.TryParse<Design>(designStr, ignoreCase: true, out var parsedDesign))
				SamplePageLayout.SetPreferredDesign(parsedDesign);

			if (string.IsNullOrWhiteSpace(rawHash) || rawHash == "overview")
				return false;

			var decoded = Uri.UnescapeDataString(rawHash);

			// 1. Exact slug match — canonical new-format (#button) and case variations (#Button).
			var sample = shell.Navigator!.FindBySlug(rawHash);

			// 2. Decoded slug match — handles percent-encoded slugs (rare but safe).
			if (sample is null && decoded != rawHash)
				sample = shell.Navigator!.FindBySlug(decoded);

			// 3. Exact title match — legacy share links use the sample title verbatim (#Text%20Box).
			sample ??= shell.Navigator!.FindByTitle(decoded);

			// 4. Case-insensitive Contains fallback — partial/legacy fragments (#Tex → "Text Box").
			sample ??= shell.Samples.FirstOrDefault(s =>
				s.Title.Contains(decoded, StringComparison.InvariantCultureIgnoreCase));

			if (sample is not null)
			{
				shell.Navigator!.NavigateTo(sample, NavigationOptions.SkipNavSync | NavigationOptions.SkipHistory);
				// Canonicalize: replace legacy title-fragment with slug, add missing design query.
				Wasm.BrowserHistoryHandler.ReplaceState(sample.Slug, SamplePageLayout.CurrentDesign.ToString());
				return true;
			}

			// Unknown fragment: let caller navigate to overview and canonicalize.
			return false;
		}

		/// <summary>
		/// Handles popstate/hashchange callbacks dispatched from <c>BrowserHistory.ts</c>.
		/// Invoked on the UI thread by the browser event loop; <paramref name="uiQueue"/> was
		/// captured on the UI thread at subscription time.
		/// </summary>
		private static void OnBrowserNavigated(DispatcherQueue uiQueue, ShellNavigator navigator, string state)
		{
			var nl = state.IndexOf('\n');
			var slug = nl >= 0 ? state.Substring(0, nl) : state;
			var designStr = nl >= 0 ? state.Substring(nl + 1) : string.Empty;

			uiQueue.TryEnqueue(() =>
			{
				if (Enum.TryParse<Design>(designStr, ignoreCase: true, out var design))
					SamplePageLayout.SetPreferredDesign(design);

				if (string.IsNullOrWhiteSpace(slug) || slug == "overview")
				{
					navigator.NavigateToOverview(NavigationOptions.SkipNavSync | NavigationOptions.SkipHistory);
				}
				else if (!navigator.NavigateToSlug(slug, NavigationOptions.SkipNavSync | NavigationOptions.SkipHistory))
				{
					// Unknown slug from browser history — canonicalize to overview.
					navigator.NavigateToOverview(NavigationOptions.SkipNavSync | NavigationOptions.SkipHistory);
					Wasm.BrowserHistoryHandler.ReplaceState("overview", SamplePageLayout.CurrentDesign.ToString());
				}
			});
		}
#endif

		private void OnCurrentSampleBackdoorChanged(DependencyObject sender, DependencyProperty dp)
		{
			var shell = sender as Shell ?? throw new InvalidOperationException("CurrentSampleBackdoor changed on a non-Shell object.");
			var backdoorParts = shell.CurrentSampleBackdoor.Split("-");
			var title = backdoorParts.FirstOrDefault();
			var designName = backdoorParts.Length > 1 ? backdoorParts[1] : string.Empty;

			var sample = shell.Samples
				.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));

			if (sample == null)
			{
				this.Log().Warn($"No SampleAttribute found with a Title that matches: {shell.CurrentSampleBackdoor}");
				return;
			}

			if (Enum.TryParse<Design>(designName, out var design))
			{
				SamplePageLayout.SetPreferredDesign(design);
			}

			shell.Navigator!.NavigateToOverview();
			shell.Navigator!.NavigateTo(sample);
		}


		private void OnNavigationItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs e)
		{
			if (e.InvokedItemContainer.DataContext is Sample sample)
			{
				var shell = VisualTreeHelperEx.FindAncestor<Shell>(sender)
					?? throw new InvalidOperationException("NavigationView is not inside a Shell.");
				shell.Navigator!.NavigateTo(sample, NavigationOptions.SkipNavSync);
			}
		}

		private void AddNavigationItems(MUXC.NavigationView nv, IReadOnlyList<Sample> samples)
		{
			var categories = samples
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

		internal async Task NavigateToAllPages()
		{
			var shell = GetWindowShell(MainWindow);
			var samples = shell.Samples;

			foreach (var sample in samples)
			{
				shell.Navigator!.NavigateTo(sample);

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
#if DEBUG || IS_CANARY_BUILD || USE_UITESTS
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
