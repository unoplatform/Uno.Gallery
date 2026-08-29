using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery;
using Uno.Toolkit.UI;

namespace Uno.Gallery.Views.SamplePages
{
	[SamplePage(SampleCategory.Toolkit, "Drawer",
		SourceSdk.UnoToolkit,
		Description = "DrawerControl and DrawerFlyoutPresenter: off-canvas panels that can be revealed by swipe gesture or programmatically.",
		DocumentationLink = "https://platform.uno/docs/articles/external/uno.toolkit.ui/doc/controls/DrawerControl.html",
		Tags = new[] { "navigation", "drawer", "gesture", "flyout" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Agnostic,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "Swipe variants require touch or another pointer gesture; buttons provide an equivalent interaction." },
		AccessibilityNotes = new[] { "Named Open and Close buttons provide keyboard alternatives to edge-swipe and light-dismiss gestures." },
		ResetBehavior = "Close an open drawer or dismiss its flyout to return to the initial state.",
		Variants = new[] { "Left DrawerControl", "Left DrawerFlyoutPresenter", "Half-height bottom drawer" },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "safearea" },
		SortOrder = 20)]
	public sealed partial class DrawerSamplePage : Page
	{
		private DrawerControl? _drawerControl;
		private Button? _closeButton;

		public DrawerSamplePage()
		{
			this.InitializeComponent();
			this.Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			_drawerControl = SamplePageLayout.GetSampleChild<DrawerControl>(Design.Agnostic, "SampleDrawerControl");
			_closeButton = SamplePageLayout.GetSampleChild<Button>(Design.Agnostic, "DrawerCloseButton");
			if (_closeButton is not null)
				_closeButton.Click += OnCloseButtonClick;
		}

		private void OnCloseButtonClick(object sender, RoutedEventArgs e)
		{
			if (_drawerControl is not null)
				_drawerControl.IsOpen = false;
		}
	}
}
