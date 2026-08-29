using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Helpers;

internal static class AccessibilityHelper
{
	public static void Announce(TextBlock target, string text)
	{
		target.Text = text;
		var peer = FrameworkElementAutomationPeer.FromElement(target)
			?? FrameworkElementAutomationPeer.CreatePeerForElement(target);
		peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
	}
}
