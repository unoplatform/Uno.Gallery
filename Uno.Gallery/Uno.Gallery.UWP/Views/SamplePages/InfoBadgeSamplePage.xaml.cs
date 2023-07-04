using System;
using Uno.Gallery.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "InfoBadge",
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
		public string SelectedStyle
		{
			get => GetProperty<string>();
			set
			{
				SetProperty(value);
				BadgeStyle = Application.Current.Resources["InfoBadgeStyle_" + SelectedStyle] as Style;
			}
		}
		public double Opacity { get => GetProperty<double>(); set => SetProperty(Math.Round(value, 1)); }
		public Style BadgeStyle { get => GetProperty<Style>(); set => SetProperty(value); }

		public InfoBadgeSamplePageViewModel()
		{
			AreBadgesVisible = true;
			NumericValue = 1;
			SelectedStyle = "None";
			BadgeStyle = Application.Current.Resources["InfoBadgeStyle_None"] as Style;
			Opacity = 1;
		}
	}
}
