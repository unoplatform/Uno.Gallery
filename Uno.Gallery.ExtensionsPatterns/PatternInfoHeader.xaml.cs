using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.ExtensionsPatterns;

public sealed partial class PatternInfoHeader : UserControl
{
	public static readonly DependencyProperty PackageNameProperty = DependencyProperty.Register(
		nameof(PackageName), typeof(string), typeof(PatternInfoHeader), new PropertyMetadata(""));
	public static readonly DependencyProperty RequirementsProperty = DependencyProperty.Register(
		nameof(Requirements), typeof(string), typeof(PatternInfoHeader), new PropertyMetadata(""));
	public static readonly DependencyProperty AccessibilityStatusProperty = DependencyProperty.Register(
		nameof(AccessibilityStatus), typeof(string), typeof(PatternInfoHeader), new PropertyMetadata(""));
	public static readonly DependencyProperty DocumentationUriProperty = DependencyProperty.Register(
		nameof(DocumentationUri), typeof(Uri), typeof(PatternInfoHeader), new PropertyMetadata(null));
	public static readonly DependencyProperty SourceUriProperty = DependencyProperty.Register(
		nameof(SourceUri), typeof(Uri), typeof(PatternInfoHeader), new PropertyMetadata(null));

	public PatternInfoHeader() => InitializeComponent();

	public string PackageName
	{
		get => (string)GetValue(PackageNameProperty);
		set => SetValue(PackageNameProperty, value);
	}

	public string Requirements
	{
		get => (string)GetValue(RequirementsProperty);
		set => SetValue(RequirementsProperty, value);
	}

	public string AccessibilityStatus
	{
		get => (string)GetValue(AccessibilityStatusProperty);
		set => SetValue(AccessibilityStatusProperty, value);
	}

	public Uri DocumentationUri
	{
		get => (Uri)GetValue(DocumentationUriProperty);
		set => SetValue(DocumentationUriProperty, value);
	}

	public Uri SourceUri
	{
		get => (Uri)GetValue(SourceUriProperty);
		set => SetValue(SourceUriProperty, value);
	}
}
