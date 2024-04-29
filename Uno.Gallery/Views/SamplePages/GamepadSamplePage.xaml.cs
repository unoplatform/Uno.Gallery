using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uno.Gallery.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Gaming.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Gamepad", Description = "Represents information about the connected gamepads.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.gaming.input.gamepad", DataType = typeof(GamepadSamplePageViewModel))]
	public sealed partial class GamepadSamplePage : Page
	{
		public GamepadSamplePage()
		{
			this.InitializeComponent();
		}

		private void CheckChangeObservation_Click(object sender, RoutedEventArgs e)
		{
			if(((Sample)DataContext).Data is GamepadSamplePageViewModel viewModel)
			{
				if(viewModel.ObservingChanges)
				{
					viewModel.StopObservingGamepadChanges();
				} else
				{
					viewModel.StartObservingGamepadChanges();
				}
			}
		}
	}

	public class GamepadSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing gamepad changes";
		private const string _stopObservingContent = "Stop observing gamepad changes";

		private DispatcherTimer _timer = new DispatcherTimer();

		public bool ObservingChanges { get => GetProperty<bool>(); set => SetProperty(value); }

		public string ButtonText { get => GetProperty<string>(); set => SetProperty(value); }

		public ObservableCollection<GamepadViewModel> AvailableGamepads { get; } = new ObservableCollection<GamepadViewModel>();


		public GamepadSamplePageViewModel()
		{
			ButtonText = _startObservingContent;
		}

		public void StartObservingGamepadChanges()
		{
			Gamepad.GamepadAdded += GamepadsChanged;
			Gamepad.GamepadRemoved += GamepadsChanged;

			_timer.Interval = TimeSpan.FromMilliseconds(100);
			_timer.Tick += OnGamepadReadingUpdate;
			_timer.Start();

			UpdateGamepads();

			ButtonText = _stopObservingContent;
			ObservingChanges = true;
		}

		public void StopObservingGamepadChanges()
		{
			Gamepad.GamepadAdded -= GamepadsChanged;
			Gamepad.GamepadRemoved -= GamepadsChanged;

			_timer.Stop();

			AvailableGamepads.Clear();

			ButtonText = _startObservingContent;
			ObservingChanges = false;
		}


		private void OnGamepadReadingUpdate(object sender, object e)
		{
			UpdateGamepads();
			foreach(var gamepad in AvailableGamepads)
			{
				gamepad.Update();
				gamepad.Position = AvailableGamepads.IndexOf(gamepad) + 1;
			}
		}

		private void GamepadsChanged(object sender, Gamepad e)
		{
			UpdateGamepads();
		}

		private async void UpdateGamepads()
		{
			_ = App.Instance.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
			{
				var existingGamepads = new HashSet<Gamepad>(AvailableGamepads.Select(g => g.Gamepad));

				var gamepadsToRemove = existingGamepads.Except(Gamepad.Gamepads);
				var gamepadsToAdd = Gamepad.Gamepads.Except(existingGamepads);

				foreach(var gamepad in gamepadsToRemove)
				{
					var vmToRemove = AvailableGamepads.FirstOrDefault(g => g.Gamepad == gamepad);
					AvailableGamepads.Remove(vmToRemove);
				}

				foreach(var gamepad in gamepadsToAdd)
				{
					var vmToAdd = new GamepadViewModel(gamepad);
					AvailableGamepads.Add(vmToAdd);
				}
			});
		}
	}

	public class GamepadViewModel : ViewModelBase
	{
		public GamepadViewModel(Gamepad gamepad)
		{
			Gamepad = gamepad;
		}

		public int Position { get => GetProperty<int>(); set => SetProperty(value); }

		public Gamepad Gamepad { get; }

		public void Update()
		{
			var reading = Gamepad.GetCurrentReading();

			Buttons = reading.Buttons.ToString("g");

			LeftThumbstickX = reading.LeftThumbstickX.ToString("0.00");
			LeftThumbstickY = reading.LeftThumbstickY.ToString("0.00");
			RightThumbstickX = reading.RightThumbstickX.ToString("0.00");
			RightThumbstickY = reading.RightThumbstickY.ToString("0.00");

			ButtonA = Buttons.Contains("A");
			ButtonX = Buttons.Contains("X");
			ButtonY = Buttons.Contains("Y");
			ButtonB = Buttons.Contains("B");

			DPadDown = Buttons.Contains("DPadDown");
			DPadLeft = Buttons.Contains("DPadLeft");
			DPadUp = Buttons.Contains("DPadUp");
			DPadRight = Buttons.Contains("DPadRight");

			ButtonView = Buttons.Contains("View");
			ButtonMenu = Buttons.Contains("Menu");

			ButtonPaddle1 = Buttons.Contains("Paddle1");
			ButtonPaddle2 = Buttons.Contains("Paddle2");
			ButtonPaddle3 = Buttons.Contains("Paddle3");
			ButtonPaddle4 = Buttons.Contains("Paddle4");

			ButtonLeftShoulder = Buttons.Contains("LeftShoulder");
			ButtonRightShoulder = Buttons.Contains("RightShoulder");

			ButtonLeftThumbstick = Buttons.Contains("LeftThumbstick");
			ButtonRightThumbstick = Buttons.Contains("RightThumbstick");

			LeftThumbstickXImage = reading.LeftThumbstickX * 6 + 75;
			LeftThumbstickYImage = reading.LeftThumbstickY * (-6) + 49;
			RightThumbstickXImage = reading.RightThumbstickX * 6 + 141;
			RightThumbstickYImage = reading.RightThumbstickY * (-6) + 80;

			LeftTrigger = reading.LeftTrigger.ToString("0.00");
			RightTrigger = reading.RightTrigger.ToString("0.00");
		}

		public string Buttons { get => GetProperty<string>(); private set => SetProperty(value); }

		public string LeftThumbstickX { get => GetProperty<string>(); private set => SetProperty(value); }
		public string LeftThumbstickY { get => GetProperty<string>(); private set => SetProperty(value); }
		public string RightThumbstickX { get => GetProperty<string>(); private set => SetProperty(value); }
		public string RightThumbstickY { get => GetProperty<string>(); private set => SetProperty(value); }

		public string LeftTrigger { get => GetProperty<string>(); private set => SetProperty(value); }
		public string RightTrigger { get => GetProperty<string>(); private set => SetProperty(value); }

		public bool ButtonLeftThumbstick { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonRightThumbstick { get => GetProperty<bool>(); private set => SetProperty(value); }

		public double LeftThumbstickXImage { get => GetProperty<double>(); private set => SetProperty(value); }
		public double LeftThumbstickYImage { get => GetProperty<double>(); private set => SetProperty(value); }
		public double RightThumbstickXImage { get => GetProperty<double>(); private set => SetProperty(value); }
		public double RightThumbstickYImage { get => GetProperty<double>(); private set => SetProperty(value); }

		public bool ButtonA { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonX { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonY { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonB { get => GetProperty<bool>(); private set => SetProperty(value); }

		public bool DPadDown { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool DPadLeft { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool DPadUp { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool DPadRight { get => GetProperty<bool>(); private set => SetProperty(value); }

		public bool ButtonView { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonMenu { get => GetProperty<bool>(); private set => SetProperty(value); }

		public bool ButtonPaddle1 { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonPaddle2 { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonPaddle3 { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonPaddle4 { get => GetProperty<bool>(); private set => SetProperty(value); }

		public bool ButtonLeftShoulder { get => GetProperty<bool>(); private set => SetProperty(value); }
		public bool ButtonRightShoulder { get => GetProperty<bool>(); private set => SetProperty(value); }
	}
}
