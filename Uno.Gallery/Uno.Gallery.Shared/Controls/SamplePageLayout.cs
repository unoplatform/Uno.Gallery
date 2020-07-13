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
		private const string VisualStateFluent = "Fluent";
		private const string VisualStateMaterial = "Material";
		private const string VisualStateNative = "Native";

		private static SamplePageLayoutMode _mode = SamplePageLayoutMode.Fluent;

		#region Dependency Properties

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

		public DataTemplate FluentTemplate
		{
			get { return (DataTemplate)GetValue(FluentTemplateProperty); }
			set { SetValue(FluentTemplateProperty, value); }
		}

		public static readonly DependencyProperty FluentTemplateProperty =
			DependencyProperty.Register("FluentTemplate", typeof(DataTemplate), typeof(SamplePageLayout), new PropertyMetadata(null));

		#endregion

		private RadioButton _fluentRadioButton;
		private RadioButton _materialRadioButton;
		private RadioButton _nativeRadioButton;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_fluentRadioButton = (RadioButton)GetTemplateChild("PART_FluentRadioButton");
			_materialRadioButton = (RadioButton)GetTemplateChild("PART_MaterialRadioButton");
			_nativeRadioButton = (RadioButton)GetTemplateChild("PART_NativeRadioButton");

			// TODO #189081 : Fix crashing on event registered on wasm
#if !__WASM__
			_fluentRadioButton.Click -= OnFluentRadioButtonChecked;
			_fluentRadioButton.Click += OnFluentRadioButtonChecked;

			_materialRadioButton.Click -= OnMaterialRadioButtonChecked;
			_materialRadioButton.Click += OnMaterialRadioButtonChecked;

			_nativeRadioButton.Click -= OnNativeRadioButtonChecked;
			_nativeRadioButton.Click += OnNativeRadioButtonChecked;
#endif

			SetCurrentRadioButton();
			UpdateVisualState();
		}

		private void SetCurrentRadioButton()
		{
			switch (_mode)
			{
				case SamplePageLayoutMode.Material:
					_materialRadioButton.IsChecked = true;
					break;
				case SamplePageLayoutMode.Native:
					_nativeRadioButton.IsChecked = true;
					break;
				case SamplePageLayoutMode.Fluent:
				default:
					_fluentRadioButton.IsChecked = true;
					break;
			}
		}

		private void OnNativeRadioButtonChecked(object sender, RoutedEventArgs e)
		{
			var selectedRadioButton = (RadioButton)sender;
			ChangeRadioButtonSelection(selectedRadioButton, SamplePageLayoutMode.Native);
			UpdateVisualState();
		}

		private void OnMaterialRadioButtonChecked(object sender, RoutedEventArgs e)
		{
			var selectedRadioButton = (RadioButton)sender;
			ChangeRadioButtonSelection(selectedRadioButton, SamplePageLayoutMode.Material);
			UpdateVisualState();
		}

		private void OnFluentRadioButtonChecked(object sender, RoutedEventArgs e)
		{
			var selectedRadioButton = (RadioButton)sender;
			ChangeRadioButtonSelection(selectedRadioButton, SamplePageLayoutMode.Fluent);
			UpdateVisualState();
		}

		private void ChangeRadioButtonSelection(RadioButton selectedRadioButton, SamplePageLayoutMode mode)
		{
			selectedRadioButton.IsChecked = true;
			_mode = mode;
		}

		private void UpdateVisualState()
		{
			switch (_mode)
			{
				case SamplePageLayoutMode.Material:
					VisualStateManager.GoToState(this, VisualStateMaterial, useTransitions: true);
					break;
				case SamplePageLayoutMode.Native:
					VisualStateManager.GoToState(this, VisualStateNative, useTransitions: true);
					break;
				case SamplePageLayoutMode.Fluent:
				default:
					VisualStateManager.GoToState(this, VisualStateFluent, useTransitions: true);
					break;
			}
		}
	}

	public enum SamplePageLayoutMode
	{
		Fluent,
		Material,
		Native
	}
}
