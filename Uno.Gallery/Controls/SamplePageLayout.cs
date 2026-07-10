using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Disposables;
using Uno.Extensions;
using Uno.Gallery.Helpers;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Uno.UI.Extensions;

namespace Uno.Gallery
{
	/// <summary>
	/// This control is used as a template for each sample page.
	/// </summary>
	public partial class SamplePageLayout : ContentControl
	{
		private const string VisualStateMaterial = nameof(Design.Material);
		private const string VisualStateFluent = nameof(Design.Fluent);
		private const string VisualStateCupertino = nameof(Design.Cupertino);
		private const string VisualStateNative = nameof(Design.Native);
		private const string VisualStateAgnostic = nameof(Design.Agnostic);

		private const string MaterialRadioButtonPartName = "PART_MaterialRadioButton";
		private const string FluentRadioButtonPartName = "PART_FluentRadioButton";
		private const string CupertinoRadioButtonPartName = "PART_CupertinoRadioButton";
		private const string NativeRadioButtonPartName = "PART_NativeRadioButton";
		private const string StickyMaterialRadioButtonPartName = "PART_StickyMaterialRadioButton";
		private const string StickyFluentRadioButtonPartName = "PART_StickyFluentRadioButton";
		private const string StickyCupertinoRadioButtonPartName = "PART_StickyCupertinoRadioButton";
		private const string StickyNativeRadioButtonPartName = "PART_StickyNativeRadioButton";
		private const string ScrollingTabsPartName = "PART_ScrollingTabs";
		private const string StickyTabsPartName = "PART_StickyTabs";
		private const string ScrollViewerPartName = "PART_ScrollViewer";
		private const string TopPartName = "PART_MobileTopBar";
		private const string ScrollingContentName = "ScrollingContent";
		private const string ShareHyperlinkPartName = "PART_ShareHyperlink";

		private static Design _design = Design.Material;

		private IReadOnlyCollection<LayoutModeMapping> LayoutModeMappings => new List<LayoutModeMapping>
		{
			new LayoutModeMapping(Design.Material, () => !IsDesignAgnostic, _materialRadioButton, _stickyMaterialRadioButton, VisualStateMaterial, MaterialTemplate),
			new LayoutModeMapping(Design.Fluent, () => !IsDesignAgnostic, _fluentRadioButton, _stickyFluentRadioButton, VisualStateFluent, FluentTemplate),
			// Cupertino is disabled as it is not up to date.
			// new LayoutModeMapping(Design.Cupertino, () => !IsDesignAgnostic, _cupertinoRadioButton, _stickyCupertinoRadioButton, VisualStateCupertino, CupertinoTemplate),
			new LayoutModeMapping(Design.Agnostic, () => IsDesignAgnostic, null, null, VisualStateAgnostic, DesignAgnosticTemplate),
#if __IOS__ || __MACOS__ || __ANDROID__ && !HAS_UNO_SKIA
			// native tab is only shown when applicable
			new LayoutModeMapping(Design.Native, () => !IsDesignAgnostic, _nativeRadioButton, _stickyNativeRadioButton, VisualStateNative, NativeTemplate),
#else
			// undefined template are not selectable and wont be selected by default
			new LayoutModeMapping(Design.Native, () => !IsDesignAgnostic, _nativeRadioButton, _stickyNativeRadioButton, VisualStateNative, default),
#endif
		};

		private RadioButton _materialRadioButton;
		private RadioButton _fluentRadioButton;
		private RadioButton _cupertinoRadioButton;
		private RadioButton _nativeRadioButton;
		private RadioButton _stickyMaterialRadioButton;
		private RadioButton _stickyFluentRadioButton;
		private RadioButton _stickyCupertinoRadioButton;
		private RadioButton _stickyNativeRadioButton;
		private FrameworkElement _scrollingTabs;
		private FrameworkElement _stickyTabs;
		private FrameworkElement _top;
		private ScrollViewer _scrollViewer;
		private FrameworkElement _scrollingContent;

		private readonly SerialDisposable _subscriptions = new SerialDisposable();

