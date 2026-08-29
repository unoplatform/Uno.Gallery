using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.ViewModels;
using Uno.Themes;

namespace Uno.Gallery.Views.Samples
{
	/// <summary>
	/// Design Tokens Reference — density, spacing, shape/corner-radius, and typography
	/// using stable Uno.Themes 7.0.3 APIs.
	///
	/// Construction-time vs runtime behavior:
	/// • Density (<see cref="Density"/> enum), spacing (Space*), and shape (Radius*)
	///   tokens are set at application startup via <c>MaterialToolkitTheme.DefaultDensity</c>
	///   and <c>MaterialToolkitTheme.DefaultCornerRadius</c>. They cannot be mutated at
	///   runtime without reconstructing the theme resources.
	/// • Token values are read from <c>Application.Current.Resources</c> in the ViewModel
	///   constructor and displayed statically — they represent construction-time values.
	/// • Typography styles (DisplayLarge, HeadlineMedium, …) are StaticResources applied
	///   directly in XAML — they are never dynamic and do not change after load.
	/// </summary>
	[SamplePage(
		SampleCategory.Theming,
		"Design Tokens",
		Description = "Reference for Material density, spacing, shape/corner-radius, and typography tokens from Uno.Themes 7.0.3.",
		DocumentationLink = "https://m3.material.io/foundations/design-tokens/overview",
		DataType = typeof(DesignTokensSamplePageViewModel),
		Tags = new[] { "material", "tokens", "spacing", "shape", "density", "typography" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Material,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "The active Material theme must be initialized; no network access is required." },
		AccessibilityNotes = new[] { "Every token row includes a text name and resolved value; color and shape previews are supplementary." },
		ResetBehavior = "This reference is read-only and reflects the theme configured at application startup.",
		Variants = new[] { "Density presets", "Spacing tokens", "Shape tokens", "Typography scale" },
		KnownLimitations = new[] { "Construction-time density and corner-radius settings cannot be changed by this page." },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-27",
		SortOrder = 11,
		RelatedSamples = new[] { "material-seed-color" })]
	public sealed partial class DesignTokensSamplePage : Page
	{
		public DesignTokensSamplePage()
		{
			this.InitializeComponent();
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public sealed class DesignTokensSamplePageViewModel : ViewModelBase
	{
		public string ActiveDensityLabel { get; }
		public string ActiveBaseUnitLabel { get; }
		public IReadOnlyList<TokenRow> SpacingTokens { get; }
		public IReadOnlyList<TokenRow> ShapeTokens { get; }

		public DesignTokensSamplePageViewModel()
		{
			// Read active density from theme — BaseTheme.DefaultDensity is public.
			var theme = SemanticThemeHelper.GetTheme();
			var density = theme?.DefaultDensity ?? Density.Regular;
			var baseUnit = density switch
			{
				Density.Compact => 3,
				Density.Comfy => 5,
				_ => 4,
			};

			ActiveDensityLabel = density.ToString();
			ActiveBaseUnitLabel = $"(base unit = {baseUnit} px)";

			SpacingTokens = BuildSpacingTokens();
			ShapeTokens = BuildShapeTokens();
		}

		private static IReadOnlyList<TokenRow> BuildSpacingTokens()
		{
			var resources = Application.Current.Resources;
			var result = new List<TokenRow>();
			foreach (var name in SpacingTokenNames)
			{
				double value = 0;
				if (resources.TryGetValue(name, out var raw))
					value = raw is double d ? d : raw is float f ? (double)f : 0;

				var valueText = value > 0 ? $"{value:0.#} px" : "—";
				var barWidth = value > 0 ? System.Math.Min(value * 2.5, 160.0) : 0;
				result.Add(new TokenRow(name, $"Token_Spacing_{name}", valueText, barWidth, default));
			}

			return result;
		}

		private static IReadOnlyList<TokenRow> BuildShapeTokens()
		{
			var resources = Application.Current.Resources;
			var result = new List<TokenRow>();
			foreach (var name in ShapeTokenNames)
			{
				double value = 0;
				if (resources.TryGetValue(name, out var raw))
					value = raw is Microsoft.UI.Xaml.CornerRadius cr ? cr.TopLeft
						  : raw is double d ? d
						  : raw is float f ? (double)f
						  : 0;

				var valueText = value >= 9999 ? "∞ (pill)" : value > 0 ? $"{value:0.#} px" : "—";
				var previewRadius = value >= 9999 ? 12.0 : System.Math.Min(value, 12.0);
				result.Add(new TokenRow(name, $"Token_Shape_{name}", valueText, 0,
					new Microsoft.UI.Xaml.CornerRadius(previewRadius)));
			}

			return result;
		}

		// ─── Token name tables ─────────────────────────────────────────────────
		// Keys verified against Uno.Themes 7.0.3 BaseTheme.ScaleGeneration.cs.
		// Space* values are doubles; Radius*CornerRadius values are CornerRadius structs.

		private static readonly string[] SpacingTokenNames =
		{
			"Space050", "Space100", "Space200", "Space300",
			"Space400", "Space500", "Space600", "Space800",
		};

		private static readonly string[] ShapeTokenNames =
		{
			"Radius050CornerRadius", "Radius100CornerRadius", "Radius300CornerRadius",
			"Radius400CornerRadius", "Radius500CornerRadius", "RadiusFullCornerRadius",
		};
	}

	// ─── Display model ─────────────────────────────────────────────────────────

	[Microsoft.UI.Xaml.Data.Bindable]
	public sealed class TokenRow
	{
		public string Name { get; }
		public string AutomationId { get; }
		public string ValueText { get; }
		public double BarWidth { get; }
		public Microsoft.UI.Xaml.CornerRadius CornerRadiusPreview { get; }

		// Used by UITests to assert the resolved value displayed in the Value column.
		public string ValueAutomationId => AutomationId + "_Value";

		public TokenRow(string name, string automationId, string valueText,
			double barWidth, Microsoft.UI.Xaml.CornerRadius cornerRadiusPreview)
		{
			Name = name;
			AutomationId = automationId;
			ValueText = valueText;
			BarWidth = barWidth;
			CornerRadiusPreview = cornerRadiusPreview;
		}
	}
}
