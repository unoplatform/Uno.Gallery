﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery
{
	public partial class SamplePageLayout
	{
		#region Property: Title

		public static DependencyProperty TitleProperty { get; } = DependencyProperty.Register(
			nameof(Title),
			typeof(string),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		#endregion
		#region Property: Description

		public static DependencyProperty DescriptionProperty { get; } = DependencyProperty.Register(
			nameof(Description),
			typeof(string),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public string Description
		{
			get => (string)GetValue(DescriptionProperty);
			set => SetValue(DescriptionProperty, value);
		}

		#endregion
		#region Property: DocumentationLink

		public static DependencyProperty DocumentationLinkProperty { get; } = DependencyProperty.Register(
			nameof(DocumentationLink),
			typeof(Uri),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public Uri DocumentationLink
		{
			get => (Uri)GetValue(DocumentationLinkProperty);
			set => SetValue(DocumentationLinkProperty, value);
		}

		#endregion
		#region Property: Source

		public static DependencyProperty SourceProperty { get; } = DependencyProperty.Register(
			nameof(Source),
			typeof(SourceSdk?),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public SourceSdk? Source
		{
			get => (SourceSdk?)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		#endregion
		#region Property: IsDesignAgnostic

		public static DependencyProperty IsDesignAgnosticProperty { get; } = DependencyProperty.Register(
			nameof(IsDesignAgnostic),
			typeof(bool),
			typeof(SamplePageLayout),
			new PropertyMetadata(default(bool)));

		public bool IsDesignAgnostic
		{
			get => (bool)GetValue(IsDesignAgnosticProperty);
			set => SetValue(IsDesignAgnosticProperty, value);
		}

		#endregion

		#region Property: FluentTemplate

		public static DependencyProperty FluentTemplateProperty { get; } = DependencyProperty.Register(
			nameof(FluentTemplate),
			typeof(DataTemplate),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public DataTemplate FluentTemplate
		{
			get => (DataTemplate)GetValue(FluentTemplateProperty);
			set => SetValue(FluentTemplateProperty, value);
		}

		#endregion
		#region Property: MaterialTemplate

		public static DependencyProperty MaterialTemplateProperty { get; } = DependencyProperty.Register(
			nameof(MaterialTemplate),
			typeof(DataTemplate),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public DataTemplate MaterialTemplate
		{
			get => (DataTemplate)GetValue(MaterialTemplateProperty);
			set => SetValue(MaterialTemplateProperty, value);
		}

		#endregion
		#region Property: CupertinoTemplate

		public static DependencyProperty CupertinoTemplateProperty { get; } = DependencyProperty.Register(
			nameof(CupertinoTemplate),
			typeof(DataTemplate),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public DataTemplate CupertinoTemplate
		{
			get => (DataTemplate)GetValue(CupertinoTemplateProperty);
			set => SetValue(CupertinoTemplateProperty, value);
		}

		#endregion
		#region Property: NativeTemplate

		public static DependencyProperty NativeTemplateProperty { get; } = DependencyProperty.Register(
			nameof(NativeTemplate),
			typeof(DataTemplate),
			typeof(SamplePageLayout),
			new PropertyMetadata(default));

		public DataTemplate NativeTemplate
		{
			get => (DataTemplate)GetValue(NativeTemplateProperty);
			set => SetValue(NativeTemplateProperty, value);
		}

		#endregion
		#region Property: DesignAgnosticTemplate

		public static DependencyProperty DesignAgnosticTemplateProperty { get; } = DependencyProperty.Register(
			nameof(DesignAgnosticTemplate),
			typeof(DataTemplate),
			typeof(SamplePageLayout),
			new PropertyMetadata(default(DataTemplate)));

		public DataTemplate DesignAgnosticTemplate
		{
			get => (DataTemplate)GetValue(DesignAgnosticTemplateProperty);
			set => SetValue(DesignAgnosticTemplateProperty, value);
		}

		#endregion

		#region Property: HeaderTemplate
		/// <summary>
		/// The Header is the part above the design tabs (Material|Fluent|Native).
		/// It contains the Description and the Source in the default style.
		/// </summary>
		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}

		public static readonly DependencyProperty HeaderTemplateProperty =
			DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(SamplePageLayout), new PropertyMetadata(null));
		#endregion

		#region Property: IsFooterVisible
		public bool IsFooterVisible
		{
			get { return (bool)GetValue(IsFooterVisibleProperty); }
			set { SetValue(IsFooterVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsFooterVisibleProperty =
			DependencyProperty.Register("IsFooterVisible", typeof(bool), typeof(SamplePageLayout), new PropertyMetadata(false));
		#endregion

		#region IsShareVisible
		public bool IsShareVisible
		{
			get { return (bool)GetValue(IsShareVisibleProperty); }
			set { SetValue(IsShareVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsShareVisibleProperty =
			DependencyProperty.Register("IsShareVisible", typeof(bool), typeof(SamplePageLayout), new PropertyMetadata(false));
		#endregion
	}
}
