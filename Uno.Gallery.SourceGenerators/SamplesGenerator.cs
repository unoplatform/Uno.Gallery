using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
	// UGG0005  Error    Invalid explicit slug (not lowercase ASCII alphanumeric + interior hyphens)
	// UGG0006  Warning  Duplicate final slug, case-insensitive; both samples emit
	// UGG0007  Warning  RelatedSamples entry references an unknown final slug (ordinal match)
	// UGG0008  Error    Null or empty element in a string metadata array (Tags, RelatedSamples)
	// UGG0009  Error    Page type or DataType is abstract or has no accessible parameterless constructor
	// UGG0010  Error    Route constant identifier collision
	// UGG0011  Error    Explicit Stable or contract-v1 sample has incomplete contract metadata

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

		public static readonly DiagnosticDescriptor InvalidSlug = new(
			id: "UGG0005",
			title: "Invalid explicit slug on SamplePageAttribute",
			messageFormat: "SamplePageAttribute.Slug '{1}' on '{0}' is not a valid slug (expected lowercase ASCII " +
						   "alphanumeric with interior hyphens only, e.g. \"my-control\"). Code generation skipped.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Slugs must contain only lowercase ASCII letters (a-z), digits (0-9), and interior hyphens. " +
						 "They must not start or end with a hyphen and must not contain consecutive hyphens. " +
						 "Remove or correct the Slug property value.");

		public static readonly DiagnosticDescriptor DuplicateSlug = new(
			id: "UGG0006",
			title: "Duplicate final slug in generated catalog",
			messageFormat: "Sample slug '{0}' on '{1}' is also the final slug of '{2}'. Set an explicit unique Slug to avoid navigation ambiguity.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Each sample must have a unique URL slug. Two samples sharing the same final slug cannot " +
						 "both be navigated to by URL.  Set SamplePageAttribute.Slug explicitly on one of them.");

		public static readonly DiagnosticDescriptor UnknownRelatedSlug = new(
			id: "UGG0007",
			title: "RelatedSamples entry references an unknown slug",
			messageFormat: "RelatedSamples entry '{0}' on '{1}' does not match any known sample slug (ordinal comparison). The entry will be emitted but may produce a dead cross-link.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Each entry in RelatedSamples must exactly match (ordinal, lowercase) the final slug of " +
						 "another sample in the catalog. " +
						 "Verify the slug value or set SamplePageAttribute.Slug explicitly on the referenced sample.");

		public static readonly DiagnosticDescriptor NullOrEmptyArrayElement = new(
			id: "UGG0008",
			title: "Null or empty element in metadata string array",
			messageFormat: "'{0}' has null or empty metadata array elements: {1}. Null and empty entries are not meaningful; the sample has been excluded from the generated output.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Null or empty strings in Tags or RelatedSamples arrays are never meaningful and indicate " +
						 "a likely authoring error. Remove or fill in the empty entries.");

		public static readonly DiagnosticDescriptor AbstractOrNoAccessibleCtor = new(
			id: "UGG0009",
			title: "Page or DataType target cannot be instantiated",
			messageFormat: "{0} '{1}' is abstract or has no accessible (public, internal, or protected-internal) " +
						   "parameterless constructor; the generated factory cannot instantiate it. " +
						   "The sample has been skipped.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "The source generator emits `static () => new T()` factory lambdas for the page type and any " +
						 "configured DataType. Both must be concrete (non-abstract) and have a parameterless constructor " +
						 "that is accessible from the same assembly (public, internal, or protected-internal) so the " +
						 "factory lambda compiles and executes correctly. Mark or add a suitable constructor, or remove " +
						 "the SamplePageAttribute from an instantiation-unsafe type.");

		public static readonly DiagnosticDescriptor IdentifierCollision = new(
			id: "UGG0010",
			title: "Route constant identifier collision after PascalCase transformation",
			messageFormat: "Identifier '{0}' derived from slug '{1}' on '{2}' collides with the identifier derived " +
						   "from the different slug '{3}'. Both route constants are omitted from SampleRoutes. " +
						   "Set an explicit Slug that avoids the collision after PascalCase transformation.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "SamplesGenerator derives a PascalCase C# identifier for each sample slug by uppercasing " +
						 "the first character of each hyphen-separated segment and joining them. " +
						 "When two different slugs produce the same identifier (e.g. 'a1b' and 'a-1b' both yield 'A1b'), " +
						 "neither constant can be emitted. " +
						 "Set an explicit Slug on one of the samples so their derived identifiers are distinct.");

		public static readonly DiagnosticDescriptor IncompleteSampleContract = new(
			id: "UGG0011",
			title: "Sample detail contract is incomplete",
			messageFormat: "Sample detail contract on '{0}' has missing or invalid fields: {1}. " +
						   "Explicit Stable samples must author ContractVersion = 1 and a complete contract.",
			category: "SamplesGenerator",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Contract-v1 and explicitly Stable samples must provide complete, reviewed detail metadata. " +
						 "Fill every listed field rather than suppressing this diagnostic.");
	}

	// ─── StringSequence ──────────────────────────────────────────────────────

	/// <summary>
	/// Ordinal-sequence-equatable wrapper around <see cref="ImmutableArray{T}">ImmutableArray&lt;string&gt;</see>
	/// suitable for use as an incremental-generator pipeline value on netstandard2.0.
	/// <para>
	/// <see cref="ImmutableArray{T}"/> uses reference equality on its backing array, so two
	/// independently-constructed arrays with identical contents compare unequal, causing Roslyn's
	/// incremental cache to re-execute downstream steps even when nothing has changed.
	/// This wrapper compares element-by-element with <see cref="StringComparison.Ordinal"/> and
	/// computes a stable hash from the same.
	/// </para>
	/// <para>
	/// Default/empty safety: a default-initialised <c>StringSequence</c> (field not yet set) is
	/// treated as an empty sequence and compares equal to any other empty sequence.
	/// </para>
	/// </summary>
	private readonly struct StringSequence : IEquatable<StringSequence>
	{
		private readonly ImmutableArray<string> _values;

		public StringSequence(ImmutableArray<string> values) => _values = values;

		public ImmutableArray<string> Values =>
			_values.IsDefault ? ImmutableArray<string>.Empty : _values;

		public bool IsEmpty => _values.IsDefaultOrEmpty;

		public bool Equals(StringSequence other)
		{
			var a = Values;
			var b = other.Values;
			if (a.Length != b.Length) return false;
			for (int i = 0; i < a.Length; i++)
				if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
					return false;
			return true;
		}

		public override bool Equals(object? obj) => obj is StringSequence other && Equals(other);

		public override int GetHashCode()
		{
			var values = Values;
			if (values.IsEmpty) return 0;
			unchecked
			{
				int hash = 17;
				foreach (var v in values)
					hash = hash * 31 + (v is null ? 0 : StringComparer.Ordinal.GetHashCode(v));
				return hash;
			}
		}

		public static bool operator ==(StringSequence left, StringSequence right) => left.Equals(right);
		public static bool operator !=(StringSequence left, StringSequence right) => !left.Equals(right);
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
		// Stores the real SyntaxNode location so diagnostics point into the actual source tree.
		// Location equality in Roslyn is (SyntaxTree reference, TextSpan): two Location values
		// are equal iff they share the same SyntaxTree object and span.  Roslyn reuses SyntaxTree
		// objects for unchanged sources between incremental pipeline runs.
		Location DeclarationLocation,
		// Phase 2 metadata fields
		string FinalSlug,
		StringSequence Tags,
		int StatusValue,
		string? Owner,
		string? ReviewedOn,
		StringSequence RelatedSamples,
		int ContractVersion,
		int SupportedDesignsValue,
		string SupportedDesignsName,
		int SupportedRenderersValue,
		string SupportedRenderersName,
		StringSequence Requirements,
		StringSequence AccessibilityNotes,
		string? ResetBehavior,
		StringSequence Variants,
		StringSequence KnownLimitations,
		string? IssueLink,
		string? ApiLink,
		bool StatusExplicit,
		string? SourcePath,
		// ─── Manifest fields ─────────────────────────────────────────────────
		// Numeric values + member names for the manifest JSON (resolved via Roslyn during Transform).
		int CategoryNumericValue,
		string CategoryName,
		int SourceSdkNumericValue,
		string SourceSdkName,
		string StatusName);

	/// <summary>
	/// Lightweight, value-comparable diagnostic carrier used inside the incremental pipeline.
	/// Declared as <c>record struct</c> (not <c>readonly record struct</c>) so it compiles on
	/// netstandard2.0 without an IsExternalInit polyfill.
	/// </summary>
	private record struct DiagnosticInfo(
		DiagnosticDescriptor Descriptor,
		Location Location,
		string MessageArg0,
		string? MessageArg1 = null,
		string? MessageArg2 = null)
	{
		public Diagnostic ToDiagnostic()
		{
			if (MessageArg2 is not null)
				return Diagnostic.Create(Descriptor, Location, MessageArg0, MessageArg1, MessageArg2);
			if (MessageArg1 is not null)
				return Diagnostic.Create(Descriptor, Location, MessageArg0, MessageArg1);
			return Diagnostic.Create(Descriptor, Location, MessageArg0);
		}
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

		var errorDiagnostics = transformResults
			.Where(r => r.Error is not null)
			.Select((r, _) => r.Error!.Value)
			.Collect();
		context.RegisterSourceOutput(errorDiagnostics, ReportDiagnostics);

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
		var sorted = samples
			.OrderBy(m => m!.Value.FullyQualifiedName, StringComparer.Ordinal)
			.ToList();

		// UGG0004: warn on duplicate title (case-insensitive); report on later duplicate.
		var seenTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var sample in sorted)
		{
			var s = sample!.Value;
			if (seenTitles.TryGetValue(s.Title, out var firstFqn))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.DuplicateSampleTitle, s.DeclarationLocation, s.Title, firstFqn));
			}
			else
			{
				seenTitles[s.Title] = s.FullyQualifiedName;
			}
		}

		// UGG0006: warn on duplicate final slug (case-insensitive); both samples still emit.
		var seenSlugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var sample in sorted)
		{
			var s = sample!.Value;
			if (seenSlugs.TryGetValue(s.FinalSlug, out var firstFqn))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.DuplicateSlug, s.DeclarationLocation,
					s.FinalSlug, s.FullyQualifiedName, firstFqn));
			}
			else
			{
				seenSlugs[s.FinalSlug] = s.FullyQualifiedName;
			}
		}

		// UGG0007: warn when a RelatedSamples entry has no exact ordinal/lowercase match; entry still emits.
		// Final slugs are always lowercase (from IsValidSlug or DeriveSlug), so ordinal comparison
		// is the correct choice: a mixed-case related slug such as "Sample-B" must warn.
		var allSlugs = new HashSet<string>(seenSlugs.Keys, StringComparer.Ordinal);
		foreach (var sample in sorted)
		{
			var s = sample!.Value;
			foreach (var relSlug in s.RelatedSamples.Values)
			{
				if (!allSlugs.Contains(relSlug))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						Diagnostics.UnknownRelatedSlug, s.DeclarationLocation, relSlug, s.FullyQualifiedName));
				}
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
			var s = sample!.Value;
			builder.AppendLine($"\t\t\t\tnew global::Uno.Gallery.Sample({CreateSamplePageAttribute(s)}, typeof({s.FullyQualifiedName}), static () => new global::{s.FullyQualifiedName}(), {CreateDataFactory(s)}){CreateSampleObjectInitializer(s)},");
		}

		builder.AppendLine("""
						};
					}
				}
			}
			""");

		context.AddSource("App.Samples.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
		GenerateRoutes(context, sorted);
		GenerateManifest(context, sorted);
	}

	private static string CreateSamplePageAttribute(SamplesModel model)
	{
		var dataType = model.DataType is null ? "null" : $"typeof(global::{model.DataType})";
		var tagsLiteral = model.Tags.IsEmpty
			? "null"
			: $"new[] {{ {string.Join(", ", model.Tags.Values.Select(StringLiteral))} }}";
		var relatedLiteral = model.RelatedSamples.IsEmpty
			? "null"
			: $"new[] {{ {string.Join(", ", model.RelatedSamples.Values.Select(StringLiteral))} }}";
		var requirementsLiteral = StringArrayLiteral(model.Requirements);
		var accessibilityNotesLiteral = StringArrayLiteral(model.AccessibilityNotes);
		var variantsLiteral = StringArrayLiteral(model.Variants);
		var knownLimitationsLiteral = StringArrayLiteral(model.KnownLimitations);

		// Title and Glyph are positional constructor args; use StringLiteral so that embedded
		// double-quotes, backslashes, and any other characters round-trip correctly through the
		// generated verbatim string literal.
		return $"new global::Uno.Gallery.SamplePageAttribute(category: {model.Category}," +
			   $" title: {StringLiteral(model.Title)}," +
			   $" source: {model.SourceSdk}," +
			   $" glyph: {StringLiteral(model.Glyph)})" +
			   $" {{ Description = {StringLiteral(model.Description)}," +
			   $" DocumentationLink = {StringLiteral(model.DocumentationLink)}," +
			   $" DataType = {dataType}," +
			   $" SortOrder = {model.SortOrder.ToString(CultureInfo.InvariantCulture)}," +
			   $" Slug = {StringLiteral(model.FinalSlug)}," +
			   $" Tags = {tagsLiteral}," +
			   $" Status = (global::Uno.Gallery.SampleStatus)({model.StatusValue.ToString(CultureInfo.InvariantCulture)})," +
			   $" Owner = {StringLiteral(model.Owner)}," +
			   $" ReviewedOn = {StringLiteral(model.ReviewedOn)}," +
			   $" ContractVersion = {model.ContractVersion.ToString(CultureInfo.InvariantCulture)}," +
			   $" SupportedDesigns = (global::Uno.Gallery.SampleDesigns)({model.SupportedDesignsValue.ToString(CultureInfo.InvariantCulture)})," +
			   $" SupportedRenderers = (global::Uno.Gallery.SampleRenderers)({model.SupportedRenderersValue.ToString(CultureInfo.InvariantCulture)})," +
			   $" Requirements = {requirementsLiteral}," +
			   $" AccessibilityNotes = {accessibilityNotesLiteral}," +
			   $" ResetBehavior = {StringLiteral(model.ResetBehavior)}," +
			   $" Variants = {variantsLiteral}," +
			   $" KnownLimitations = {knownLimitationsLiteral}," +
			   $" IssueLink = {StringLiteral(model.IssueLink)}," +
			   $" ApiLink = {StringLiteral(model.ApiLink)}," +
			   $" RelatedSamples = {relatedLiteral} }}";
	}

	private static string StringArrayLiteral(StringSequence values) =>
		values.IsEmpty
			? "null"
			: $"new[] {{ {string.Join(", ", values.Values.Select(StringLiteral))} }}";

	private static string CreateSampleObjectInitializer(SamplesModel model)
	{
		if (model.SourcePath is null) return string.Empty;
		return $" {{ SourcePath = {StringLiteral(model.SourcePath)} }}";
	}

	private static string CreateDataFactory(SamplesModel model) =>
		model.DataType is null ? "null" : $"static () => new global::{model.DataType}()";

	private static string StringLiteral(string? value)
	{
		if (value is null) return "null";
		return $@"@""{value.Replace(@"""", @"""""")}""";
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
		var categoryNumericValue = (int)samplePageAttribute.ConstructorArguments[0].Value!;
		var categoryName = GetEnumMemberName(samplePageAttribute.ConstructorArguments[0]);
		var title = (string)samplePageAttribute.ConstructorArguments[1].Value!;
		var source = $"(global::Uno.Gallery.SourceSdk)({((int)samplePageAttribute.ConstructorArguments[2].Value!).ToString(CultureInfo.InvariantCulture)})";
		var sourceNumericValue = (int)samplePageAttribute.ConstructorArguments[2].Value!;
		var sourceSdkName = GetEnumMemberName(samplePageAttribute.ConstructorArguments[2]);
		var glyph = (string)samplePageAttribute.ConstructorArguments[3].Value!;

		var description = GetNamedArgumentOrDefault<string>(samplePageAttribute, "Description", null);
		var documentationLink = GetNamedArgumentOrDefault<string>(samplePageAttribute, "DocumentationLink", null);
		var dataTypeSymbol = GetNamedArgumentOrDefault<ISymbol>(samplePageAttribute, "DataType", null) as INamedTypeSymbol;
		var dataType = dataTypeSymbol?.ToDisplayString();
		var sortOrder = GetNamedArgumentOrDefault<int>(samplePageAttribute, "SortOrder", int.MaxValue);

		var declLoc = context.TargetNode.GetLocation();

		// Phase 2: validate explicit slug (UGG0005) and compute FinalSlug.
		// Report on the Slug argument expression (narrower span) when obtainable from the syntax tree;
		// fall back to the class declaration location.
		var explicitSlug = GetNamedArgumentOrDefault<string>(samplePageAttribute, "Slug", null);
		if (explicitSlug is not null && !IsValidSlug(explicitSlug))
		{
			var slugLoc = GetNamedArgumentExpressionLocation(
				samplePageAttribute, "Slug", cancellationToken) ?? declLoc;
			return TransformResult.Fail(new DiagnosticInfo(
				Diagnostics.InvalidSlug, slugLoc,
				attributedSymbol.ToDisplayString(), explicitSlug));
		}
		var finalSlug = explicitSlug ?? SlugHelper.DeriveSlug(title);

		// Phase 2: remaining metadata.
		// UGG0008 from GetNamedStringArray is accumulated and returned as the first error found;
		// the model is not emitted when any array element is null/empty.
		var invalidElements = new List<(string ArrayName, int Index)>();
		var tags = GetNamedStringArray(samplePageAttribute, "Tags", invalidElements);
		var relatedSamples = GetNamedStringArray(samplePageAttribute, "RelatedSamples", invalidElements);
		var requirements = GetNamedStringArray(samplePageAttribute, "Requirements", invalidElements);
		var accessibilityNotes = GetNamedStringArray(samplePageAttribute, "AccessibilityNotes", invalidElements);
		var variants = GetNamedStringArray(samplePageAttribute, "Variants", invalidElements);
		var knownLimitations = GetNamedStringArray(samplePageAttribute, "KnownLimitations", invalidElements);
		if (invalidElements.Count > 0)
		{
			var parts = string.Join(", ", invalidElements.Select(e => $"{e.ArrayName}[{e.Index}]"));
			return TransformResult.Fail(new DiagnosticInfo(
				Diagnostics.NullOrEmptyArrayElement,
				declLoc,
				attributedSymbol.ToDisplayString(), parts));
		}

		// UGG0009: page type must be concrete and have an accessible parameterless constructor.
		if (!IsInstantiable(attributedSymbol))
		{
			return TransformResult.Fail(new DiagnosticInfo(
				Diagnostics.AbstractOrNoAccessibleCtor, declLoc,
				"Page type", attributedSymbol.ToDisplayString()));
		}

		// UGG0009: DataType, when specified, must be concrete and have an accessible parameterless constructor.
		if (dataTypeSymbol is not null && !IsInstantiable(dataTypeSymbol))
		{
			var dataTypeLoc = GetNamedArgumentExpressionLocation(samplePageAttribute, "DataType", cancellationToken) ?? declLoc;
			return TransformResult.Fail(new DiagnosticInfo(
				Diagnostics.AbstractOrNoAccessibleCtor, dataTypeLoc,
				"DataType", dataTypeSymbol.ToDisplayString()));
		}

		var (statusValue, statusName) = GetNamedEnumWithName(samplePageAttribute, "Status", 0, "Stable");
		var statusExplicit = HasNamedArgument(samplePageAttribute, "Status");
		var owner = GetNamedArgumentOrDefault<string>(samplePageAttribute, "Owner", null);
		var reviewedOn = GetNamedArgumentOrDefault<string>(samplePageAttribute, "ReviewedOn", null);
		var contractVersion = GetNamedArgumentOrDefault<int>(samplePageAttribute, "ContractVersion", 0);
		var (supportedDesignsValue, supportedDesignsName) =
			GetNamedFlagsWithName(samplePageAttribute, "SupportedDesigns", 0, "None");
		var (supportedRenderersValue, supportedRenderersName) =
			GetNamedFlagsWithName(samplePageAttribute, "SupportedRenderers", 0, "None");
		var resetBehavior = GetNamedArgumentOrDefault<string>(samplePageAttribute, "ResetBehavior", null);
		var issueLink = GetNamedArgumentOrDefault<string>(samplePageAttribute, "IssueLink", null);
		var apiLink = GetNamedArgumentOrDefault<string>(samplePageAttribute, "ApiLink", null);

		if ((statusExplicit && statusValue == 0) || contractVersion != 0)
		{
			var invalidContractFields = GetInvalidContractFields(
				statusExplicit,
				statusValue,
				contractVersion,
				description,
				documentationLink,
				tags,
				owner,
				reviewedOn,
				supportedDesignsValue,
				supportedRenderersValue,
				requirements,
				accessibilityNotes,
				resetBehavior,
				variants,
				issueLink,
				apiLink);
			if (invalidContractFields.Count > 0)
			{
				return TransformResult.Fail(new DiagnosticInfo(
					Diagnostics.IncompleteSampleContract,
					declLoc,
					attributedSymbol.ToDisplayString(),
					string.Join(", ", invalidContractFields)));
			}
		}
		var sourcePath = ComputeSourcePath(declLoc.SourceTree?.FilePath);

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
			declLoc,
			finalSlug,
			tags,
			statusValue,
			owner,
			reviewedOn,
			relatedSamples,
			contractVersion,
			supportedDesignsValue,
			supportedDesignsName,
			supportedRenderersValue,
			supportedRenderersName,
			requirements,
			accessibilityNotes,
			resetBehavior,
			variants,
			knownLimitations,
			issueLink,
			apiLink,
			statusExplicit,
			sourcePath,
			categoryNumericValue,
			categoryName,
			sourceNumericValue,
			sourceSdkName,
			statusName));
	}

	private static List<string> GetInvalidContractFields(
		bool statusExplicit,
		int statusValue,
		int contractVersion,
		string? description,
		string? documentationLink,
		StringSequence tags,
		string? owner,
		string? reviewedOn,
		int supportedDesigns,
		int supportedRenderers,
		StringSequence requirements,
		StringSequence accessibilityNotes,
		string? resetBehavior,
		StringSequence variants,
		string? issueLink,
		string? apiLink)
	{
		var invalid = new List<string>();
		if (contractVersion is not 0 and not 1) invalid.Add("ContractVersion (supported values are 0 and 1)");
		if (statusExplicit && statusValue == 0 && contractVersion != 1) invalid.Add("ContractVersion (must be 1)");
		if (string.IsNullOrWhiteSpace(description)) invalid.Add("Description");
		if (!IsValidAbsoluteUri(documentationLink)) invalid.Add("DocumentationLink (expected absolute URI)");
		if (!HasNonWhiteSpaceValues(tags)) invalid.Add("Tags");
		if (string.IsNullOrWhiteSpace(owner)) invalid.Add("Owner");
		if (!IsValidReviewedOn(reviewedOn)) invalid.Add("ReviewedOn (expected YYYY-MM-DD)");
		if (supportedDesigns == 0 || (supportedDesigns & ~0x1F) != 0) invalid.Add("SupportedDesigns");
		if (supportedRenderers == 0 || (supportedRenderers & ~0x07) != 0) invalid.Add("SupportedRenderers");
		if (!HasNonWhiteSpaceValues(requirements)) invalid.Add("Requirements");
		if (!HasNonWhiteSpaceValues(accessibilityNotes)) invalid.Add("AccessibilityNotes");
		if (string.IsNullOrWhiteSpace(resetBehavior)) invalid.Add("ResetBehavior");
		if (!HasNonWhiteSpaceValues(variants)) invalid.Add("Variants");
		if (issueLink is not null && !IsValidAbsoluteUri(issueLink)) invalid.Add("IssueLink (expected absolute URI)");
		if (apiLink is not null && !IsValidAbsoluteUri(apiLink)) invalid.Add("ApiLink (expected absolute URI)");
		return invalid;
	}

	private static bool HasNonWhiteSpaceValues(StringSequence values) =>
		!values.IsEmpty && values.Values.All(value => !string.IsNullOrWhiteSpace(value));

	private static bool IsValidReviewedOn(string? value) =>
		value is not null
		&& DateTime.TryParseExact(
			value,
			"yyyy-MM-dd",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out _);

	private static bool IsValidAbsoluteUri(string? value) =>
		!string.IsNullOrWhiteSpace(value)
		&& Uri.TryCreate(value, UriKind.Absolute, out var uri)
		&& (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

	private static bool HasNamedArgument(AttributeData attribute, string name) =>
		attribute.NamedArguments.Any(argument => argument.Key == name);

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

	/// <summary>
	/// Returns the enum member name for a <see cref="TypedConstant"/> of enum kind.
	/// Iterates the enum type's fields, matching by value. Returns the numeric string as fallback.
	/// </summary>
	private static string GetEnumMemberName(TypedConstant constant)
	{
		if (constant.Type is INamedTypeSymbol enumType && !constant.IsNull && constant.Value is not null)
		{
			var intValue = Convert.ToInt32(constant.Value, CultureInfo.InvariantCulture);
			foreach (var member in enumType.GetMembers())
			{
				if (member is IFieldSymbol field && field.HasConstantValue && field.ConstantValue is not null)
				{
					try
					{
						if (Convert.ToInt32(field.ConstantValue, CultureInfo.InvariantCulture) == intValue)
							return field.Name;
					}
					catch (OverflowException) { /* skip large flag values */ }
				}
			}
			return intValue.ToString(CultureInfo.InvariantCulture);
		}
		return constant.Value?.ToString() ?? "0";
	}

	/// <summary>
	/// Reads a named enum argument, returning both the integer value and the enum member name.
	/// Falls back to <paramref name="defaultValue"/> / <paramref name="defaultName"/> when absent.
	/// </summary>
	private static (int Value, string Name) GetNamedEnumWithName(
		AttributeData attr, string name, int defaultValue, string defaultName)
	{
		foreach (var named in attr.NamedArguments)
		{
			if (named.Key != name) continue;
			if (named.Value.Value is null) return (defaultValue, defaultName);
			var intValue = Convert.ToInt32(named.Value.Value, CultureInfo.InvariantCulture);
			var memberName = GetEnumMemberName(named.Value);
			return (intValue, memberName);
		}
		return (defaultValue, defaultName);
	}

	private static (int Value, string Name) GetNamedFlagsWithName(
		AttributeData attr, string name, int defaultValue, string defaultName)
	{
		foreach (var named in attr.NamedArguments)
		{
			if (named.Key != name) continue;
			if (named.Value.Value is null) return (defaultValue, defaultName);

			var value = Convert.ToInt32(named.Value.Value, CultureInfo.InvariantCulture);
			if (value == 0) return (0, defaultName);
			if (named.Value.Type is not INamedTypeSymbol enumType)
				return (value, value.ToString(CultureInfo.InvariantCulture));

			var names = enumType.GetMembers()
				.OfType<IFieldSymbol>()
				.Where(field => field.HasConstantValue && field.ConstantValue is not null)
				.Select(field => (field.Name, Value: Convert.ToInt32(field.ConstantValue, CultureInfo.InvariantCulture)))
				.Where(field => field.Value != 0 && (field.Value & (field.Value - 1)) == 0 && (value & field.Value) != 0)
				.OrderBy(field => field.Value)
				.Select(field => field.Name)
				.ToArray();
			return (value, names.Length == 0 ? value.ToString(CultureInfo.InvariantCulture) : string.Join(", ", names));
		}
		return (defaultValue, defaultName);
	}

	// ─── Route constants generation ───────────────────────────────────────────

	/// <summary>
	/// Converts a validated slug to a PascalCase C# identifier.
	/// Each hyphen-delimited segment has its first character uppercased; segments are joined.
	/// A leading underscore is prepended when the result starts with a digit.
	/// </summary>
	/// <remarks>
	/// Collision scenario: slugs containing digit-start segments after a hyphen produce the same
	/// identifier as the equivalent single-segment slug.  Example: <c>a1b</c> → <c>A1b</c> and
	/// <c>a-1b</c> → <c>A</c> + <c>1b</c> = <c>A1b</c>.  Such collisions are reported as UGG0010.
	/// </remarks>
	internal static string SlugToPascalIdentifier(string slug)
	{
		var segments = slug.Split('-');
		var sb = new StringBuilder();
		foreach (var seg in segments)
		{
			if (seg.Length == 0) continue;
			var first = seg[0];
			sb.Append(first >= 'a' && first <= 'z' ? (char)(first - 32) : first);
			if (seg.Length > 1)
				sb.Append(seg, 1, seg.Length - 1);
		}
		if (sb.Length == 0) return "_Sample";
		return sb[0] >= '0' && sb[0] <= '9' ? "_" + sb : sb.ToString();
	}

	/// <summary>
	/// Emits <c>SampleRoutes.g.cs</c> — one <c>public const string</c> per unique slug.
	/// Detects identifier collisions (UGG0010) across <em>different</em> slugs and omits both
	/// constants when a collision is found, without affecting <c>GetSamples</c> output.
	/// UGG0006 slug-duplicate pairs share the same slug and thus the same identifier;
	/// one constant is emitted for the shared value.
	/// </summary>
	private static void GenerateRoutes(SourceProductionContext context, List<SamplesModel?> sorted)
	{
		// First pass: detect identifier collisions (different slug → same identifier).
		// UGG0010 fires on the first declaration (once, when the first collision is detected)
		// and on every later colliding declaration.
		var identifierToFirstSlug = new Dictionary<string, string>(StringComparer.Ordinal);
		var identifierToFirstInfo = new Dictionary<string, (string Fqn, Location Loc)>(StringComparer.Ordinal);
		var collidingIdentifiers = new HashSet<string>(StringComparer.Ordinal);
		// Track which first-declarations have already received a UGG0010 (report once per identifier).
		var firstDeclReported = new HashSet<string>(StringComparer.Ordinal);

		foreach (var item in sorted)
		{
			var s = item!.Value;
			var id = SlugToPascalIdentifier(s.FinalSlug);
			if (identifierToFirstSlug.TryGetValue(id, out var firstSlug))
			{
				// Same identifier from a different slug → UGG0010 collision.
				// Identical slug (UGG0006 pair) with same identifier is expected, not a collision.
				if (!string.Equals(firstSlug, s.FinalSlug, StringComparison.OrdinalIgnoreCase))
				{
					collidingIdentifiers.Add(id);
					var (firstFqn, firstLoc) = identifierToFirstInfo[id];

					// Report on the first declaration exactly once.
					if (firstDeclReported.Add(id))
					{
						context.ReportDiagnostic(Diagnostic.Create(
							Diagnostics.IdentifierCollision,
							firstLoc,
							id, firstSlug, firstFqn, s.FinalSlug));
					}
					// Report on this (later) colliding declaration.
					context.ReportDiagnostic(Diagnostic.Create(
						Diagnostics.IdentifierCollision,
						s.DeclarationLocation,
						id, s.FinalSlug, s.FullyQualifiedName, firstSlug));
				}
			}
			else
			{
				identifierToFirstSlug[id] = s.FinalSlug;
				identifierToFirstInfo[id] = (s.FullyQualifiedName, s.DeclarationLocation);
			}
		}

		// Build the file.
		var sb = new StringBuilder();
		sb.AppendLine("// <auto-generated/>");
		sb.AppendLine("namespace Uno.Gallery");
		sb.AppendLine("{");
		sb.AppendLine("\t/// <summary>");
		sb.AppendLine("\t/// Generated route constants — one <c>public const string</c> per unique sample slug.");
		sb.AppendLine("\t/// These are the stable internal contract for sample-navigation identifiers.");
		sb.AppendLine("\t/// </summary>");
		sb.AppendLine("\t/// <remarks>");
		sb.AppendLine("\t/// Generated by <c>SamplesGenerator</c>; do not edit manually.");
		sb.AppendLine("\t/// Identifier collisions (UGG0010) are omitted; slug duplicates (UGG0006)");
		sb.AppendLine("\t/// emit a single shared constant because their slug value is identical.");
		sb.AppendLine("\t/// </remarks>");
		sb.AppendLine("\tinternal static class SampleRoutes");
		sb.AppendLine("\t{");

		var emittedIdentifiers = new HashSet<string>(StringComparer.Ordinal);
		foreach (var item in sorted)
		{
			var s = item!.Value;
			var id = SlugToPascalIdentifier(s.FinalSlug);
			if (collidingIdentifiers.Contains(id)) continue;       // omit collisions
			if (!emittedIdentifiers.Add(id)) continue;             // already emitted (UGG0006 pair)
			sb.AppendLine($"\t\tpublic const string {id} = @\"{s.FinalSlug.Replace("\"", "\"\"")}\";");
		}

		sb.AppendLine("\t}");
		sb.AppendLine("}");

		context.AddSource("SampleRoutes.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
	}

	// ─── Manifest generation ──────────────────────────────────────────────────

	/// <summary>
	/// Emits <c>SampleManifest.g.cs</c> — a deterministic JSON catalog of all samples in the
	/// current compilation target, sorted by FQN and wrapped in a <c>GetJson()</c> method.
	/// No file I/O or external packages are used; escaping is implemented inline.
	/// Schema version 1.
	/// </summary>
	private static void GenerateManifest(SourceProductionContext context, List<SamplesModel?> sorted)
	{
		// Each JSON chunk is emitted as a separate sb.Append() statement so that no single generated
		// string literal exceeds 8,000 *escaped* characters.  Without escaped-length bounding, a
		// sequence of control characters or non-ASCII could expand up to 6× per raw char, producing
		// literals large enough to stress IL toolchains.
		// Two-stage escaping: AppendJsonString (JSON layer) pre-encodes surrogate pairs and control
		// characters as JSON \uXXXX sequences; any remaining non-ASCII characters in the resulting
		// JSON string are then re-escaped to \uXXXX by EmitCSharpStringAppendSafe (C#-literal layer),
		// so the final generated C# source contains only printable ASCII.
		// EmitCSharpStringAppendSafe also measures escaped length and flushes a new Append call
		// before crossing the 8,000-character limit.

		var file = new StringBuilder();
		file.AppendLine("// <auto-generated/>");
		file.AppendLine("namespace Uno.Gallery");
		file.AppendLine("{");
		file.AppendLine("\t/// <summary>");
		file.AppendLine("\t/// Deterministic JSON catalog of samples compiled into this target.");
		file.AppendLine("\t/// Schema version 1. Entries are sorted by fully-qualified type name.");
		file.AppendLine("\t/// This is the stable internal contract; physical file export is a future step.");
		file.AppendLine("\t/// </summary>");
		file.AppendLine("\t/// <remarks>Generated by <c>SamplesGenerator</c>; do not edit manually.</remarks>");
		file.AppendLine("\tinternal static class SampleManifest");
		file.AppendLine("\t{");
		file.AppendLine("\t\t/// <summary>Returns the deterministic JSON catalog (schema version 1).</summary>");
		file.AppendLine("\t\tpublic static string GetJson()");
		file.AppendLine("\t\t{");
		file.AppendLine("\t\t\tvar sb = new global::System.Text.StringBuilder();");

		EmitCSharpStringAppendSafe(file, "{\"schemaVersion\":1,\"samples\":[", 3);

		bool firstSample = true;
		foreach (var item in sorted)
		{
			var sampleJson = BuildSampleJson(item!.Value).ToString();
			if (!firstSample)
				EmitCSharpStringAppendSafe(file, ",", 3);
			firstSample = false;
			EmitCSharpStringAppendSafe(file, sampleJson, 3);
		}

		EmitCSharpStringAppendSafe(file, "]}", 3);
		file.AppendLine("\t\t\treturn sb.ToString();");
		file.AppendLine("\t\t}");
		file.AppendLine("\t}");
		file.AppendLine("}");

		context.AddSource("SampleManifest.g.cs", SourceText.From(file.ToString(), Encoding.UTF8));
	}

	/// <summary>
	/// Appends one or more <c>sb.Append("...");</c> statements for <paramref name="value"/>,
	/// splitting into multiple calls so that no single generated string literal contains more
	/// than <c>8,000</c> escaped characters.  Each source character may expand to up to six
	/// escaped characters (e.g. a lone control char → <c>\u001F</c>); raw-length chunking alone
	/// cannot bound the literal size.  Escape sequences are never split across chunk boundaries.
	/// </summary>
	private static void EmitCSharpStringAppendSafe(StringBuilder file, string value, int indentTabs)
	{
		const int MaxEscapedChunkLen = 8_000;
		var indent = new string('\t', indentTabs);
		var chunk = new StringBuilder();

		void Flush()
		{
			if (chunk.Length == 0) return;
			file.Append(indent);
			file.Append("sb.Append(\"");
			file.Append(chunk);
			file.AppendLine("\");");
			chunk.Clear();
		}

		foreach (var c in value)
		{
			string? esc = c switch
			{
				'"' => "\\\"",
				'\\' => "\\\\",
				'\r' => "\\r",
				'\n' => "\\n",
				'\t' => "\\t",
				'\b' => "\\b",
				'\f' => "\\f",
				_ => null
			};
			if (esc is null && (c < 0x20 || c > 0x7E))
				esc = "\\u" + ((int)c).ToString("X4", CultureInfo.InvariantCulture);

			var escapedLen = esc?.Length ?? 1;
			if (chunk.Length + escapedLen > MaxEscapedChunkLen)
				Flush();

			if (esc is not null)
				chunk.Append(esc);
			else
				chunk.Append(c);
		}
		Flush();
	}

	/// <summary>
	/// Builds the compact JSON object for a single sample.
	/// All string values are JSON-escaped via <see cref="AppendJsonString"/>.
	/// </summary>
	private static StringBuilder BuildSampleJson(in SamplesModel s)
	{
		var sb = new StringBuilder();
		sb.Append('{');

		AppendJsonField(sb, "fqn", s.FullyQualifiedName);
		sb.Append(',');
		AppendJsonField(sb, "slug", s.FinalSlug);
		sb.Append(',');
		AppendJsonField(sb, "title", s.Title);
		sb.Append(',');

		// category: {"value":N,"name":"Controls"}
		sb.Append("\"category\":{");
		AppendJsonFieldInt(sb, "value", s.CategoryNumericValue);
		sb.Append(',');
		AppendJsonField(sb, "name", s.CategoryName);
		sb.Append('}');
		sb.Append(',');

		AppendJsonField(sb, "description", s.Description);
		sb.Append(',');
		AppendJsonField(sb, "glyph", s.Glyph);
		sb.Append(',');
		AppendJsonField(sb, "documentationLink", s.DocumentationLink);
		sb.Append(',');

		// sourceSdk: {"value":N,"name":"WinUI"}
		sb.Append("\"sourceSdk\":{");
		AppendJsonFieldInt(sb, "value", s.SourceSdkNumericValue);
		sb.Append(',');
		AppendJsonField(sb, "name", s.SourceSdkName);
		sb.Append('}');
		sb.Append(',');

		AppendJsonFieldInt(sb, "sortOrder", s.SortOrder);
		sb.Append(',');

		// status: {"value":N,"name":"Stable"}
		sb.Append("\"status\":{");
		AppendJsonFieldInt(sb, "value", s.StatusValue);
		sb.Append(',');
		AppendJsonField(sb, "name", s.StatusName);
		sb.Append('}');
		sb.Append(',');

		// tags: [...] or []
		sb.Append("\"tags\":[");
		var tags = s.Tags.Values;
		for (int i = 0; i < tags.Length; i++)
		{
			if (i > 0) sb.Append(',');
			AppendJsonString(sb, tags[i]);
		}
		sb.Append(']');
		sb.Append(',');

		AppendJsonField(sb, "owner", s.Owner);
		sb.Append(',');
		AppendJsonField(sb, "reviewedOn", s.ReviewedOn);
		sb.Append(',');
		AppendJsonFieldInt(sb, "contractVersion", s.ContractVersion);
		sb.Append(',');

		sb.Append("\"supportedDesigns\":{");
		AppendJsonFieldInt(sb, "value", s.SupportedDesignsValue);
		sb.Append(',');
		AppendJsonField(sb, "name", s.SupportedDesignsName);
		sb.Append('}');
		sb.Append(',');

		sb.Append("\"supportedRenderers\":{");
		AppendJsonFieldInt(sb, "value", s.SupportedRenderersValue);
		sb.Append(',');
		AppendJsonField(sb, "name", s.SupportedRenderersName);
		sb.Append('}');
		sb.Append(',');

		AppendJsonArrayField(sb, "requirements", s.Requirements);
		sb.Append(',');
		AppendJsonArrayField(sb, "accessibilityNotes", s.AccessibilityNotes);
		sb.Append(',');
		AppendJsonField(sb, "resetBehavior", s.ResetBehavior);
		sb.Append(',');
		AppendJsonArrayField(sb, "variants", s.Variants);
		sb.Append(',');
		AppendJsonArrayField(sb, "knownLimitations", s.KnownLimitations);
		sb.Append(',');
		AppendJsonField(sb, "issueLink", s.IssueLink);
		sb.Append(',');
		AppendJsonField(sb, "apiLink", s.ApiLink);
		sb.Append(',');
		AppendJsonFieldBoolean(sb, "statusExplicit", s.StatusExplicit);
		sb.Append(',');

		// relatedSamples: [...] or []
		sb.Append("\"relatedSamples\":[");
		var related = s.RelatedSamples.Values;
		for (int i = 0; i < related.Length; i++)
		{
			if (i > 0) sb.Append(',');
			AppendJsonString(sb, related[i]);
		}
		sb.Append(']');
		sb.Append(',');

		AppendJsonField(sb, "sourcePath", s.SourcePath);
		sb.Append(',');

		// platformConditionals: null when no SampleConditionalAttribute, numeric value otherwise
		sb.Append("\"platformConditionals\":");
		if (s.Conditionals is null)
			sb.Append("null");
		else
			sb.Append(((uint)s.Conditionals.Value).ToString(CultureInfo.InvariantCulture));

		sb.Append('}');
		return sb;
	}

	private static void AppendJsonArrayField(StringBuilder sb, string key, StringSequence values)
	{
		sb.Append('"');
		sb.Append(key);
		sb.Append("\":[");
		var items = values.Values;
		for (int i = 0; i < items.Length; i++)
		{
			if (i > 0) sb.Append(',');
			AppendJsonString(sb, items[i]);
		}
		sb.Append(']');
	}

	private static void AppendJsonField(StringBuilder sb, string key, string? value)
	{
		sb.Append('"');
		sb.Append(key);
		sb.Append("\":");
		AppendJsonString(sb, value);
	}

	private static void AppendJsonFieldInt(StringBuilder sb, string key, int value)
	{
		sb.Append('"');
		sb.Append(key);
		sb.Append("\":");
		sb.Append(value.ToString(CultureInfo.InvariantCulture));
	}

	private static void AppendJsonFieldBoolean(StringBuilder sb, string key, bool value)
	{
		sb.Append('"');
		sb.Append(key);
		sb.Append("\":");
		sb.Append(value ? "true" : "false");
	}

	/// <summary>
	/// Appends a JSON string value (with surrounding double-quotes) or <c>null</c> literal.
	/// Control characters are escaped as <c>\uXXXX</c>; <c>"</c> and <c>\</c> are escaped.
	/// Valid surrogate pairs are emitted as two consecutive <c>\uHHHH\uLLLL</c> JSON escapes so
	/// that JSON parsers reconstruct the original code point.  Lone surrogates (high or low)
	/// are replaced with <c>\uFFFD</c> (replacement character) to produce valid JSON.
	/// Non-ASCII BMP characters are passed through as UTF-8 (valid JSON).
	/// </summary>
	private static void AppendJsonString(StringBuilder sb, string? value)
	{
		if (value is null) { sb.Append("null"); return; }
		sb.Append('"');
		for (int i = 0; i < value.Length; i++)
		{
			var c = value[i];

			// Valid surrogate pair → emit as \uHHHH\uLLLL so any JSON parser reconstructs the codepoint.
			if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
			{
				sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
				i++;
				sb.Append("\\u").Append(((int)value[i]).ToString("X4", CultureInfo.InvariantCulture));
				continue;
			}
			// Lone surrogate (high without following low, or unpaired low) → replacement character.
			if (char.IsSurrogate(c))
			{
				sb.Append("\\uFFFD");
				continue;
			}

			switch (c)
			{
				case '"': sb.Append("\\\""); break;
				case '\\': sb.Append("\\\\"); break;
				case '\b': sb.Append("\\b"); break;
				case '\f': sb.Append("\\f"); break;
				case '\n': sb.Append("\\n"); break;
				case '\r': sb.Append("\\r"); break;
				case '\t': sb.Append("\\t"); break;
				default:
					if (c < 0x20)
						sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
					else
						sb.Append(c);
					break;
			}
		}
		sb.Append('"');
	}


	private static T? GetNamedArgumentOrDefault<T>(AttributeData samplePageAttribute, string argumentName, T? defaultValue)
	{
		foreach (var namedArgument in samplePageAttribute.NamedArguments)
		{
			if (namedArgument.Key == argumentName)
			{
				var rawValue = namedArgument.Value.Value;
				if (rawValue is null) return defaultValue;
				return (T)rawValue;
			}
		}
		return defaultValue;
	}

	/// <summary>
	/// Returns the <see cref="Location"/> of the value expression for a named attribute argument,
	/// obtained by walking the <see cref="AttributeSyntax"/>.
	/// This produces a span narrower than the class declaration, enabling precise pragma suppressions
	/// and IDE click-through.  Returns <c>null</c> when the syntax reference is unavailable.
	/// </summary>
	private static Location? GetNamedArgumentExpressionLocation(
		AttributeData attr,
		string name,
		CancellationToken cancellationToken)
	{
		if (attr.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax attrSyntax)
			return null;
		if (attrSyntax.ArgumentList is null) return null;
		foreach (var arg in attrSyntax.ArgumentList.Arguments)
		{
			if (arg.NameEquals?.Name.Identifier.Text == name && arg.Expression is not null)
				return arg.Expression.GetLocation();
		}
		return null;
	}

	/// <summary>
	/// Reads a named string-array argument.  Null or empty element strings append
	/// <c>(arrayName, index)</c> pairs to <paramref name="invalidElementsOut"/>; such elements are
	/// excluded from the returned sequence.  An explicitly-null array (<c>Tags = null</c>) is
	/// treated as omitted/empty and never throws.
	/// Returns an empty <see cref="StringSequence"/> when the argument is absent or null.
	/// </summary>
	private static StringSequence GetNamedStringArray(
		AttributeData attr,
		string name,
		List<(string ArrayName, int Index)> invalidElementsOut)
	{
		foreach (var named in attr.NamedArguments)
		{
			if (named.Key != name) continue;
			if (named.Value.Kind != TypedConstantKind.Array || named.Value.IsNull) return default;
			var builder = ImmutableArray.CreateBuilder<string>(named.Value.Values.Length);
			for (int i = 0; i < named.Value.Values.Length; i++)
			{
				var item = named.Value.Values[i];
				if (item.Kind == TypedConstantKind.Primitive && item.Value is string s && s.Length > 0)
					builder.Add(s);
				else
					invalidElementsOut.Add((name, i));
			}
			return new StringSequence(builder.ToImmutable());
		}
		return default;
	}

	/// <summary>
	/// Validates the slug format: lowercase ASCII alphanumeric with interior hyphens only.
	/// No leading/trailing hyphens, no consecutive hyphens, no uppercase or non-ASCII.
	/// Internal for unit-test accessibility.
	/// </summary>
	internal static bool IsValidSlugPublicForTest(string slug) => IsValidSlug(slug);

	private static bool IsValidSlug(string slug)
	{
		if (string.IsNullOrEmpty(slug)) return false;
		bool expectAlnum = true;
		for (int i = 0; i < slug.Length; i++)
		{
			var c = slug[i];
			if (expectAlnum)
			{
				if (!IsLowerAsciiAlnum(c)) return false;
				expectAlnum = false;
			}
			else
			{
				if (IsLowerAsciiAlnum(c)) continue;
				if (c == '-') { expectAlnum = true; continue; }
				return false;
			}
		}
		return !expectAlnum; // no trailing hyphen
	}

	private static bool IsLowerAsciiAlnum(char c) =>
		(c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');

	/// <summary>
	/// Returns a repo-relative path anchored at the first <c>/Views/</c> segment,
	/// or the bare filename as a fallback.  Returns <c>null</c> for empty or in-memory paths.
	/// </summary>
	private static string? ComputeSourcePath(string? filePath)
	{
		if (filePath is null || filePath.Length == 0) return null;
		var normalized = filePath.Replace('\\', '/');
		var idx = normalized.IndexOf("/Views/", StringComparison.OrdinalIgnoreCase);
		if (idx >= 0) return normalized.Substring(idx + 1);
		var lastSlash = normalized.LastIndexOf('/');
		return lastSlash >= 0 ? normalized.Substring(lastSlash + 1) : normalized;
	}

	/// <summary>
	/// Returns <c>true</c> when <paramref name="type"/> is not abstract and has at least one
	/// parameterless constructor accessible from the same assembly (public, internal, or
	/// protected-internal).  The generated factory lambda <c>static () => new T()</c> requires
	/// exactly this — it does not subclass <c>T</c>, so <c>protected</c>-only constructors are
	/// not reachable.
	/// </summary>
	private static bool IsInstantiable(INamedTypeSymbol type) =>
		!type.IsAbstract && HasAccessibleParameterlessCtor(type);

	private static bool HasAccessibleParameterlessCtor(INamedTypeSymbol type)
	{
		foreach (var ctor in type.Constructors)
		{
			if (ctor.Parameters.Length == 0 && IsAccessibleFromSameAssembly(ctor.DeclaredAccessibility))
				return true;
		}
		return false;
	}

	// Accessible from the same assembly: public, internal, or protected-internal (C# `protected internal`).
	// `protected` alone and `private protected` require a subclass context that the generated lambda lacks.
	private static bool IsAccessibleFromSameAssembly(Accessibility accessibility) =>
		accessibility is Accessibility.Public
			or Accessibility.Internal
			or Accessibility.ProtectedOrInternal;
}