		public SamplePageLayout()
		{
			DataContextChanged += OnDataContextChanged;

			void OnDataContextChanged(object sender, DataContextChangedEventArgs args)
			{
				if (args.NewValue is Sample sample)
				{
					Title = sample.Title;
					Description = sample.Description;
					DocumentationLink = sample.DocumentationLink;
					Source = sample.Source;

#if __IOS__ || __ANDROID__
					IsFooterVisible = true;
					IsShareVisible = true;
#else
					IsFooterVisible = sample.DocumentationLink != null;
					IsShareVisible = false;
#endif
				}
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_materialRadioButton = (RadioButton)GetTemplateChild(MaterialRadioButtonPartName);
			_fluentRadioButton = (RadioButton)GetTemplateChild(FluentRadioButtonPartName);
			_cupertinoRadioButton = (RadioButton)GetTemplateChild(CupertinoRadioButtonPartName);
			_nativeRadioButton = (RadioButton)GetTemplateChild(NativeRadioButtonPartName);
			_stickyMaterialRadioButton = (RadioButton)GetTemplateChild(StickyMaterialRadioButtonPartName);
			_stickyFluentRadioButton = (RadioButton)GetTemplateChild(StickyFluentRadioButtonPartName);
			_stickyCupertinoRadioButton = (RadioButton)GetTemplateChild(StickyCupertinoRadioButtonPartName);
			_stickyNativeRadioButton = (RadioButton)GetTemplateChild(StickyNativeRadioButtonPartName);
			_scrollingTabs = (FrameworkElement)GetTemplateChild(ScrollingTabsPartName);
			_stickyTabs = (FrameworkElement)GetTemplateChild(StickyTabsPartName);
			_scrollViewer = (ScrollViewer)GetTemplateChild(ScrollViewerPartName);
			_top = (FrameworkElement)GetTemplateChild(TopPartName);
			_scrollingContent = (FrameworkElement)GetTemplateChild(ScrollingContentName);
			var shareHyperlink = (Hyperlink)GetTemplateChild(ShareHyperlinkPartName);

			// ensure previous subscriptions is removed before adding new ones, in case OnApplyTemplate is called multiple times
			var disposables = new CompositeDisposable();
			_subscriptions.Disposable = disposables;

			_scrollViewer.ViewChanged += OnScrolled;
			Disposable
				.Create(() => _scrollViewer.ViewChanged -= OnScrolled)
				.DisposeWith(disposables);

			if (shareHyperlink != null) // This feature is not available on all platforms.
			{
				shareHyperlink.Click += OnShareClicked;
				Disposable
					.Create(() => shareHyperlink.Click -= OnShareClicked)
					.DisposeWith(disposables);
			}

			BindOnClick(_materialRadioButton);
			BindOnClick(_fluentRadioButton);
			BindOnClick(_cupertinoRadioButton);
			BindOnClick(_nativeRadioButton);
			BindOnClick(_stickyMaterialRadioButton);
			BindOnClick(_stickyFluentRadioButton);
			BindOnClick(_stickyCupertinoRadioButton);
			BindOnClick(_stickyNativeRadioButton);

			UpdateLayoutRadioButtons();

			void BindOnClick(RadioButton radio)
			{
				radio.Click += OnLayoutRadioButtonChecked;
				Disposable
					.Create(() => radio.Click -= OnLayoutRadioButtonChecked)
					.DisposeWith(disposables);
			}

			void OnScrolled(object sender, ScrollViewerViewChangedEventArgs e)
			{
				var relativeOffset = GetRelativeOffset();
				if (relativeOffset < 0)
				{
					_stickyTabs.Visibility = Visibility.Visible;
				}
				else
				{
					_stickyTabs.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void OnShareClicked(Hyperlink sender, HyperlinkClickEventArgs args)
		{
#if (__IOS__ || __ANDROID__) && !NET6_0_OR_GREATER
			var sample = DataContext as Sample;
			_ = Deeplinking.BranchService.Instance.ShareSample(sample, _design);
#endif
		}

		/// <summary>
		/// Changes the preferred design.
		/// This doesn't change the current UI. It only affects the next created sample.
		/// </summary>
		/// <param name="design">The desired design.</param>
		public static void SetPreferredDesign(Design design)
		{
			_design = design;
		}

		private void RegisterEvent(RoutedEventHandler click)
		{
			click += OnLayoutRadioButtonChecked;
		}

		private void UpdateLayoutRadioButtons()
		{
			var mappings = LayoutModeMappings;
			var previouslySelected = default(LayoutModeMapping);

			bool IsAvailable(LayoutModeMapping mapping) => mapping.Predicate() && mapping.Template != null;

			foreach (var mapping in mappings)
			{
				var available = IsAvailable(mapping);
				var visibility = available ? Visibility.Visible : Visibility.Collapsed;
				mapping.RadioButton?.Apply(x => x.Visibility = visibility);
				mapping.StickyRadioButton?.Apply(x => x.Visibility = visibility);

				if (mapping.Design == _design && available)
				{
					previouslySelected = mapping;
				}
			}

			// selected mode is based on previous selection and availability (whether the template is defined)
			var selected = previouslySelected ?? mappings.FirstOrDefault(x => x.Template != null);
			if (selected != null)
			{
				UpdateLayoutMode(transitionTo: selected.Design);
			}
		}

		private void OnLayoutRadioButtonChecked(object sender, RoutedEventArgs e)
		{
			if (sender is RadioButton radio && LayoutModeMappings.FirstOrDefault(x => x.RadioButton == radio || x.StickyRadioButton == radio) is LayoutModeMapping mapping)
			{
				_design = mapping.Design;
				UpdateLayoutMode();
			}
		}

		private void UpdateLayoutMode(Design? transitionTo = null)
		{
			var design = transitionTo ?? _design;

			var current = LayoutModeMappings.FirstOrDefault(x => x.Design == design);
			if (current != null)
			{
				current.RadioButton?.Apply(x => x.IsChecked = true);
				current.StickyRadioButton?.Apply(x => x.IsChecked = true);

				VisualStateManager.GoToState(this, current.VisualStateName, useTransitions: true);
			}
		}

		private double GetRelativeOffset()
		{
#if WINDOWS
			// On UWP we can count on finding a ScrollContentPresenter.
			var scp = VisualTreeHelperEx.GetFirstDescendant<ScrollContentPresenter>(_scrollViewer);
			var content = scp?.Content as FrameworkElement;
			var transform = _scrollingTabs.TransformToVisual(content);
			return transform.TransformPoint(new Point(0, 0)).Y - _scrollViewer.VerticalOffset;
#elif __IOS__
			var transform = _scrollingTabs.TransformToVisual(_scrollViewer);
			return transform.TransformPoint(new Point(0, 0)).Y;
#else
			var transform = _scrollingTabs.TransformToVisual(_scrollingContent);
			return transform.TransformPoint(new Point(0, 0)).Y - _scrollViewer.VerticalOffset;
#endif
		}

		/// <summary>
		/// Get control inside the specified layout template.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mode">The layout mode in which the control is defined</param>
		/// <param name="name">The 'x:Name' of the control</param>
		/// <returns></returns>
		/// <remarks>The caller must ensure the control is loaded. This is best done from <see cref="FrameworkElement.Loaded"/> event.</remarks>
		public T GetSampleChild<T>(Design mode, string name)
			where T : FrameworkElement
		{
			var presenter = (ContentPresenter)GetTemplateChild($"{mode}ContentPresenter");

			return VisualTreeHelperEx.GetFirstDescendant<T>(presenter, x => x.Name == name);
		}

		private class LayoutModeMapping
		{
			public Design Design { get; set; }
			public Func<bool> Predicate { get; set; }
			public RadioButton RadioButton { get; set; }
			public RadioButton StickyRadioButton { get; set; }
			public string VisualStateName { get; set; }
			public DataTemplate Template { get; set; }

			public LayoutModeMapping(
				Design design,
				Func<bool> predicate,
				RadioButton radioButton,
				RadioButton stickyRadioButton,
				string visualStateName,
				DataTemplate template)
			{
				Design = design;
				Predicate = predicate;
				RadioButton = radioButton;
				StickyRadioButton = stickyRadioButton;
				VisualStateName = visualStateName;
				Template = template;
			}
		}
	}
}
