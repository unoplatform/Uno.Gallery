using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Gallery.Views.Samples
{

	[SamplePage(SampleCategory.Features, "Animation", Description = "Animations samples using VisualStates, DoubleAnimations, EasingFunctions and RenderTransforms")]
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
