using System;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.Accessibility, "Localization and RTL",
	Description = "Resource-based strings, pseudo-localization, long text, bidirectional content, mirrored layout, and state preservation.",
	DocumentationLink = "https://platform.uno/docs/articles/features/localization.html",
	Slug = "localization-rtl",
	Tags = new[] { "localization", "rtl", "bidi", "pseudo-localization", "resources" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
	AccessibilityNotes = new[] { "Direction and pseudo-localization toggles are named, text remains readable in bidi layouts, and state is preserved while direction changes." },
	ResetBehavior = SampleContractDefaults.ReloadToReset,
	Variants = new[] { "Left-to-right and right-to-left layout", "Mixed English, Arabic, and Hebrew text", "Long-string expansion", "Pseudo-localization preview" },
	Owner = "unoplatform",
	ReviewedOn = "2026-08-26",
	RelatedSamples = new[] { "accessibility" })]
public sealed partial class LocalizationSamplePage : Page
{
	private const string LatinCharacters = "abcdefghijklmnopqrstuvwxyz";
	private const string PseudoCharacters = "áƀçðëƒğħïĵķľɱñöþʠřšŧüṽŵẋÿž";
	private bool _pseudoEnabled;

	public LocalizationSamplePage()
	{
		InitializeComponent();
	}

	private void LocalizedPreview_Loaded(object sender, RoutedEventArgs e)
	{
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "DirectionStatus")
			?? throw new InvalidOperationException("The localization direction status is not loaded.");
		SetDirectionStatus(status, ((FrameworkElement)sender).FlowDirection);
	}

	private void ToggleRtl_Click(object sender, RoutedEventArgs e)
	{
		var preview = SamplePageLayoutRoot.GetSampleChild<StackPanel>(Design.Agnostic, "LocalizedPreview")
			?? throw new InvalidOperationException("The localization preview is not loaded.");
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "DirectionStatus")
			?? throw new InvalidOperationException("The localization direction status is not loaded.");

		preview.FlowDirection = preview.FlowDirection == FlowDirection.LeftToRight
			? FlowDirection.RightToLeft
			: FlowDirection.LeftToRight;
		SetDirectionStatus(status, preview.FlowDirection);
	}

	private void TogglePseudo_Click(object sender, RoutedEventArgs e)
	{
		var resolved = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "ResolvedResourceText")
			?? throw new InvalidOperationException("The localized resource preview is not loaded.");
		var preview = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "PseudoPreviewText")
			?? throw new InvalidOperationException("The pseudo-localization preview is not loaded.");

		_pseudoEnabled = !_pseudoEnabled;
		preview.Text = _pseudoEnabled
			? resolved.Text.StartsWith("[!! ", StringComparison.Ordinal)
				? resolved.Text
				: PseudoLocalize(resolved.Text)
			: LocalizationHelper.GetString(
				"LocalizationPseudoPreviewOff",
				"Pseudo-localization preview is off.");
	}

	private static void SetDirectionStatus(TextBlock status, FlowDirection direction)
		=> status.Text = string.Format(
			CultureInfo.CurrentCulture,
			LocalizationHelper.GetString("LocalizationDirectionStatusFormat", "Direction: {0}"),
			direction);

	private static string PseudoLocalize(string value)
	{
		var builder = new StringBuilder(value.Length);
		foreach (var character in value)
		{
			var index = LatinCharacters.IndexOf(char.ToLowerInvariant(character));
			if (index < 0)
			{
				builder.Append(character);
				continue;
			}

			var replacement = PseudoCharacters[index];
			builder.Append(char.IsUpper(character) ? char.ToUpperInvariant(replacement) : replacement);
		}
		var paddingLength = Math.Max(3, (int)Math.Ceiling(value.Length * 0.3));
		return $"[!! {builder} {new string('~', paddingLength)} !!]";
	}
}
