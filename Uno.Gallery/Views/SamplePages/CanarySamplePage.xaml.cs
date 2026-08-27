using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples
{
	public sealed class AssemblyInfoItem
	{
		public string Name { get; }
		public string Version { get; }
		public string DisplayVersion { get; }

		public AssemblyInfoItem(string name, string version, string displayVersion)
		{
			Name = name;
			Version = version;
			DisplayVersion = displayVersion;
		}
	}

	[SamplePage(SampleCategory.Canary, "Diagnostics",
		Description = "Offline build, renderer, platform, execution-mode, feature-availability, and performance diagnostics.",
		DocumentationLink = "https://platform.uno/docs/articles/uno-development/uno-internals.html",
		Slug = "diagnostics",
		Tags = new[] { "diagnostics", "renderer", "platform", "aot", "offline" },
		Status = SampleStatus.Experimental,
		Owner = "unoplatform/maintainers",
		ReviewedOn = "2026-08-27")]
	public sealed partial class CanarySamplePage : Page
	{
		private bool _assembliesLoaded;
		private TextBox? _perfMarksTextBox;

		public CanarySamplePage()
		{
			this.InitializeComponent();
		}

		private void NavigateAllPages_Click(object sender, RoutedEventArgs e)
		{
			_ = App.Instance.NavigateToAllPages();
		}

		private void BuildDiagnostics_Loaded(object sender, RoutedEventArgs e)
		{
			((TextBlock)sender).Text =
				$"Renderer: {BuildInfo.Renderer}{Environment.NewLine}" +
				$"Backend: {BuildInfo.Backend}{Environment.NewLine}" +
				$"Platform: {BuildInfo.Platform}{Environment.NewLine}" +
				$"Target framework: {BuildInfo.TargetFramework}{Environment.NewLine}" +
				$"Build: {BuildInfo.Configuration}; execution: {BuildInfo.ExecutionMode}{Environment.NewLine}" +
				$"Features: {BuildInfo.FeatureAvailability}";
		}

		private void PerfMarksTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is not TextBox tb) return;
			_perfMarksTextBox = tb;
			tb.Text = PerformanceMarks.ExportJson();
		}

		private void RefreshMarks_Click(object sender, RoutedEventArgs e)
		{
			if (_perfMarksTextBox is { } tb)
				tb.Text = PerformanceMarks.ExportJson();
		}

		private void AssembliesList_Loaded(object sender, RoutedEventArgs e)
		{
			if (_assembliesLoaded || sender is not ItemsControl control)
				return;

			_assembliesLoaded = true;
			control.ItemsSource = AssemblyLoadContext.Default.Assemblies
				.Select(GetAssemblyVersionString)
				.OrderBy(t => t.name)
				.Select(t => new AssemblyInfoItem(t.name, t.version, t.displayVersion))
				.ToArray();
		}

		private (string name, string version, string displayVersion) GetAssemblyVersionString(Assembly assembly)
		{
			if (assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() is AssemblyInformationalVersionAttribute aiva)
			{
				return (assembly.GetName().Name, assembly.GetName().Version?.ToString(), aiva.InformationalVersion);
			}
			else
			{
				return ("0.0.0.0", "0.0.0.0", "0.0.0.0");
			}
		}
	}
}
