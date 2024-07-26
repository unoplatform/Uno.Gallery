using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.Samples
{

	[SamplePage(SampleCategory.UIFeatures, "Animation", Description = "Animations samples using VisualStates, DoubleAnimations, EasingFunctions and RenderTransforms", DocumentationLink = "https://docs.microsoft.com/en-us/windows/uwp/design/motion/xaml-animation")]
	public sealed partial class AnimationSamplePage : Page
	{
		private Control _sampleLayoutRoot;

		public AnimationSamplePage()
		{
			this.InitializeComponent();
			Loaded += (s, e) =>
			{
				_sampleLayoutRoot = SamplePageLayout.GetSampleChild<UserControl>(Design.Fluent, "FluentLayoutRoot");
				if (_sampleLayoutRoot != null)
				{
					VisualStateManager.GoToState(_sampleLayoutRoot, "NotAnimated", false);
				}
			};
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			if (_sampleLayoutRoot != null)
			{
				// Make sure we change state to allow for the button to work more than once.
				VisualStateManager.GoToState(_sampleLayoutRoot, "NotAnimated", false);
				VisualStateManager.GoToState(_sampleLayoutRoot, "Animation_1", true);
			}
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			if (_sampleLayoutRoot != null)
			{
				// Make sure we change state to allow for the button to work more than once.
				VisualStateManager.GoToState(_sampleLayoutRoot, "NotAnimated", false);
				VisualStateManager.GoToState(_sampleLayoutRoot, "Animation_2", true);
			}
		}

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			if (_sampleLayoutRoot != null)
			{
				// Make sure we change state to allow for the button to work more than once.
				VisualStateManager.GoToState(_sampleLayoutRoot, "NotAnimated", false);
				VisualStateManager.GoToState(_sampleLayoutRoot, "Animation_3", true);
			}
		}

		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			if (_sampleLayoutRoot != null)
			{
				// Make sure we change state to allow for the button to work more than once.
				VisualStateManager.GoToState(_sampleLayoutRoot, "NotAnimated", false);
				VisualStateManager.GoToState(_sampleLayoutRoot, "Animation_4", true);
			}
		}
	}
}
