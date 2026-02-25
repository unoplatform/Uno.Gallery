using System;
using System.ComponentModel;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Windows.Networking.Connectivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Network Information", Description = "Provides access to network connection information for the local machine.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.networking.connectivity.networkinformation")]
	public sealed partial class NetworkInformationSamplePage : Page
	{
		public NetworkInformationSamplePage()
		{
			this.InitializeComponent();
		}

		private void CheckConnectivity_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is NetworkInformationSamplePageViewModel viewModel)
			{
				viewModel.UpdateConnectivity();
			}
		}

		private void ObserveChangesButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is NetworkInformationSamplePageViewModel viewModel)
			{
				if (!viewModel.ObservingChanges)
				{
					viewModel.StartObservingChanges();
				}
				else
				{
					viewModel.StopObservingChanges();
				}
			}
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public class NetworkInformationSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing connectivity changes";
		private const string _stopObservingContent = "Stop observing connectivity changes";

		public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }

		public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }

		public bool ObservingChanges { get => GetProperty<bool>(); set => SetProperty(value); }

		public string CurrentConnectivity { get => GetProperty<string>(); set => SetProperty(value); }

		public NetworkInformationSamplePageViewModel()
		{
			CurrentConnectivity = "Unknown";
			ButtonContent = _startObservingContent;
		}

		public void StartObservingChanges()
		{
			NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
			ButtonContent = _stopObservingContent;
			ObservingChanges = true;
		}

		public void StopObservingChanges()
		{
			NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
			ButtonContent = _startObservingContent;
			ObservingChanges = false;
		}
		private void NetworkInformation_NetworkStatusChanged(object sender)
		{
			UpdateConnectivity();
		}

		public void UpdateConnectivity()
		{
			var profile = NetworkInformation.GetInternetConnectionProfile();
			var level = profile.GetNetworkConnectivityLevel();
			switch (level)
			{
				case NetworkConnectivityLevel.ConstrainedInternetAccess:
					CurrentConnectivity = "Constrained Internet Access";
					break;
				case NetworkConnectivityLevel.InternetAccess:
					CurrentConnectivity = "Internet Access";
					break;
				case NetworkConnectivityLevel.LocalAccess:
					CurrentConnectivity = "Local Access";
					break;
				case NetworkConnectivityLevel.None:
					CurrentConnectivity = "None";
					break;
			}

			LastReadTimestamp = DateTime.Now;
		}
	}
}
