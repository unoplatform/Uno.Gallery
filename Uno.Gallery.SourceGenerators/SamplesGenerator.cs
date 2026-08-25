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
		string? SourcePath);

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
			   $" RelatedSamples = {relatedLiteral} }}";
	}

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
		var title = (string)samplePageAttribute.ConstructorArguments[1].Value!;
		var source = $"(global::Uno.Gallery.SourceSdk)({((int)samplePageAttribute.ConstructorArguments[2].Value!).ToString(CultureInfo.InvariantCulture)})";
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

		var statusValue = GetNamedEnumIntOrDefault(samplePageAttribute, "Status", 0);
		var owner = GetNamedArgumentOrDefault<string>(samplePageAttribute, "Owner", null);
		var reviewedOn = GetNamedArgumentOrDefault<string>(samplePageAttribute, "ReviewedOn", null);
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
			sourcePath));
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
			{
				var rawValue = namedArgument.Value.Value;
				if (rawValue is null) return defaultValue;
				return (T)rawValue;
			}
		}
		return defaultValue;
	}

	private static int GetNamedEnumIntOrDefault(AttributeData attr, string name, int defaultValue)
	{
		foreach (var named in attr.NamedArguments)
		{
			if (named.Key != name) continue;
			if (named.Value.Value is null) return defaultValue;
			// Convert.ToInt32 handles both a boxed-int and a boxed-enum-type value safely.
			return Convert.ToInt32(named.Value.Value);
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
