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
		public static App Instance { get; private set; }

		public Window MainWindow { get; private set; }

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
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

			if (e.Arguments?.Contains("--exit") ?? false)
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
			shell.RegisterPropertyChangedCallback(Shell.CurrentSampleBackdoorProperty, OnCurrentSampleBackdoorChanged);
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

		private void OnCurrentSampleBackdoorChanged(DependencyObject sender, DependencyProperty dp)
		{
			var shell = sender as Shell ?? throw new InvalidOperationException("CurrentSampleBackdoor changed on a non-Shell object.");
			var backdoorParts = shell.CurrentSampleBackdoor.Split("-");
			var title = backdoorParts.FirstOrDefault();
			var designName = backdoorParts.Length > 1 ? backdoorParts[1] : string.Empty;

			var sample = GetSamples()
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

			ShellNavigateTo<OverviewPage>(shell);
			ShellNavigateTo(shell, sample);
		}


		private void OnNavigationItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs e)
		{
			if (e.InvokedItemContainer.DataContext is Sample sample)
			{
				var shell = VisualTreeHelperEx.FindAncestor<Shell>(sender)
					?? throw new InvalidOperationException("NavigationView is not inside a Shell.");
				ShellNavigateTo(shell, sample, trySynchronizeCurrentItem: false);
			}
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
					x.Category != SampleCategory.Canary
#endif
				)
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
