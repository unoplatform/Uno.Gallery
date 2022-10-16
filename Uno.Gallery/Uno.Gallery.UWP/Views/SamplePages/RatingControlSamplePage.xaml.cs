using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Input;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.Toolkit, "RatingControl",
		Description = "A control to display rating points with stars.",
		DocumentationLink = "https://platform.uno/docs/articles/external/figma-docs/components/rating-control.html",
		DataType = typeof(RatingControlSamplePageViewModel))]
	public sealed partial class RatingControlSamplePage : Page
	{
		public RatingControlSamplePage()
		{
			this.InitializeComponent();
		}
	}

	public class RatingControlSamplePageViewModel : ViewModelBase
	{
		public int MaxRating { get => GetProperty<int>(); set => SetProperty(value); }
		public bool IsReadOnly { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsClearEnabled { get => GetProperty<bool>(); set => SetProperty(value); }
		public string Caption { get => GetProperty<string>(); set => SetProperty(value); }
		public double RatingControlValue { get => GetProperty<double>(); set => SetProperty(value); }
		public bool CanDrag { get => GetProperty<bool>(); set => SetProperty(value); }
		public ICommand Minus1Command => new Command(Minus1);
		public ICommand ClearCommand => new Command(Clear);
		public ICommand Plus1Command => new Command(Plus1);
		public RatingControlSamplePageViewModel()
		{
			MaxRating = 5;
			IsReadOnly = false;
			IsClearEnabled = false;
			Caption = "";
			RatingControlValue = 3;
			CanDrag = false;
		}

		public void Minus1()
		{
			RatingControlValue--;
			if (RatingControlValue < 1) RatingControlValue = IsClearEnabled ? -1 : 1;
		}
		public void Clear()
		{
			RatingControlValue = IsClearEnabled ? -1 : 1;
		}
		public void Plus1()
		{
			RatingControlValue++;
			if (RatingControlValue > MaxRating) RatingControlValue = MaxRating;
			if (RatingControlValue == 0) RatingControlValue = 1;
		}
	}
}
