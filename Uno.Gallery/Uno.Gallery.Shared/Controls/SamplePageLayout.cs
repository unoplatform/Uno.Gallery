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
		private const string VisualStateMaterial = nameof(SamplePageLayoutMode.Material);
		private const string VisualStateFluent = nameof(SamplePageLayoutMode.Fluent);
		private const string VisualStateNative = nameof(SamplePageLayoutMode.Native);

		private const string MaterialRadioButtonPartName = "PART_MaterialRadioButton";
		private const string FluentRadioButtonPartName = "PART_FluentRadioButton";
		private const string NativeRadioButtonPartName = "PART_NativeRadioButton";

		private static SamplePageLayoutMode _mode = SamplePageLayoutMode.Material;

		private IReadOnlyCollection<LayoutModeMapping> LayoutModeMappings => new List<LayoutModeMapping>
		{
			new LayoutModeMapping(SamplePageLayoutMode.Material, _materialRadioButton, VisualStateMaterial, MaterialTemplate),
			new LayoutModeMapping(SamplePageLayoutMode.Fluent, _fluentRadioButton, VisualStateFluent, FluentTemplate),
			new LayoutModeMapping(SamplePageLayoutMode.Native, _nativeRadioButton, VisualStateNative, NativeTemplate),
		};
		private RadioButton _materialRadioButton;
		private RadioButton _fluentRadioButton;
		private RadioButton _nativeRadioButton;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_materialRadioButton = (RadioButton)GetTemplateChild(MaterialRadioButtonPartName);
			_fluentRadioButton = (RadioButton)GetTemplateChild(FluentRadioButtonPartName);
			_nativeRadioButton = (RadioButton)GetTemplateChild(NativeRadioButtonPartName);

			_materialRadioButton.Click += OnLayoutRadioButtonChecked;
			_fluentRadioButton.Click += OnLayoutRadioButtonChecked;
			_nativeRadioButton.Click += OnLayoutRadioButtonChecked;

			UpdateLayoutRadioButtons();
		}

		private void UpdateLayoutRadioButtons()
		{
			var mappings = LayoutModeMappings;
			var previouslySelected = default(LayoutModeMapping);

			foreach (var mapping in mappings)
			{
				mapping.RadioButton.Visibility = mapping.Template != null ? Visibility.Visible : Visibility.Collapsed;
				if (mapping.Template != null && mapping.Mode == _mode)
				{
					previouslySelected = mapping;
				}
			}

			// selected mode is based on previous selection and availability (whether the template is defined)
			var selected = previouslySelected ?? mappings.FirstOrDefault(x => x.Template != null);
			if (selected != null)
			{
				UpdateLayoutMode(transitionTo: selected.Mode);
			}
		}

		private void OnLayoutRadioButtonChecked(object sender, RoutedEventArgs e)
		{
			if (sender is RadioButton radio && LayoutModeMappings.FirstOrDefault(x => x.RadioButton == radio) is LayoutModeMapping mapping)
			{
				_mode = mapping.Mode;
				UpdateLayoutMode();
			}
		}

		private void UpdateLayoutMode(SamplePageLayoutMode? transitionTo = null)
		{
			var mode = transitionTo ?? _mode;

			var current = LayoutModeMappings.FirstOrDefault(x => x.Mode == mode);
			if (current != null)
			{
				if (transitionTo.HasValue)
				{
					current.RadioButton.IsChecked = true;
				}

				VisualStateManager.GoToState(this, current.VisualStateName, useTransitions: true);
			}
		}

		private class LayoutModeMapping
		{
			public SamplePageLayoutMode Mode { get; set; }
			public RadioButton RadioButton { get; set; }
			public string VisualStateName { get; set; }
			public DataTemplate Template { get; set; }

			public LayoutModeMapping(SamplePageLayoutMode mode, RadioButton radioButton, string visualStateName, DataTemplate template)
			{
				Mode = mode;
				RadioButton = radioButton;
				VisualStateName = visualStateName;
				Template = template;
			}
		}
	}
}
