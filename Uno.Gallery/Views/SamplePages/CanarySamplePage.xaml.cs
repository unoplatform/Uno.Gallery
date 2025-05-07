using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.Loader;
using System.Reflection;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Canary, "Diagnostics")]
	public sealed partial class CanarySamplePage : Page
	{
		public CanarySamplePage()
		{
			this.InitializeComponent();

			assembliesList.ItemsSource =
				AssemblyLoadContext.Default
				.Assemblies
				.Select(a =>
				{
					var asmVersion = GetAssemblyVersionString(a);
					return new {
						Name = asmVersion.name,
						Version = asmVersion.version,
						DisplayVersion = asmVersion.displayVersion
					};
				})
				.OrderBy(n => n.Name)
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

		private void OnNavigateAllPages()
		{
			_ = App.Instance.NavigateToAllPages();
		}
	}
}
