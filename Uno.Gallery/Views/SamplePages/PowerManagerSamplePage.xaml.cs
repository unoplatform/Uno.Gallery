using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.System.Power;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Power Manager", Description = "Provides access to information about a device's battery and power supply status.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.system.power.powermanager")]
	[SampleConditional(SampleConditionals.Mobile | SampleConditionals.Windows, Reason = "PowerManager is currently only implemented on Android and iOS")]
	public sealed partial class PowerManagerSamplePage : Page
	{
		public PowerManagerSamplePage()
		{
			this.InitializeComponent();
		}

		private void MonitorButton_Click(object sender, RoutedEventArgs args)
		{
			if ((sender as Button)?.DataContext is PowerManagerSamplePageViewModel viewModel)
			{
				if (!viewModel.MonitoringPowerStatus)
				{
					viewModel.StartMonitorPowerStatus();
				}
				else
				{
					viewModel.StopMonitorPowerStatus();
				}
			}
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public class PowerManagerSamplePageViewModel : INotifyPropertyChanged
	{
		private bool _monitoringPowerStatus;

		private string _batteryStatusText;
		private string _energySaverStatusText;
		private string _powerSupplyStatusText;
		private string _remainingChargePercentStatusText;
		private string _remainingDischargeTimeStatusText;

		public event PropertyChangedEventHandler PropertyChanged;

		public string BatteryStatusText
		{
			get => _batteryStatusText;
			set
			{
				_batteryStatusText = value;
				OnPropertyChanged();
			}
		}

		public string EnergySaverStatusText
		{
			get => _energySaverStatusText;
			set
			{
				_energySaverStatusText = value;
				OnPropertyChanged();
			}
		}

		public string PowerSupplyStatusText
		{
			get => _powerSupplyStatusText;
			set
			{
				_powerSupplyStatusText = value;
				OnPropertyChanged();
			}
		}

		public string RemainingChargePercentStatusText
		{
			get => _remainingChargePercentStatusText;
			set
			{
				_remainingChargePercentStatusText = value;
				OnPropertyChanged();
			}
		}

		public string RemainingDischargeTimeStatusText
		{
			get => _remainingDischargeTimeStatusText;
			set
			{
				_remainingDischargeTimeStatusText = value;
				OnPropertyChanged();
			}
		}

		public string MonitorButtonLabel => $"{(_monitoringPowerStatus ? "Stop" : "Start")} monitoring battery status";

		public bool MonitoringPowerStatus
		{
			get => _monitoringPowerStatus;
			set
			{
				_monitoringPowerStatus = value;
				OnPropertyChanged(nameof(MonitorButtonLabel));
				OnPropertyChanged(nameof(MonitoringPowerStatus));
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propName = "")
		{
			_ = App.Instance.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
			});
		}

		public PowerManagerSamplePageViewModel()
		{
			BatteryStatusText = PowerManager.BatteryStatus.ToString();
			EnergySaverStatusText = PowerManager.EnergySaverStatus.ToString();
			PowerSupplyStatusText = PowerManager.PowerSupplyStatus.ToString();
			RemainingChargePercentStatusText = $"{PowerManager.RemainingChargePercent}%";
#if WINDOWS
			// Not implemented in Uno
			RemainingDischargeTimeStatusText = PowerManager.RemainingDischargeTime.ToString();
#endif
		}

		public void StartMonitorPowerStatus()
		{
			MonitoringPowerStatus = true;
			PowerManager.BatteryStatusChanged += PowerManager_BatteryStatusChanged;
			PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
			PowerManager.PowerSupplyStatusChanged += PowerManager_PowerSupplyStatusChanged;
			PowerManager.RemainingChargePercentChanged += PowerManager_RemainingChargePercentChanged;
#if WINDOWS
			PowerManager.RemainingDischargeTimeChanged += PowerManager_RemainingDischargeTimeChanged;
#endif
		}

		public void StopMonitorPowerStatus()
		{
			MonitoringPowerStatus = false;
			PowerManager.BatteryStatusChanged -= PowerManager_BatteryStatusChanged;
			PowerManager.EnergySaverStatusChanged -= PowerManager_EnergySaverStatusChanged;
			PowerManager.PowerSupplyStatusChanged -= PowerManager_PowerSupplyStatusChanged;
			PowerManager.RemainingChargePercentChanged -= PowerManager_RemainingChargePercentChanged;
#if WINDOWS
			PowerManager.RemainingDischargeTimeChanged -= PowerManager_RemainingDischargeTimeChanged;
#endif
		}

		private void PowerManager_BatteryStatusChanged(object sender, object e)
		{
			BatteryStatusText = PowerManager.BatteryStatus.ToString();
		}

		private void PowerManager_EnergySaverStatusChanged(object sender, object e)
		{
			EnergySaverStatusText = PowerManager.EnergySaverStatus.ToString();
		}

		private void PowerManager_PowerSupplyStatusChanged(object sender, object e)
		{
			PowerSupplyStatusText = PowerManager.PowerSupplyStatus.ToString();
		}

		private void PowerManager_RemainingChargePercentChanged(object sender, object e)
		{
			RemainingChargePercentStatusText = $"{PowerManager.RemainingChargePercent}%";
		}

		private void PowerManager_RemainingDischargeTimeChanged(object sender, object e)
		{
			RemainingDischargeTimeStatusText = PowerManager.RemainingDischargeTime.ToString();
		}

	}
}
