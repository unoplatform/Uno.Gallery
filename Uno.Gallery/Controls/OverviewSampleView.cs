using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

namespace Uno.Gallery
{
	[TemplatePart(Name = ViewButtonPartName, Type = typeof(Button))]
	public partial class OverviewSampleView : ContentControl
	{
		private const string ViewButtonPartName = "PART_ViewButton";

		private Button? _viewButton;

		public Type SamplePageType
		{
			get { return (Type)GetValue(SamplePageTypeProperty); }
			set { SetValue(SamplePageTypeProperty, value); }
		}

		public static readonly DependencyProperty SamplePageTypeProperty =
			DependencyProperty.Register("SamplePageType", typeof(Type), typeof(OverviewSampleView), new PropertyMetadata(null, OnSamplePageTypeChanged));

		private static void OnSamplePageTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is Type type)
			{
				var that = (OverviewSampleView)d;
				that.Sample = new Sample(type.GetTypeInfo().GetCustomAttribute<SamplePageAttribute>(), type);
				that.UpdateViewButtonAutomationId();
			}
		}

		public Sample Sample
		{
			get { return (Sample)GetValue(SampleProperty); }
			set { SetValue(SampleProperty, value); }
		}

		public static readonly DependencyProperty SampleProperty =
			DependencyProperty.Register("Sample", typeof(Sample), typeof(OverviewSampleView), new PropertyMetadata(null));

		public Design SampleDesign
		{
			get => (Design)GetValue(SampleDesignProperty);
			set => SetValue(SampleDesignProperty, value);
		}

		public static readonly DependencyProperty SampleDesignProperty =
			DependencyProperty.Register(nameof(SampleDesign), typeof(Design), typeof(OverviewSampleView), new PropertyMetadata(Design.Material, OnSampleDesignChanged));

		private static void OnSampleDesignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> ((OverviewSampleView)d).UpdateViewButtonAutomationId();

		public string ViewButtonAutomationId
		{
			get => (string)GetValue(ViewButtonAutomationIdProperty);
			private set => SetValue(ViewButtonAutomationIdProperty, value);
		}

		public static readonly DependencyProperty ViewButtonAutomationIdProperty =
			DependencyProperty.Register(nameof(ViewButtonAutomationId), typeof(string), typeof(OverviewSampleView), new PropertyMetadata(null));

		private void UpdateViewButtonAutomationId()
		{
			if (SamplePageType is null) return;
			ViewButtonAutomationId = $"ViewButton_{SamplePageType.Name}_{SampleDesign}";
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_viewButton is not null)
				_viewButton.Click -= OnViewClicked;

			_viewButton = GetTemplateChild(ViewButtonPartName) as Button;

			if (_viewButton is not null)
				_viewButton.Click += OnViewClicked;
		}

		private void OnViewClicked(object sender, RoutedEventArgs e)
		{
			var shell = VisualTreeHelperEx.FindAncestor<Shell>(this)
				?? throw new InvalidOperationException(
					"Cannot find Shell ancestor; cannot navigate from OverviewSampleView. " +
					"OverviewSampleView must be placed inside a Shell in the visual tree.");
			var nav = shell.Navigator
				?? throw new InvalidOperationException(
					"Shell.Navigator is not set; cannot navigate from OverviewSampleView. " +
					"Navigator must be assigned before the visual tree is entered.");
			SamplePageLayout.SetPreferredDesign(SampleDesign);
			nav.NavigateTo(Sample);
		}
	}
}
