using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Uno.Gallery.Helpers;
using Uno.Gallery.ViewModels;
using Uno.Toolkit.UI;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.Toolkit, "Toolkit Extensions",
	SourceSdk.UnoToolkit,
	Description = "Practical Toolkit attached helpers grouped by input and commands, scoped resources and visual states, and selector synchronization.",
	DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls-styles.html",
	Slug = "toolkit-extensions",
	DataType = typeof(ToolkitExtensionsViewModel),
	Tags = new[] { "attached-properties", "input", "command", "resources", "visual-state", "selector", "tabbar" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
	AccessibilityNotes = new[] { "Input, toggle, resource, state, and selection examples provide named controls plus deterministic text status." },
	ResetBehavior = "Page-local visual states reset when reopened; command result text is cached for this window until the Gallery process restarts.",
	Variants = new[] { "Input and command helpers", "Scoped resources", "Attached visual states", "Selector synchronization" },
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "itemsrepeater", "tabbar", "textbox" })]
public sealed partial class ToolkitExtensionsSamplePage : Page
{
	private bool _accentState;
	private ToolkitExtensionsViewModel? _viewModel;

	public ToolkitExtensionsSamplePage()
	{
		InitializeComponent();
		Loaded += OnPageLoaded;
		Unloaded += OnPageUnloaded;
	}

	private void OnPageLoaded(object sender, RoutedEventArgs e)
	{
		if (DataContext is Sample { Data: ToolkitExtensionsViewModel viewModel } &&
			!ReferenceEquals(_viewModel, viewModel))
		{
			if (_viewModel is not null)
			{
				_viewModel.AnnouncementRequested -= OnAnnouncementRequested;
			}
			_viewModel = viewModel;
			_viewModel.AnnouncementRequested += OnAnnouncementRequested;
		}
	}

	private void OnPageUnloaded(object sender, RoutedEventArgs e)
	{
		if (_viewModel is not null)
		{
			_viewModel.AnnouncementRequested -= OnAnnouncementRequested;
			_viewModel = null;
		}
	}

	private void OnAnnouncementRequested(string targetName, string text)
		=> AccessibilityHelper.Announce(GetRequiredChild<TextBlock>(targetName), text);

	private void VerifyInput_Click(object sender, RoutedEventArgs e)
	{
		var first = GetRequiredChild<TextBox>("NextInput");
		var submit = GetRequiredChild<TextBox>("SubmitInput");
		var status = GetRequiredChild<TextBlock>("InputStatus");
		AccessibilityHelper.Announce(status,
			$"First: {InputExtensions.GetReturnType(first)}, focus next: {InputExtensions.GetAutoFocusNextElement(first) == submit}; " +
			$"Submit: {InputExtensions.GetReturnType(submit)}, dismiss: {InputExtensions.GetAutoDismiss(submit)}");
	}

	private void ToggleProgrammatically_Click(object sender, RoutedEventArgs e)
	{
		var toggle = GetRequiredChild<ToggleSwitch>("CommandToggle");
		toggle.IsOn = !toggle.IsOn;
	}

	private void VerifyResources_Click(object sender, RoutedEventArgs e)
	{
		var button = GetRequiredChild<Button>("ScopedResourceButton");
		var status = GetRequiredChild<TextBlock>("ResourceStatus");
		button.ApplyTemplate();
		button.UpdateLayout();
		if (button.Background is not SolidColorBrush brush)
		{
			throw new InvalidOperationException("ResourceExtensions did not resolve the button background brush.");
		}

		AccessibilityHelper.Announce(status, $"Resolved background: {brush.Color}");
	}

	private void ToggleState_Click(object sender, RoutedEventArgs e)
	{
		var button = GetRequiredChild<Button>("StateButton");
		var status = GetRequiredChild<TextBlock>("StateStatus");

		_accentState = !_accentState;
		var state = _accentState ? "Accent" : "Quiet";
		VisualStateManagerExtensions.SetStates(button, state);
		button.UpdateLayout();
		var stateRoot = GetRequiredChild<Grid>("StateRoot");
		if (stateRoot.Background is not SolidColorBrush brush)
		{
			throw new InvalidOperationException("The attached visual state did not resolve a background brush.");
		}
		AccessibilityHelper.Announce(
			status,
			$"State: {VisualStateManagerExtensions.GetStates(button)}; background: {brush.Color}");
	}

	private void NextSelection_Click(object sender, RoutedEventArgs e)
	{
		var flipView = GetRequiredChild<FlipView>("ExtensionsFlipView");
		flipView.SelectedIndex = (flipView.SelectedIndex + 1) % flipView.Items.Count;
	}

	private void ExtensionsFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is FlipView flipView)
		{
			DispatcherQueue.TryEnqueue(() => UpdateSelectionStatus(flipView));
		}
	}

	private void UpdateSelectionStatus(FlipView flipView)
	{
		var tabBar = GetRequiredChild<TabBar>("ExtensionsTabBar");
		var pager = GetRequiredChild<PipsPager>("ExtensionsPager");
		var status = GetRequiredChild<TextBlock>("SelectionStatus");
		AccessibilityHelper.Announce(status,
			$"FlipView: {flipView.SelectedIndex}; TabBar: {tabBar.SelectedIndex}; " +
			$"PipsPager: {pager.SelectedPageIndex}");
	}

	private T GetRequiredChild<T>(string name) where T : FrameworkElement
		=> SamplePageLayoutRoot.GetSampleChild<T>(Design.Agnostic, name)
			?? throw new InvalidOperationException($"Toolkit extensions sample child '{name}' is not loaded.");
}

[Microsoft.UI.Xaml.Data.Bindable]
public sealed class ToolkitExtensionsViewModel : ViewModelBase
{
	public ToolkitExtensionsViewModel()
	{
		SubmitStatus = "Submitted: (none)";
		ToggleStatus = "Toggle command value: False";
		SubmitCommand = new Command(parameter =>
		{
			SubmitStatus = $"Submitted: {parameter}";
			AnnouncementRequested?.Invoke("SubmitStatusText", SubmitStatus);
		});
		ToggleCommand = new Command(parameter =>
		{
			ToggleStatus = $"Toggle command value: {parameter}";
			AnnouncementRequested?.Invoke("CommandStatusText", ToggleStatus);
		});
	}

	public event Action<string, string>? AnnouncementRequested;

	public ICommand SubmitCommand { get; }

	public ICommand ToggleCommand { get; }

	public string SubmitStatus
	{
		get => GetProperty<string>();
		set => SetProperty(value);
	}

	public string ToggleStatus
	{
		get => GetProperty<string>();
		set => SetProperty(value);
	}
}
