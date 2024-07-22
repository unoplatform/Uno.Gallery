﻿using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "Flyout", Description = "A flyout is a UI container that can be light dismissed. It can contain other flyouts or context menus to create a nested experience.", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/dialogs-and-flyouts/flyouts")]
public sealed partial class FlyoutSamplePage : Page
{
	public FlyoutSamplePage()
	{
		this.InitializeComponent();
	}
}
