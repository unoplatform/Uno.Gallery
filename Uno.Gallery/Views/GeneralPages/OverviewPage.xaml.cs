using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Extensions;
using Uno.Logging;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using static Uno.Gallery.SamplePageLayout;

namespace Uno.Gallery.Views.GeneralPages
{
	[SamplePage(SampleCategory.None, "Overview", glyph: "\uE10F")]
	public sealed partial class OverviewPage : Page
	{
		private const int DebugModeClicksRequired = 7;
		private const int DebugModeCountdownClicks = 3;

		private string _versionText;
		private int _versionClickCount;

		public OverviewPage()
		{
			this.InitializeComponent();

			Loaded += OverviewPage_Loaded;
		}

		private void OverviewPage_Loaded(object sender, RoutedEventArgs e)
		{
			if (FindName("VersionLabel") is TextBlock tb)
			{
				tb.Text = _versionText = GetAppVersion();
			}
		}

		private void VersionLabel_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (sender is not TextBlock tb)
			{
				return;
			}

			var remaining = DebugModeClicksRequired - ++_versionClickCount;
			if (App.IsDebugMode || remaining <= 0)
			{
				// note: always re-enter, so the shell of a window opened before entering the debug mode also gets updated
				App.Instance.EnterDebugMode(Shell.GetForElement(this));

				tb.Text = $"{_versionText} (already in debug mode)";
			}
			else if (remaining <= DebugModeCountdownClicks)
			{
				tb.Text = $"{_versionText} ({remaining} more {(remaining == 1 ? "click" : "clicks")} to enter debug mode)";
			}
		}

		private string GetAppVersion()
		{
			var application = Application.Current;
			var assembly = application.GetType().GetTypeInfo().Assembly;

			if (assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() is AssemblyInformationalVersionAttribute aiva)
			{
				return $"Uno Gallery Version {aiva.InformationalVersion}";
			}
			else
			{
				var version = Package.Current.Id.Version;
				return $"Uno Gallery Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
			}
		}
	}
}
