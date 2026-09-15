#if USE_UITESTS
namespace Uno.Gallery;

public sealed partial class Shell
{
	internal string UITestSampleHostLoadedState { get; set; } = string.Empty;

	internal string UITestUnhandledExceptionState { get; set; } = string.Empty;

	internal bool UITestSmokeCaptureEnabled { get; set; }

	private void InitializeUITestResponse()
		=> UITestBackdoorResponse.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

	internal void SetUITestResponse(string response)
		=> UITestBackdoorResponse.Text = response;
}
#endif
