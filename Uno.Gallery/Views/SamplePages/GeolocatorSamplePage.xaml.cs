using System;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.NonUIFeatures, "Geolocator",
	Description = "Requests location and visibly reports allowed, denied, unspecified, and error states.",
	DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.geolocation.geolocator",
	Slug = "geolocator",
	Tags = new[] { "location", "permissions", "sensor", "platform" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Live location requires a platform location provider and user permission; the denied-state preview is local and deterministic." },
	AccessibilityNotes = new[] { "Actions are keyboard-focusable and permission, tracking, success, empty, and failure states are exposed as text." },
	ResetBehavior = "Stop tracking to detach the position listener; leaving the sample also stops active tracking.",
	Variants = new[] { "One-shot position", "Position tracking", "Deterministic denied-permission preview" },
	KnownLimitations = new[] { "Coordinates, precision, permission prompts, and provider availability depend on the host device and browser." },
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27")]
public sealed partial class GeolocatorSamplePage : Page
{
	public GeolocatorSamplePage()
	{
		InitializeComponent();
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

	private void PreviewDeniedButton_Click(object sender, RoutedEventArgs e)
	{
		if ((sender as Button)?.DataContext is GeolocatorSamplePageViewModel viewModel)
		{
			viewModel.SetAccessStatus(GeolocationAccessStatus.Denied);
			if (LocalSamplePageLayout.GetSampleChild<TextBlock>(Design.Agnostic, "GeolocatorStatus") is { } status)
			{
				AccessibilityHelper.Announce(status, viewModel.StatusMessage);
			}
		}
	}

	private void GeolocatorRoot_Unloaded(object sender, RoutedEventArgs e)
	{
		if ((sender as FrameworkElement)?.DataContext is GeolocatorSamplePageViewModel viewModel)
		{
			viewModel.StopTracking();
		}
	}
}

[Microsoft.UI.Xaml.Data.Bindable]
public class GeolocatorSamplePageViewModel : INotifyPropertyChanged
{
	private readonly Geolocator _geolocator = new();
	private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
	private bool _isTracking;
	private string _statusMessage = "Location has not been requested.";

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool IsTracking
	{
		get => _isTracking;
		set
		{
			_isTracking = value;
			RaisePropertyChanged(nameof(IsTracking));
			RaisePropertyChanged(nameof(ToggleButtonContent));
		}
	}

	public string ButtonContent => "Get geoposition";
	public string ToggleButtonContent => _isTracking ? "Stop tracking" : "Start tracking";

	public string StatusMessage
	{
		get => _statusMessage;
		private set
		{
			_statusMessage = value;
			RaisePropertyChanged(nameof(StatusMessage));
		}
	}

	private double? _geolocatedLatitude;
	public double? GeolocatedLatitude
	{
		get => _geolocatedLatitude;
		set { _geolocatedLatitude = value; RaisePropertyChanged(nameof(GeolocatedLatitude)); }
	}

	private double? _geolocatedLongitude;
	public double? GeolocatedLongitude
	{
		get => _geolocatedLongitude;
		set { _geolocatedLongitude = value; RaisePropertyChanged(nameof(GeolocatedLongitude)); }
	}

	private double? _geolocatedAltitude;
	public double? GeolocatedAltitude
	{
		get => _geolocatedAltitude;
		set { _geolocatedAltitude = value; RaisePropertyChanged(nameof(GeolocatedAltitude)); }
	}

	private double? _geolocatedAccuracy;
	public double? GeolocatedAccuracy
	{
		get => _geolocatedAccuracy;
		set { _geolocatedAccuracy = value; RaisePropertyChanged(nameof(GeolocatedAccuracy)); }
	}

	private DateTime _geolocatedTimestamp;
	public DateTime GeolocatedTimestamp
	{
		get => _geolocatedTimestamp;
		set { _geolocatedTimestamp = value; RaisePropertyChanged(nameof(GeolocatedTimestamp)); }
	}

	public async void GetGeoposition()
	{
		try
		{
			var accessStatus = await Geolocator.RequestAccessAsync();
			if (accessStatus != GeolocationAccessStatus.Allowed)
			{
				SetAccessStatus(accessStatus);
				return;
			}

			StatusMessage = "Location access allowed; waiting for a position.";
			UpdateGeolocation(await _geolocator.GetGeopositionAsync());
		}
		catch (Exception ex)
		{
			StatusMessage = $"Location request failed: {ex.GetType().Name}.";
		}
	}

	public async void ToggleTracker()
	{
		try
		{
			var accessStatus = await Geolocator.RequestAccessAsync();
			if (accessStatus != GeolocationAccessStatus.Allowed)
			{
				SetAccessStatus(accessStatus);
				return;
			}

			if (IsTracking)
			{
				_geolocator.PositionChanged -= Geolocator_PositionChanged;
			}
			else
			{
				_geolocator.PositionChanged += Geolocator_PositionChanged;
			}

			IsTracking = !IsTracking;
			StatusMessage = IsTracking ? "Position tracking started." : "Position tracking stopped.";
		}
		catch (Exception ex)
		{
			StatusMessage = $"Location tracking failed: {ex.GetType().Name}.";
		}
	}

	public void SetAccessStatus(GeolocationAccessStatus accessStatus)
		=> StatusMessage = accessStatus switch
		{
			GeolocationAccessStatus.Denied => "Location access denied. Enable location permission in system settings.",
			GeolocationAccessStatus.Unspecified => "Location access is unspecified. The platform did not return a permission decision.",
			GeolocationAccessStatus.Allowed => "Location access allowed.",
			_ => $"Unknown location access state: {accessStatus}."
		};

	public void StopTracking()
	{
		if (!IsTracking) return;

		_geolocator.PositionChanged -= Geolocator_PositionChanged;
		IsTracking = false;
		StatusMessage = "Position tracking stopped.";
	}

	private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
		=> _ = _dispatcherQueue.TryEnqueue(() => UpdateGeolocation(args?.Position));

	private void UpdateGeolocation(Geoposition? position)
	{
		GeolocatedAccuracy = position?.Coordinate?.Accuracy;
		GeolocatedAltitude = position?.Coordinate?.Point?.Position.Altitude;
		GeolocatedLatitude = position?.Coordinate?.Point?.Position.Latitude;
		GeolocatedLongitude = position?.Coordinate?.Point?.Position.Longitude;
		GeolocatedTimestamp = DateTime.Now;
		StatusMessage = position is null
			? "The location provider returned no position."
			: "Position received successfully.";
	}

	private void RaisePropertyChanged(string propertyName)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
