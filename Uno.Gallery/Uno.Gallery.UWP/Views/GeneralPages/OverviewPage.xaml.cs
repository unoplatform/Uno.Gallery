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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Uno.Gallery.Controls.SamplePageLayout;

namespace Uno.Gallery.Views.GeneralPages
{
	[SamplePage(SampleCategory.None, "Overview")]
	public sealed partial class OverviewPage : Page
	{
		public Sample[] ColorSamples { get; set; }
		public int SelectedComponentIndex { get; set; }
		public KeyValuePair<SamplePageLayoutMode, ComponentSampleInfo[]> SelectedComponent { get; set; }
		public IReadOnlyDictionary<SamplePageLayoutMode, ComponentSampleInfo[]> ComponentSamples { get; set; }

		public OverviewPage()
		{
			this.InitializeComponent();

			try
			{
				LoadSampleItems();
			}
			catch (Exception e)
			{
				this.Log().Error($"Failed to load sample items", e);
			}
		}

		private void OverviewPage_Click(object sender, RoutedEventArgs e)
		{
			var context = (sender as FrameworkElement).DataContext;
			if (context is Sample sample)
			{
				(Application.Current as App)?.ShellNavigateTo(sample);
			}
			else if (context is ComponentSampleInfo component)
			{
				(Application.Current as App)?.ShellNavigateTo(component.Sample);
			}
		}

		private void LoadSampleItems()
		{
			var samples = Assembly.GetExecutingAssembly().DefinedTypes
				.Where(x => x.Namespace?.StartsWith("Uno.Gallery") == true)
				.Select(x => new { TypeInfo = x, SamplePageAttribute = x.GetCustomAttribute<SamplePageAttribute>() })
				.Where(x => x.SamplePageAttribute != null && x.SamplePageAttribute.Category != SampleCategory.None)
				.Select(x => new Sample(x.SamplePageAttribute, x.TypeInfo.AsType()))
				.OrderBy(x => x.Title)
				.ToArray();

			ColorSamples = samples.Where(x => x.Category == SampleCategory.Colors).ToArray();

			var componentResources = new ResourceDictionary() { Source = new Uri("ms-resource:///Files/Views/GeneralPages/ComponentTemplates.xaml") };
			var modes = new[] { SamplePageLayoutMode.Material, SamplePageLayoutMode.Fluent };
			ComponentSamples = samples
				.Where(x => x.Category == SampleCategory.Components)
				.SelectMany(
					// note: component samples without the [OverviewExample] tag will not be listed
					x => x.ViewType.GetCustomAttributes<OverviewExampleAttribute>(),
					(sample, example) => new ComponentSampleInfo
					{
						Sample = sample,
						Mode = example.Mode,
						ExampleTemplate = componentResources.TryGetValue(example.TemplateKey, out var resource)
							? (resource is DataTemplate || resource == null)
								? (DataTemplate)resource
								: throw new InvalidOperationException($"Expecting '{example.TemplateKey}' resource to be a {nameof(DataTemplate)}. Got {resource?.GetType().Name ?? "(null)"} instead.")
							: throw new KeyNotFoundException("Unable to find resource with key: " + example.TemplateKey)
					}
				)
				.GroupBy(x => x.Mode)
				.ToDictionary(g => g.Key, g => g.ToArray());

			SelectedComponentIndex = 0;
			SelectedComponent = ComponentSamples.ElementAtOrDefault(SelectedComponentIndex);
		}

		public class ComponentSampleInfo
		{
			public SamplePageLayoutMode Mode { get; set; }

			public Sample Sample { get; set; }

			public DataTemplate ExampleTemplate { get; set; }
		}
	}
}
