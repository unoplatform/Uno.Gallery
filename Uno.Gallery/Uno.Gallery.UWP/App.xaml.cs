using Microsoft.Extensions.Logging;
using ShowMeTheXAML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Uno.Extensions;
using Uno.Gallery.Helpers;
using Uno.Gallery.Views.GeneralPages;
using Uno.Logging;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MUXC = Microsoft.UI.Xaml.Controls;
using MUXCP = Microsoft.UI.Xaml.Controls.Primitives;

namespace Uno.Gallery
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		private static Sample[] _samples;
		private Shell _shell;

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{

#if !WINDOWS_UWP
			Uno.UI.FeatureConfiguration.ApiInformation.NotImplementedLogLevel = LogLevel.Debug; // Raise not implemented usages as Debug messages
#endif

			ConfigureFilters(global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);
			ConfigureXamlDisplay();

			this.InitializeComponent();
			this.Suspending += OnSuspending;

#if __WASM__
			_ = Windows.UI.Xaml.Window.Current.Dispatcher?.RunIdleAsync(_ => AnalyticsService.Initialize());
#endif
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			this.Log().Debug("Launched app.");
			OnLaunchedOrActivated();
		}

		/// <summary>
		/// This method is used as the entry point when opening the app from an url.
		/// </summary>
		protected override void OnActivated(IActivatedEventArgs args)
		{
			this.Log().Debug("Activated app.");
			base.OnActivated(args);
		}

		private void OnLaunchedOrActivated()
		{
			var window = Windows.UI.Xaml.Window.Current;
			var isFirstLaunch = !(window.Content is Shell);

			if (isFirstLaunch)
			{
#if __IOS__ && USE_UITESTS
				// requires Xamarin Test Cloud Agent
				Xamarin.Calabash.Start();
#endif

				InitializeThemes();

#if WINDOWS_UWP
				ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 568)); // (size of the iPhone SE)
#endif

				if (!(window.Content is Shell))
				{
					window.Content = _shell = BuildShell();
				}
			}

			// Ensure the current window is active
			window.Activate();
		}		

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
				_ = Windows.UI.Xaml.Window.Current.Dispatcher.RunIdleAsync(_ => AnalyticsService.TrackView(sample?.Title ?? page.GetType().Name));
#endif

				_shell.NavigationView.Content = page;
			}
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
#if !WINDOWS_UWP
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
			var sample = GetSamples()
				.FirstOrDefault(x => string.Equals(x.Title, _shell.CurrentSampleBackdoor, StringComparison.OrdinalIgnoreCase));

			if (sample == null)
			{
				this.Log().LogWarning($"No SampleAttribute found with a Title that matches: {_shell.CurrentSampleBackdoor}");
				return;
			}

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
					parentItem = new MUXC.NavigationViewItem
					{
						Content = category.Key.GetDescription() ?? category.Key.ToString(),
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
						{ "Uno.Gallery", LogLevel.Debug },
#if !DEBUG
						{ "Windows.UI.Xaml", LogLevel.None },
						{ "Windows.ApplicationModel.Core.CoreApplicationViewTitleBar", LogLevel.None },
						{ "Uno.UI.DataBinding.BindingPropertyHelper", LogLevel.None },
#endif

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

		private void ConfigureXamlDisplay()
		{
			XamlDisplay.Init(GetType().Assembly);
		}

		public static IEnumerable<Sample> GetSamples() 
			=> _samples = _samples ?? Assembly.GetExecutingAssembly()
				.DefinedTypes
				.Where(x => x.Namespace?.StartsWith("Uno.Gallery") == true)
				.Select(x => new { TypeInfo = x, SamplePageAttribute = x.GetCustomAttribute<SamplePageAttribute>() })
				.Where(x => x.SamplePageAttribute != null)
				.Select(x => new Sample(x.SamplePageAttribute, x.TypeInfo.AsType()))
				.ToArray();


		private void InitializeThemes()
		{
			Material.Resources.Init(this, colorPaletteOverride: new ResourceDictionary() { Source = new Uri("ms-appx:///Views/Colors.xaml") });
			Cupertino.Resources.Init(this, null);
			this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("ms-appx:///Views/Styles/TextBlock.xaml") });
		}
	}
}
