using System;
using System.ComponentModel;
using Uno.Extensions;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Store;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
    [SamplePage(SampleCategory.NonUIFeatures, "Geolocator", Description = "Represents a geolocator sensor. This sensor returns latitude, longitude, longitude and accuracy.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.geolocation.geolocator")]
    public sealed partial class GeolocatorSamplePage : Page
    {
        public GeolocatorSamplePage()
        {
            this.InitializeComponent();
        }

        private void GetGeopositionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GeolocatorSamplePageViewModel viewModel)
            {
                viewModel.GetGeoposition();
            }
        }

        private void ToggleGeopositionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GeolocatorSamplePageViewModel viewModel)
            {
                viewModel.ToggleTracker();
            }
        }
    }

    public class GeolocatorSamplePageViewModel : INotifyPropertyChanged
    {
        private Geolocator _geolocator;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isTracking = false;
        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                _isTracking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTracking)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToggleButtonContent)));
            }
        }
        public string ButtonContent => "Get geoposition";
        public string ToggleButtonContent => _isTracking ? "Stop tracking" : "Start tracking";

        private double? _GeolocatedLatitude;
        public double? GeolocatedLatitude
        {
            get => _GeolocatedLatitude;
            set
            {
                _GeolocatedLatitude = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeolocatedLatitude)));
            }
        }
        
        private double? _GeolocatedLongitude;
        public double? GeolocatedLongitude
        {
            get => _GeolocatedLongitude;
            set
            {
                _GeolocatedLongitude = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeolocatedLongitude)));
            }
        }
        
        private double? _GeolocatedAltitude;
        public double? GeolocatedAltitude
        {
            get => _GeolocatedAltitude;
            set
            {
                _GeolocatedAltitude = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeolocatedAltitude)));
            }
        }
        
        private double? _GeolocatedAccuracy;
        public double? GeolocatedAccuracy
        {
            get => _GeolocatedAccuracy;
            set
            {
                _GeolocatedAccuracy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeolocatedAccuracy)));
            }
        }
        
        private DateTime _GeolocatedTimestamp;
        public DateTime GeolocatedTimestamp
        {
            get => _GeolocatedTimestamp;
            set
            {
                _GeolocatedTimestamp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeolocatedTimestamp)));
            }
        }

        public GeolocatorSamplePageViewModel()
        {
            _geolocator = new Geolocator();
        }

        public async void GetGeoposition()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch(accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    var geoposition = await _geolocator.GetGeopositionAsync();
                    UpdateGeolocation(geoposition);
                    break;
                case GeolocationAccessStatus.Denied:
                    ShowMessageGeolocationAccessDenied();
                    break;
                case GeolocationAccessStatus.Unspecified:
                    ShowMessageGeolocationAccessUnspecified();
                    break;
            }
        }

        public async void ToggleTracker()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch(accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    if (IsTracking)
                        _geolocator.PositionChanged -= _geolocator_PositionChanged;
                    else
                        _geolocator.PositionChanged += _geolocator_PositionChanged;
                    IsTracking = !IsTracking;
                    break;
                case GeolocationAccessStatus.Denied:
                    ShowMessageGeolocationAccessDenied();
                    break;
                case GeolocationAccessStatus.Unspecified:
                    ShowMessageGeolocationAccessUnspecified();
                    break;
            }
        }

        private async void _geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateGeolocation(args?.Position);
            });
        }

        private async void ShowMessageGeolocationAccessDenied()
        {
            // Code to handle access to location services denied.
        }

        private async void ShowMessageGeolocationAccessUnspecified()
        {
            // Code to handle access to location services with unspecified status.
        }

        private void UpdateGeolocation(Geoposition position)
        {
            GeolocatedAccuracy = position?.Coordinate?.Accuracy;
            GeolocatedAltitude = position?.Coordinate?.Point?.Position.Altitude;
            GeolocatedLatitude = position?.Coordinate?.Point?.Position.Latitude;
            GeolocatedLongitude = position?.Coordinate?.Point?.Position.Longitude;
            GeolocatedTimestamp = DateTime.Now;
        }
    }
}
