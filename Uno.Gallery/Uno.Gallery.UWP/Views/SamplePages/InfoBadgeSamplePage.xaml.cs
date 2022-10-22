using System;
using System.Collections.Generic;
using System.ComponentModel;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Toolkit, "InfoBadge",
		Description = "Represents a control for indicating notifications, alerts, new content, or to attract focus to an area within an app.",
		DocumentationLink = "https://learn.microsoft.com/es-es/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.infobadge",
		DataType = typeof(InfoBadgeSamplePageViewModel))]
	public sealed partial class InfoBadgeSamplePage : Page
	{
		public InfoBadgeSamplePage()
		{
			this.InitializeComponent();
		}
	}

	public class InfoBadgeSamplePageViewModel : ViewModelBase
	{
		public bool AreBadgesVisible { get => GetProperty<bool>(); set => SetProperty(value); }
		public int NumericValue { get => GetProperty<int>(); set => SetProperty(value); }
		public string SelectedStyle { get => GetProperty<string>(); set
			{
				SetProperty(value);
				BadgeStyle = Application.Current.Resources["InfoBadgeStyle_" + SelectedStyle] as Style;
			}
		}
		public double Opacity { get => GetProperty<double>(); set => SetProperty(value); }
		public Style BadgeStyle { get => GetProperty<Style>(); set => SetProperty(value); }
		public NavigationViewPaneDisplayMode PaneDisplayMode { get => GetProperty<NavigationViewPaneDisplayMode>(); set => SetProperty(value); }
		public List<NavigationViewPaneDisplayMode> AvailablePaneDisplayModes = new List<NavigationViewPaneDisplayMode>() {
			NavigationViewPaneDisplayMode.Auto,
			NavigationViewPaneDisplayMode.Left,
			NavigationViewPaneDisplayMode.LeftCompact,
			NavigationViewPaneDisplayMode.LeftMinimal,
			NavigationViewPaneDisplayMode.Top
		};

		public InfoBadgeSamplePageViewModel()
		{
			PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
			AreBadgesVisible = true;
			NumericValue = 1;
			BadgeStyle = Application.Current.Resources["InfoBadgeStyle_None"] as Style;
			Opacity = 1;
		}
	}
}
