using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Gallery.Entities.Data;
using Uno.Toolkit.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Uno.Gallery.ViewModels;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "ShadowContainer",
		SourceSdk.UnoToolkit,
		Description = "The ShadowContainer provides the possibility to add many-colored shadows to its content.",
		DocumentationLink = "https://github.com/unoplatform/uno.toolkit.ui/blob/main/doc/controls/ShadowContainer.md",
		DataType = typeof(ShadowContainerViewModel))]
	public sealed partial class ShadowContainerSamplePage : Page
	{
		public List<string> CbbItems = new List<string> {
				"Item 1",
				"Item 2",
				"Item 3"
			};
		private ShadowCollection _shadows;

		public ShadowContainerSamplePage()
		{
			this.InitializeComponent();

			this.Loaded += (s, e) =>
			{
				_shadows = ShadowContainer.Shadows;
				ShadowsItemsRepeater.ItemsSource = _shadows;
				_shadows.CollectionChanged += (s, e) => OnShadowsChanged();

				OnShadowsChanged();
			};
		}

		private void OnShadowsChanged()
		{
			RemoveShadowButton.IsEnabled = _shadows?.Count > 1;
		}

		private void AddShadow(object sender, RoutedEventArgs e)
		{
			var defaultShadow = (Toolkit.UI.Shadow)Resources["DefaultShadow"];

			_shadows.Add(defaultShadow.Clone());
		}

		private void RemoveShadow(object sender, RoutedEventArgs e)
		{
			if (_shadows.Count == 0)
			{
				return;
			}

			_shadows.RemoveAt(_shadows.Count - 1);
		}

		[Microsoft.UI.Xaml.Data.Bindable]
		public class ShadowContainerViewModel : ViewModelBase
		{
			public ObservableCollection<string> CbbItems { get; } = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};
		}

		
	}
}
