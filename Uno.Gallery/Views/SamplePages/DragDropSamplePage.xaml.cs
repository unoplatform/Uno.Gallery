using System;
using System.Threading.Tasks;
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
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { "Uses an in-app text payload and requires no file-system permission or external data." },
	AccessibilityNotes = new[] { "The deterministic transfer button provides a keyboard alternative and every result is announced as text." },
	ResetBehavior = "Reload the sample to clear the transfer count and status.",
	Variants = new[] { "Pointer drag and drop", "Keyboard-accessible deterministic transfer", "Rejected non-text payload" },
	KnownLimitations = new[]
	{
		"Pointer drag gesture support varies by target; the keyboard-accessible deterministic transfer remains available.",
		"OS file drops are excluded because they require host permissions and an external payload."
	},
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "clipboard" })]
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
		=> await ProcessTransferAsync(e.DataView, "drag/drop");

	private async void DeterministicTransfer_Click(object sender, RoutedEventArgs e)
	{
		var package = new DataPackage
		{
			RequestedOperation = DataPackageOperation.Copy
		};
		package.SetText(Payload);
		await ProcessTransferAsync(package.GetView(), "deterministic data package");
	}

	private async Task ProcessTransferAsync(DataPackageView dataView, string path)
	{
		try
		{
			if (dataView.Contains(StandardDataFormats.Text))
			{
				CompleteTransfer(await dataView.GetTextAsync(), path);
			}
			else
			{
				UpdateStatus("Rejected transfer: the payload did not contain text.");
			}
		}
		catch (Exception error)
		{
			UpdateStatus($"Transfer failed: {error.GetType().Name}.");
		}
	}

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
