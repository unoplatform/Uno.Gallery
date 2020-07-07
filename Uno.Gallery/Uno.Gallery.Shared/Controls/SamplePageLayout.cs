using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Uno.Gallery.Controls
{
	/// <summary>
	/// This control is used as a template for each sample page.
	/// </summary>
	public partial class SamplePageLayout : ContentControl
	{
		private const string VisualStateDefault = "Default";
		private const string VisualStateMaterial = "Material";
		private const string VisualStateNative = "Native";

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof(string), typeof(SamplePageLayout), new PropertyMetadata(string.Empty));


		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register("Description", typeof(string), typeof(SamplePageLayout), new PropertyMetadata(string.Empty));

		public SamplePageLayoutMode Mode
		{
			get { return (SamplePageLayoutMode)GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}

		public static readonly DependencyProperty ModeProperty =
			DependencyProperty.Register("Mode", typeof(SamplePageLayoutMode), typeof(SamplePageLayout), new PropertyMetadata(SamplePageLayoutMode.Default, OnModeChanged));

		public DataTemplate MaterialTemplate
		{
			get { return (DataTemplate)GetValue(MaterialTemplateProperty); }
			set { SetValue(MaterialTemplateProperty, value); }
		}

		public static readonly DependencyProperty MaterialTemplateProperty =
			DependencyProperty.Register("MaterialTemplate", typeof(DataTemplate), typeof(SamplePageLayout), new PropertyMetadata(null));

		public DataTemplate NativeTemplate
		{
			get { return (DataTemplate)GetValue(NativeTemplateProperty); }
			set { SetValue(NativeTemplateProperty, value); }
		}

		public static readonly DependencyProperty NativeTemplateProperty =
			DependencyProperty.Register("NativeTemplate", typeof(DataTemplate), typeof(SamplePageLayout), new PropertyMetadata(null));

		public DataTemplate DefaultTemplate
		{
			get { return (DataTemplate)GetValue(DefaultTemplateProperty); }
			set { SetValue(DefaultTemplateProperty, value); }
		}

		public static readonly DependencyProperty DefaultTemplateProperty =
			DependencyProperty.Register("DefaultTemplate", typeof(DataTemplate), typeof(SamplePageLayout), new PropertyMetadata(null));

		public SamplePageLayout()
		{
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			UpdateVisualState();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			UpdateVisualState();
		}

		private void UpdateVisualState()
		{
			switch (Mode)
			{
				case SamplePageLayoutMode.Material:
					VisualStateManager.GoToState(this, VisualStateMaterial, useTransitions: true);
					break;
				case SamplePageLayoutMode.Native:
					VisualStateManager.GoToState(this, VisualStateNative, useTransitions: true);
					break;
				case SamplePageLayoutMode.Default:
				default:
					VisualStateManager.GoToState(this, VisualStateDefault, useTransitions: true);
					break;
			}
		}

		private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (SamplePageLayout)d;
			control.UpdateVisualState();
		}
	}

	public enum SamplePageLayoutMode
	{
		Default,
		Material,
		Native
	}
}
