using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;
using Windows.ApplicationModel.DataTransfer;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIFeatures, "Drag and Drop",
	Description = "Transfers an app-owned text/card payload with XAML drag/drop and provides a deterministic equivalent action for automation.",
	DocumentationLink = "https://learn.microsoft.com/windows/apps/design/input/drag-and-drop",
	Slug = "drag-drop",
	Tags = new[] { "input", "drag", "drop", "transfer", "offline" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "clipboard", "composition-visuals" })]
public sealed partial class DragDropSamplePage : Page
{
	private const string Payload = "Gallery card: deterministic payload";
	private int _transferCount;

	public DragDropSamplePage()
	{
		InitializeComponent();
	}

	private void Source_DragStarting(UIElement sender, DragStartingEventArgs args)
	{
		args.Data.SetText(Payload);
		args.Data.RequestedOperation = DataPackageOperation.Copy;
	}

	private void Target_DragOver(object sender, DragEventArgs e)
	{
		e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.Text)
			? DataPackageOperation.Copy
			: DataPackageOperation.None;
	}

	private async void Target_Drop(object sender, DragEventArgs e)
	{
		if (e.DataView.Contains(StandardDataFormats.Text))
		{
			CompleteTransfer(await e.DataView.GetTextAsync(), "drag/drop");
		}
		else
		{
			UpdateStatus("Rejected transfer: the payload did not contain app-owned text.");
		}
	}

	private void DeterministicTransfer_Click(object sender, RoutedEventArgs e)
		=> CompleteTransfer(Payload, "deterministic action");

	private void CompleteTransfer(string payload, string path)
	{
		_transferCount++;
		UpdateStatus($"Transfer {_transferCount} via {path}: {payload}");
	}

	private void UpdateStatus(string message)
	{
		if (SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "DragDrop_Status") is { } status)
		{
			AccessibilityHelper.Announce(status, message);
		}
	}
}
