#if USE_UITESTS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;

namespace Uno.Gallery;

public partial class SamplePageLayout
{
	private void InitializeUITestMarker()
	{
		AutomationProperties.SetAutomationId(this, "SampleHostLoaded");
		Loaded += OnUITestLoaded;
	}

	private void OnUITestLoaded(object sender, RoutedEventArgs args)
	{
		if (DataContext is Sample sample)
		{
			var shell = Shell.GetForElement(this);
			shell.UITestSampleHostLoadedState = sample.Slug + "\n" + CurrentDesign;
		}
	}
}
#endif
