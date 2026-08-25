using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Uno.Gallery.SourceGenerators;

[Generator]
public sealed class SamplesGenerator : IIncrementalGenerator
{
	// ─── Diagnostics ──────────────────────────────────────────────────────────
	// UGG0001  Error    Unexpected SamplePageAttribute constructor shape (param count or names)
	// UGG0002  Error    SamplePageAttribute applied to a non-named-type target
	// UGG0003  Error    Unexpected SampleConditionalAttribute constructor shape
	// UGG0004  Warning  Duplicate sample title found in the generated catalog

	private static class Diagnostics
	{
		public static readonly DiagnosticDescriptor UnexpectedAttributeShape = new(
			id: "UGG0001",
			title: "Unexpected SamplePageAttribute constructor shape",
			messageFormat: "SamplePageAttribute on '{0}' has an unexpected constructor shape (got {1}); " +
						   "expected parameters (category, title, source, glyph). Code generation skipped.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "The generator requires SamplePageAttribute to have exactly four constructor parameters " +
						 "named category, title, source, and glyph in that order.  " +
						 "Rename the parameters or fix the attribute declaration.");

		public static readonly DiagnosticDescriptor TargetNotNamedType = new(
			id: "UGG0002",
			title: "SamplePageAttribute applied to non-class target",
			messageFormat: "SamplePageAttribute can only generate code for named class types; " +
						   "'{0}' is not a valid target. Code generation skipped.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Apply SamplePageAttribute only to class declarations.");

		public static readonly DiagnosticDescriptor UnexpectedConditionalShape = new(
			id: "UGG0003",
			title: "Unexpected SampleConditionalAttribute constructor shape",
			messageFormat: "SampleConditionalAttribute on '{0}' must have exactly one constructor argument " +
						   "of type SampleConditionals. Code generation for this sample is skipped.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "The generator reads the first constructor argument of SampleConditionalAttribute as the " +
						 "SampleConditionals flags value.  Ensure the attribute has exactly one such argument.");

		public static readonly DiagnosticDescriptor DuplicateSampleTitle = new(
			id: "UGG0004",
			title: "Duplicate sample title in generated catalog",
			messageFormat: "Sample title '{0}' is also used by '{1}'. Give one of them a distinct title to avoid navigation ambiguity.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Each sample title should be unique across the catalog.  Rename the later duplicate so navigation and search remain unambiguous.");
	}

	// ─── Pipeline models ──────────────────────────────────────────────────────

	private record struct SamplesModel(
		SampleConditionals? Conditionals,
		string FullyQualifiedName,
		string Category,
		string Title,
		string? Description,
		string? DocumentationLink,
		string? DataType,
		string SourceSdk,
		string Glyph,
		int SortOrder,
		// Stores the real SyntaxNode location so UGG0004 points into the actual source tree
		// and #pragma warning disable UGG0004 works at the declaration site.
		// Location equality in Roslyn is (SyntaxTree reference, TextSpan): two Location
		// values are equal iff they share the same SyntaxTree object and span.  Roslyn
		// reuses SyntaxTree objects for unchanged sources between incremental pipeline runs,
		// so this reference-equality is stable for caching purposes.
		Location DeclarationLocation);

	/// <summary>
	/// Lightweight, value-comparable diagnostic carrier used inside the incremental pipeline.
	/// <see cref="DiagnosticDescriptor"/> is a static singleton so reference equality is correct.
	/// <see cref="Location"/> equality in Roslyn is SyntaxTree-reference plus TextSpan: two
	/// <see cref="Location"/> objects compare equal only when they share the same (reference-identical)
	/// <see cref="SyntaxTree"/> and an identical span — content-structural equality is not guaranteed
	/// across incremental steps that rebuild the tree.  These diagnostics are used only in the error
	/// branch, which does not participate in cross-step value caching that would require stable equality.
	/// Declared as <c>record struct</c> (not <c>readonly record struct</c>) so it compiles on
	/// netstandard2.0 without an IsExternalInit polyfill.
	/// </summary>
	private record struct DiagnosticInfo(
		DiagnosticDescriptor Descriptor,
		Location Location,
		string MessageArg0,
		string? MessageArg1 = null)
	{
		public Diagnostic ToDiagnostic() =>
			MessageArg1 is null
				? Diagnostic.Create(Descriptor, Location, MessageArg0)
				: Diagnostic.Create(Descriptor, Location, MessageArg0, MessageArg1);
	}

	private record struct TransformResult(SamplesModel? Model, DiagnosticInfo? Error)
	{
		public static TransformResult Ok(SamplesModel model) => new(model, null);
		public static TransformResult Fail(DiagnosticInfo error) => new(null, error);
	}

