﻿using System;
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
			}
		}

		public Sample Sample
		{
			get { return (Sample)GetValue(SampleProperty); }
			set { SetValue(SampleProperty, value); }
		}

		public static readonly DependencyProperty SampleProperty =
			DependencyProperty.Register("Sample", typeof(Sample), typeof(OverviewSampleView), new PropertyMetadata(null));

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			var viewButton = (Button)GetTemplateChild(ViewButtonPartName);
			viewButton.Click -= OnViewClicked;
			viewButton.Click += OnViewClicked;

			void OnViewClicked(object sender, RoutedEventArgs e)
			{
				var shell = VisualTreeHelperEx.FindAncestor<Shell>(this);
				(Application.Current as App)?.ShellNavigateTo(shell, Sample);
			}
		}
	}
}
