// Adapted from unoplatform/uno.toolkit.ui samples/Uno.Toolkit.Samples/Content/Controls/LoadingViewSample.xaml.cs (MIT)
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;
using Uno.Gallery.ViewModels;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "LoadingView",
		SourceSdk.UnoToolkit,
		Description = "Displays loading indicators over content while one or more ILoadable sources are executing.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/LoadingView.html",
		DataType = typeof(LoadingViewSampleViewModel),
		Tags = new[] { "loading", "async", "feedback", "progress" },
		Status = SampleStatus.Stable,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		SortOrder = 30)]
	public sealed partial class LoadingViewSamplePage : Page
	{
		public LoadingViewSamplePage()
		{
			this.InitializeComponent();
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public class LoadingViewSampleViewModel : ViewModelBase
	{
		public string ResultText { get => GetProperty<string>() ?? "Press the button to load data."; set => SetProperty(value); }
		public string SlowResultText { get => GetProperty<string>() ?? "Press either button to start loading."; set => SetProperty(value); }

		public LoadingAsyncCommand LoadCommand { get; }
		public LoadingAsyncCommand SlowLoadCommand { get; }

		public LoadingViewSampleViewModel()
		{
			LoadCommand = new LoadingAsyncCommand(() => SimulateLoad(2_000, t => ResultText = $"Loaded at {t:HH:mm:ss}"));
			SlowLoadCommand = new LoadingAsyncCommand(() => SimulateLoad(4_000, t => SlowResultText = $"Slow-loaded at {t:HH:mm:ss}"));
		}

		private static async Task SimulateLoad(int delayMs, Action<DateTime> onCompleted)
		{
			await Task.Delay(delayMs);
			onCompleted(DateTime.Now);
		}

		/// <summary>
		/// A minimal async command that also implements <see cref="Uno.Toolkit.ILoadable"/>
		/// so it can be bound directly to <c>utu:LoadableSource.Source</c>.
		/// </summary>
		[Microsoft.UI.Xaml.Data.Bindable]
		public sealed class LoadingAsyncCommand : ICommand, Uno.Toolkit.ILoadable
		{
			private readonly Func<Task> _executeAsync;
			private bool _isExecuting;

			public event EventHandler? CanExecuteChanged;
			public event EventHandler? IsExecutingChanged;

			public LoadingAsyncCommand(Func<Task> executeAsync)
			{
				_executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
			}

			public bool IsExecuting
			{
				get => _isExecuting;
				private set
				{
					if (_isExecuting == value) return;
					_isExecuting = value;
					IsExecutingChanged?.Invoke(this, EventArgs.Empty);
					CanExecuteChanged?.Invoke(this, EventArgs.Empty);
				}
			}

			public bool CanExecute(object? parameter) => !IsExecuting;

			public async void Execute(object? parameter)
			{
				try
				{
					IsExecuting = true;
					await _executeAsync();
				}
				finally
				{
					IsExecuting = false;
				}
			}
		}
	}
}