	// ─── IIncrementalGenerator ────────────────────────────────────────────────

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var currentPlatformProvider = context.ParseOptionsProvider.Select(
			(options, _) => GetSampleConditionalsFromPreprocessorSymbolNames(options.PreprocessorSymbolNames));

		var transformResults = context.SyntaxProvider.ForAttributeWithMetadataName(
			"Uno.Gallery.SamplePageAttribute",
			predicate: (_, _) => true,
			transform: Transform);

		// Branch A: surface Roslyn diagnostics from the error path
		var errorDiagnostics = transformResults
			.Where(r => r.Error is not null)
			.Select((r, _) => r.Error!.Value)
			.Collect();
		context.RegisterSourceOutput(errorDiagnostics, ReportDiagnostics);

		// Branch B: generate source from valid models only
		var validModels = transformResults
			.Where(r => r.Model is not null)
			.Select((r, _) => r.Model!.Value);

		var filteredSamples = validModels
			.Combine(currentPlatformProvider)
			.Select(FilterSamples)
			.Where(m => m is not null)
			.Collect();

		context.RegisterSourceOutput(filteredSamples, GenerateCode);
	}

	// ─── Source production ────────────────────────────────────────────────────

	private static void ReportDiagnostics(SourceProductionContext context, ImmutableArray<DiagnosticInfo> diagnostics)
	{
		foreach (var d in diagnostics)
			context.ReportDiagnostic(d.ToDiagnostic());
	}

	private static void GenerateCode(SourceProductionContext context, ImmutableArray<SamplesModel?> samples)
	{
		// Sort by FullyQualifiedName so generated output is deterministic regardless of syntax-tree
		// or file order.  Runtime UI order is controlled separately via SamplePageAttribute.SortOrder.
		var sorted = samples
			.OrderBy(m => m!.Value.FullyQualifiedName, StringComparer.Ordinal)
			.ToList();

		// UGG0004: warn when two samples share the same title (case-insensitive, matching
		// runtime navigation/search which also compares case-insensitively); report on the
		// later duplicate with the first-seen type's FQN in the message so the warning is
		// actionable and navigable.
		var seenTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var sample in sorted)
		{
			var s = sample!.Value;
			if (seenTitles.TryGetValue(s.Title, out var firstFqn))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.DuplicateSampleTitle,
					s.DeclarationLocation,
					s.Title,
					firstFqn));
			}
			else
			{
				seenTitles[s.Title] = s.FullyQualifiedName;
			}
		}

		var builder = new StringBuilder();
		builder.AppendLine("""
			namespace Uno.Gallery
			{
				public partial class App
				{
					public static Sample[] GetSamples()
					{
						return new[]
						{
			""");

		foreach (var sample in sorted)
		{
			var fullyQualifiedName = sample!.Value.FullyQualifiedName;
			builder.AppendLine($"\t\t\t\tnew global::Uno.Gallery.Sample({CreateSamplePageAttribute(sample!.Value)}, typeof({fullyQualifiedName})),");
		}

		builder.AppendLine("""
						};
					}
				}
			}
			""");

		context.AddSource("App.Samples.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	private static string CreateSamplePageAttribute(SamplesModel model)
	{
		var dataType = model.DataType is null ? "null" : $"typeof(global::{model.DataType})";
		var documentationLink = model.DocumentationLink is null ? "null" : $@"""{model.DocumentationLink}""";
		var description = model.Description is null ? "null" : $@"@""{model.Description.Replace(@"""", @"""""")}""";

		return $$"""
			new global::Uno.Gallery.SamplePageAttribute(category: {{model.Category}}, title: "{{model.Title}}", source: {{model.SourceSdk}}, glyph: "{{model.Glyph}}") { Description = {{description}}, DocumentationLink = {{documentationLink}}, DataType = {{dataType}}, SortOrder = {{model.SortOrder.ToString(CultureInfo.InvariantCulture)}} }
			""";
	}

	// ─── Filtering ────────────────────────────────────────────────────────────

	private static SamplesModel? FilterSamples(
		(SamplesModel Left, SampleConditionals Right) tuple,
		CancellationToken token) =>
		ShouldBeDisplayed(tuple.Left.Conditionals, tuple.Right) ? tuple.Left : null;

	private static bool ShouldBeDisplayed(SampleConditionals? conditionals, SampleConditionals compilationConditionals)
	{
		if (conditionals is null)
			return true;

		if (conditionals.Value.HasFlag(SampleConditionals.Disabled))
			return false;

		return conditionals.Value.HasFlag(compilationConditionals);
	}

	private static SampleConditionals GetSampleConditionalsFromPreprocessorSymbolNames(
		IEnumerable<string> preprocessorSymbolNames)
	{
		foreach (var preprocessorSymbol in preprocessorSymbolNames)
		{
			if (preprocessorSymbol == "WINDOWS") return SampleConditionals.Windows;
			if (preprocessorSymbol == "__ANDROID__") return SampleConditionals.Droid;
			if (preprocessorSymbol == "__MACOS__") return SampleConditionals.macOS;
			if (preprocessorSymbol == "__IOS__") return SampleConditionals.iOS;
			if (preprocessorSymbol == "__WASM__") return SampleConditionals.Wasm;
			if (preprocessorSymbol == "HAS_UNO_SKIA") return SampleConditionals.SkiaDesktop;
		}
		return SampleConditionals.Always;
	}

	// ─── Transform ────────────────────────────────────────────────────────────

	private static TransformResult Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not INamedTypeSymbol attributedSymbol)
		{
			return TransformResult.Fail(new DiagnosticInfo(
				Diagnostics.TargetNotNamedType,
				context.TargetNode.GetLocation(),
				context.TargetSymbol.ToDisplayString()));
		}

		SampleConditionals? conditionals = null;
		foreach (var attribute in attributedSymbol.GetAttributes())
		{
			if (attribute.AttributeClass?.Name != "SampleConditionalAttribute")
				continue;

			if (attribute.ConstructorArguments.Length != 1)
			{
				return TransformResult.Fail(new DiagnosticInfo(
					Diagnostics.UnexpectedConditionalShape,
					context.TargetNode.GetLocation(),
					attributedSymbol.ToDisplayString()));
			}

			conditionals = (SampleConditionals)attribute.ConstructorArguments[0].Value!;
		}

		var samplePageAttribute = context.Attributes[0];
		var ctor = samplePageAttribute.AttributeConstructor;

		if (!IsExpectedSamplePageAttributeShape(ctor))
		{
			var actual = ctor is null
				? "(no constructor resolved)"
				: $"({string.Join(", ", ctor.Parameters.Select(p => p.Name))})";
			return TransformResult.Fail(new DiagnosticInfo(
				Diagnostics.UnexpectedAttributeShape,
				context.TargetNode.GetLocation(),
				attributedSymbol.ToDisplayString(),
				actual));
		}

		var category = $"(global::Uno.Gallery.SampleCategory)({((int)samplePageAttribute.ConstructorArguments[0].Value!).ToString(CultureInfo.InvariantCulture)})";
		var title = (string)samplePageAttribute.ConstructorArguments[1].Value!;
		var source = $"(global::Uno.Gallery.SourceSdk)({((int)samplePageAttribute.ConstructorArguments[2].Value!).ToString(CultureInfo.InvariantCulture)})";
		var glyph = (string)samplePageAttribute.ConstructorArguments[3].Value!;

		var description = GetNamedArgumentOrDefault<string>(samplePageAttribute, "Description", null);
		var documentationLink = GetNamedArgumentOrDefault<string>(samplePageAttribute, "DocumentationLink", null);
		var dataType = GetNamedArgumentOrDefault<ISymbol>(samplePageAttribute, "DataType", null)?.ToDisplayString();
		var sortOrder = GetNamedArgumentOrDefault<int>(samplePageAttribute, "SortOrder", int.MaxValue);

		var declLoc = context.TargetNode.GetLocation();
		return TransformResult.Ok(new SamplesModel(
			conditionals,
			attributedSymbol.ToDisplayString(),
			category,
			title,
			description,
			documentationLink,
			dataType,
			source,
			glyph,
			sortOrder,
			declLoc));
	}

	/// <summary>
	/// Returns <c>true</c> when the resolved constructor has the four positional parameters the
	/// generator depends on: <c>category</c>, <c>title</c>, <c>source</c>, <c>glyph</c>.
	/// Internal for unit-test accessibility.
	/// </summary>
	internal static bool IsExpectedSamplePageAttributeShape(IMethodSymbol? ctor) =>
		ctor is not null
		&& ctor.Parameters.Length == 4
		&& ctor.Parameters[0].Name == "category"
		&& ctor.Parameters[1].Name == "title"
		&& ctor.Parameters[2].Name == "source"
		&& ctor.Parameters[3].Name == "glyph";

	private static T? GetNamedArgumentOrDefault<T>(AttributeData samplePageAttribute, string argumentName, T? defaultValue)
	{
		foreach (var namedArgument in samplePageAttribute.NamedArguments)
		{
			if (namedArgument.Key == argumentName)
				return (T)namedArgument.Value.Value!;
		}
		return defaultValue;
	}
}
