using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "itemsrepeater", "tabbar", "textbox" })]
public sealed partial class ToolkitExtensionsSamplePage : Page
{
	private bool _accentState;

	public ToolkitExtensionsSamplePage()
	{
		InitializeComponent();
	}

	private void VerifyInput_Click(object sender, RoutedEventArgs e)
	{
		var first = GetSampleChild<TextBox>("NextInput");
		var submit = GetSampleChild<TextBox>("SubmitInput");
		var status = GetSampleChild<TextBlock>("InputStatus");
		if (first is null || submit is null || status is null)
		{
			return;
		}

		status.Text =
			$"First: {InputExtensions.GetReturnType(first)}, focus next: {InputExtensions.GetAutoFocusNextElement(first) == submit}; " +
			$"Submit: {InputExtensions.GetReturnType(submit)}, dismiss: {InputExtensions.GetAutoDismiss(submit)}";
	}

	private void ToggleProgrammatically_Click(object sender, RoutedEventArgs e)
	{
		var toggle = GetSampleChild<ToggleSwitch>("CommandToggle");
		if (toggle is not null)
		{
			toggle.IsOn = !toggle.IsOn;
		}
	}

	private void VerifyResources_Click(object sender, RoutedEventArgs e)
	{
		var button = GetSampleChild<Button>("ScopedResourceButton");
		var status = GetSampleChild<TextBlock>("ResourceStatus");
		if (button is null || status is null)
		{
			return;
		}

		var resources = ResourceExtensions.GetResources(button);
		status.Text = resources?.ContainsKey("ButtonBackground") == true
			? "Scoped resource ButtonBackground: #FF0063B1"
			: "Scoped resource unavailable";
	}

	private void ToggleState_Click(object sender, RoutedEventArgs e)
	{
		var button = GetSampleChild<Button>("StateButton");
		var status = GetSampleChild<TextBlock>("StateStatus");
		if (button is null || status is null)
		{
			return;
		}

		_accentState = !_accentState;
		var state = _accentState ? "Accent" : "Quiet";
		VisualStateManagerExtensions.SetStates(button, state);
		status.Text = $"State: {VisualStateManagerExtensions.GetStates(button)}";
	}

	private void NextSelection_Click(object sender, RoutedEventArgs e)
	{
		var flipView = GetSampleChild<FlipView>("ExtensionsFlipView");
		if (flipView is not null)
		{
			flipView.SelectedIndex = (flipView.SelectedIndex + 1) % flipView.Items.Count;
			UpdateSelectionStatus(flipView);
		}
	}

	private void ExtensionsFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is FlipView flipView)
		{
			UpdateSelectionStatus(flipView);
		}
	}

	private void UpdateSelectionStatus(FlipView flipView)
	{
		var tabBar = GetSampleChild<TabBar>("ExtensionsTabBar");
		var pager = GetSampleChild<PipsPager>("ExtensionsPager");
		var status = GetSampleChild<TextBlock>("SelectionStatus");
		if (tabBar is null || pager is null || status is null)
		{
			return;
		}

		status.Text =
			$"FlipView: {flipView.SelectedIndex}; TabBar: {tabBar.SelectedIndex}; " +
			$"PipsPager: {pager.SelectedPageIndex}; offset: {SelectorExtensions.GetSelectionOffset(flipView):F2}";
	}

	private T? GetSampleChild<T>(string name) where T : FrameworkElement
		=> SamplePageLayoutRoot.GetSampleChild<T>(Design.Agnostic, name);
}

[Microsoft.UI.Xaml.Data.Bindable]
public sealed class ToolkitExtensionsViewModel : ViewModelBase
{
	public ToolkitExtensionsViewModel()
	{
		SubmitStatus = "Submitted: (none)";
		ToggleStatus = "Toggle command value: False";
		SubmitCommand = new Command(parameter => SubmitStatus = $"Submitted: {parameter}");
		ToggleCommand = new Command(parameter => ToggleStatus = $"Toggle command value: {parameter}");
	}

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
