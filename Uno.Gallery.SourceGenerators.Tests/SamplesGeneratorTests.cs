using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Uno.Gallery.SourceGenerators;

namespace Uno.Gallery.SourceGenerators.Tests;

/// <summary>
/// Source-generator unit tests for <see cref="SamplesGenerator"/>.
///
/// Each test drives the generator through <see cref="CSharpGeneratorDriver"/> over a small
/// in-memory compilation, verifying either generated source text or Roslyn diagnostics.
/// Stubs for the Uno.Gallery entity types are provided inline so the tests have no dependency
/// on the app project itself.
/// </summary>
[TestFixture]
public sealed class SamplesGeneratorTests
{
	// ─── Shared stubs ────────────────────────────────────────────────────────
	// A minimal definition of every type the generator references by name.
	// All stubs live in namespace Uno.Gallery so the generator's ForAttributeWithMetadataName
	// lookup ("Uno.Gallery.SamplePageAttribute") resolves correctly.

	private const string GoodStubs = """
		using System;

		namespace Uno.Gallery
		{
		    public enum SampleCategory { Controls = 0, Layout = 1, Media = 2 }
		    public enum SourceSdk { WinUI = 0, UWP = 1 }

		    [Flags]
		    public enum SampleConditionals : uint
		    {
		        Windows    = 1 << 0,
		        Wasm       = 1 << 1,
		        SkiaDesktop = 1 << 2,
		        Droid      = 1 << 3,
		        iOS        = 1 << 4,
		        macOS      = 1 << 5,
		        Desktop    = Windows | Wasm | SkiaDesktop | macOS,
		        Mobile     = Droid | iOS,
		        SkiaBased  = Wasm | SkiaDesktop,
		        Disabled   = 1U << 31,
		        Always     = uint.MaxValue ^ Disabled,
		    }

		    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
		    public sealed class SamplePageAttribute : Attribute
		    {
		        public SamplePageAttribute(SampleCategory category, string title,
		            SourceSdk source = SourceSdk.WinUI, string glyph = "")
		        {
		            Category = category; Title = title; Source = source; Glyph = glyph;
		        }
		        public SampleCategory Category { get; }
		        public string Title { get; }
		        public SourceSdk Source { get; }
		        public string Glyph { get; }
		        public string? Description { get; set; }
		        public string? DocumentationLink { get; set; }
		        public Type? DataType { get; set; }
		        public int SortOrder { get; set; } = int.MaxValue;
		    }

		    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
		    public class SampleConditionalAttribute : Attribute
		    {
		        public SampleConditionalAttribute(SampleConditionals conditionals) =>
		            Conditionals = conditionals;
		        public SampleConditionals Conditionals { get; }
		    }

		    public partial class App { }
		    public class Sample { public Sample(SamplePageAttribute a, Type t) { } }
		}
		""";

	// ─── Helpers ─────────────────────────────────────────────────────────────

	/// <summary>Runs <see cref="SamplesGenerator"/> on <paramref name="sources"/> (plus common stubs).</summary>
	private static GeneratorRunResult RunGenerator(
		IEnumerable<string> sources,
		IEnumerable<string>? preprocessorSymbols = null)
	{
		var parseOptions = preprocessorSymbols is not null
			? CSharpParseOptions.Default.WithPreprocessorSymbols(preprocessorSymbols)
			: CSharpParseOptions.Default;

		var trees = new[] { GoodStubs }
			.Concat(sources)
			.Select(s => CSharpSyntaxTree.ParseText(s, parseOptions));

		var compilation = CSharpCompilation.Create(
			"TestCompilation",
			trees,
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		GeneratorDriver driver = CSharpGeneratorDriver.Create(new SamplesGenerator());
		driver = driver.WithUpdatedParseOptions(parseOptions);
		driver = driver.RunGenerators(compilation);

		return driver.GetRunResult().Results.Single();
	}

	/// <summary>Like <see cref="RunGenerator"/> but uses <paramref name="stubs"/> instead of <see cref="GoodStubs"/>.</summary>
	private static GeneratorRunResult RunGeneratorWithStubs(
		string stubs,
		IEnumerable<string> sources,
		IEnumerable<string>? preprocessorSymbols = null)
	{
		var parseOptions = preprocessorSymbols is not null
			? CSharpParseOptions.Default.WithPreprocessorSymbols(preprocessorSymbols)
			: CSharpParseOptions.Default;

		var trees = new[] { stubs }
			.Concat(sources)
			.Select(s => CSharpSyntaxTree.ParseText(s, parseOptions));

		var compilation = CSharpCompilation.Create(
			"TestCompilation",
			trees,
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		GeneratorDriver driver = CSharpGeneratorDriver.Create(new SamplesGenerator());
		driver = driver.WithUpdatedParseOptions(parseOptions);
		driver = driver.RunGenerators(compilation);

		return driver.GetRunResult().Results.Single();
	}

	private static IEnumerable<MetadataReference> GetMetadataReferences()
	{
		var paths = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
		if (paths is null)
			yield break;

		foreach (var p in paths.Split(Path.PathSeparator))
			yield return MetadataReference.CreateFromFile(p);
	}

	private static string GetGeneratedSource(GeneratorRunResult result)
	{
		var entry = result.GeneratedSources
			.FirstOrDefault(s => s.HintName == "App.Samples.g.cs");
		return entry.SourceText?.ToString() ?? string.Empty;
	}

	private static ImmutableArray<Diagnostic> UggDiagnostics(GeneratorRunResult result) =>
		result.Diagnostics.Where(d => d.Id.StartsWith("UGG", StringComparison.Ordinal)).ToImmutableArray();

	// ─── Tests ───────────────────────────────────────────────────────────────

	[Test]
	public void Valid_SamplePage_emits_GetSamples_and_typeof_entry()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Sample", SourceSdk.WinUI, "\uE8FA")]
			    public class MySamplePage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null, "Generator must not throw");
		Assert.That(UggDiagnostics(result), Is.Empty, "No UGG diagnostics expected for valid input");

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("public static Sample[] GetSamples()"),
			"Generated file must declare GetSamples()");
		Assert.That(generated, Does.Contain("typeof(Uno.Gallery.MySamplePage)"),
			"Generated file must contain typeof for the attributed class");
		Assert.That(generated, Does.Contain("My Sample"),
			"Title must appear in generated attribute");
	}

	[Test]
	public void Valid_named_args_appear_in_generated_attribute()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Named Args Sample", SortOrder = 5,
			                Description = "A description", DocumentationLink = "https://example.com")]
			    public class NamedArgSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("SortOrder = 5"), "SortOrder named arg must be emitted");
		Assert.That(generated, Does.Contain("A description"), "Description must be emitted");
		Assert.That(generated, Does.Contain("https://example.com"), "DocumentationLink must be emitted");
	}

	[Test]
	public void SampleConditional_Disabled_filters_sample_from_output()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Disabled Sample")]
			    [SampleConditional(SampleConditionals.Disabled)]
			    public class DisabledSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		// Sample is excluded by the Disabled flag — generated file should be empty or omit the class.
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Not.Contain("DisabledSample"),
			"Disabled sample must not appear in generated output");
	}

	[Test]
	public void SampleConditional_Windows_included_when_WINDOWS_symbol_set()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Windows Only")]
			    [SampleConditional(SampleConditionals.Windows)]
			    public class WindowsSample { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols: ["WINDOWS"]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain("WindowsSample"),
			"Windows-only sample must be emitted when WINDOWS is defined");
	}

	[Test]
	public void SampleConditional_Windows_excluded_without_platform_symbol()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Windows Only")]
			    [SampleConditional(SampleConditionals.Windows)]
			    public class WindowsSampleNoSymbol { }
			}
			""";

		var result = RunGenerator([source]); // No preprocessor symbols → GetSampleConditionalsFromPreprocessorSymbolNames
		                                     // returns SampleConditionals.Always (no known platform detected).
		                                     // ShouldBeDisplayed checks conditionals.Value.HasFlag(compilationConditionals),
		                                     // i.e. Windows.HasFlag(Always).  HasFlag returns true iff all bits of
		                                     // compilationConditionals are set in conditionals; since Always has many
		                                     // more bits than Windows (Always ⊄ Windows), the check is false and the
		                                     // sample is excluded.

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("WindowsSampleNoSymbol"),
			"Windows-only sample must be excluded when no WINDOWS symbol is defined");
	}

	[Test]
	public void Malformed_SamplePage_constructor_arg_count_produces_UGG0001()
	{
		// Define SamplePageAttribute with a 3-param constructor (missing 'glyph').
		// The C# compiler in the test compilation accepts it; our generator should detect
		// the unexpected shape and emit UGG0001 without throwing.
		const string stubs = """
			using System;
			namespace Uno.Gallery
			{
			    public enum SampleCategory { Controls = 0 }
			    public enum SourceSdk { WinUI = 0 }
			    [Flags] public enum SampleConditionals : uint { Disabled = 1U << 31, Always = uint.MaxValue ^ Disabled }

			    // Three-param constructor — 'glyph' is absent, triggering UGG0001.
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class SamplePageAttribute : Attribute
			    {
			        public SamplePageAttribute(SampleCategory category, string title, SourceSdk source) { }
			        public int SortOrder { get; set; } = int.MaxValue;
			    }

			    public partial class App { }
			}
			""";

		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Bad Sample", SourceSdk.WinUI)]
			    public class BadSample { }
			}
			""";

		var result = RunGeneratorWithStubs(stubs, [source]);

		Assert.That(result.Exception, Is.Null, "Generator must not throw on unexpected shape");
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0001"), Is.True,
			"UGG0001 must be emitted for a 3-param constructor");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("BadSample"),
			"Malformed sample must not appear in generated output");
	}

	[Test]
	public void Malformed_SamplePage_wrong_parameter_names_produces_UGG0001()
	{
		// Constructor has 4 params but wrong names, verifying name-based check.
		const string stubs = """
			using System;
			namespace Uno.Gallery
			{
			    public enum SampleCategory { Controls = 0 }
			    public enum SourceSdk { WinUI = 0 }
			    [Flags] public enum SampleConditionals : uint { Disabled = 1U << 31, Always = uint.MaxValue ^ Disabled }

			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class SamplePageAttribute : Attribute
			    {
			        // Wrong names: 'cat', 'name', 'sdk', 'icon' instead of category/title/source/glyph
			        public SamplePageAttribute(SampleCategory cat, string name, SourceSdk sdk, string icon) { }
			    }

			    public partial class App { }
			}
			""";

		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Bad Names", SourceSdk.WinUI, "")]
			    public class WrongNameSample { }
			}
			""";

		var result = RunGeneratorWithStubs(stubs, [source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0001"), Is.True,
			"UGG0001 must be emitted for wrong parameter names");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("WrongNameSample"));
	}

	[Test]
	public void Non_class_target_produces_UGG0002()
	{
		// ForAttributeWithMetadataName can surface any attributed declared symbol — not only classes.
		// Here SamplePageAttribute is redefined with AttributeTargets.Method so the C# compiler
		// permits it on a method.  The generator receives an IMethodSymbol (not INamedTypeSymbol)
		// and must emit UGG0002 rather than throwing.
		const string stubs = """
			using System;
			namespace Uno.Gallery
			{
			    public enum SampleCategory { Controls = 0 }
			    public enum SourceSdk { WinUI = 0 }
			    [Flags] public enum SampleConditionals : uint { Disabled = 1U << 31, Always = uint.MaxValue ^ Disabled }

			    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
			    public sealed class SamplePageAttribute : Attribute
			    {
			        public SamplePageAttribute(SampleCategory category, string title,
			            SourceSdk source = SourceSdk.WinUI, string glyph = "") { }
			    }

			    public partial class App { }
			}
			""";

		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class ContainerClass
			    {
			        [SamplePage(SampleCategory.Controls, "Method Target")]
			        public void NotAClass() { }
			    }
			}
			""";

		var result = RunGeneratorWithStubs(stubs, [source]);

		Assert.That(result.Exception, Is.Null, "Generator must not throw on non-class target");
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0002"), Is.True,
			"UGG0002 must be emitted when attribute is on a method");
	}

	[Test]
	public void Malformed_SampleConditional_constructor_produces_UGG0003()
	{
		// Define SampleConditionalAttribute with a 2-arg constructor.
		// The generator checks ConstructorArguments.Length == 1 and emits UGG0003.
		const string stubs = """
			using System;
			namespace Uno.Gallery
			{
			    public enum SampleCategory { Controls = 0 }
			    public enum SourceSdk { WinUI = 0 }
			    [Flags] public enum SampleConditionals : uint
			    {
			        Windows = 1 << 0,
			        Wasm    = 1 << 1,
			        Disabled = 1U << 31,
			        Always  = uint.MaxValue ^ Disabled,
			    }

			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class SamplePageAttribute : Attribute
			    {
			        public SamplePageAttribute(SampleCategory category, string title,
			            SourceSdk source = SourceSdk.WinUI, string glyph = "") { }
			        public int SortOrder { get; set; } = int.MaxValue;
			    }

			    // Two-arg constructor — generator expects exactly one argument.
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public class SampleConditionalAttribute : Attribute
			    {
			        public SampleConditionalAttribute(SampleConditionals a, SampleConditionals b) { }
			    }

			    public partial class App { }
			}
			""";

		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Conditional Sample")]
			    [SampleConditional(SampleConditionals.Windows, SampleConditionals.Wasm)]
			    public class BadConditionalSample { }
			}
			""";

		var result = RunGeneratorWithStubs(stubs, [source]);

		Assert.That(result.Exception, Is.Null, "Generator must not throw on malformed SampleConditional");
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0003"), Is.True,
			"UGG0003 must be emitted for a two-arg SampleConditionalAttribute");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("BadConditionalSample"),
			"Sample with malformed SampleConditional must not appear in output");
	}

	[Test]
	public void Duplicate_sample_title_produces_UGG0004_warning()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Duplicate Title")]
			    public class FirstDuplicate { }

			    [SamplePage(SampleCategory.Layout, "Duplicate Title")]
			    public class SecondDuplicate { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);

		var ugg0004 = UggDiagnostics(result).Where(d => d.Id == "UGG0004").ToList();
		Assert.That(ugg0004, Is.Not.Empty, "UGG0004 warning must be emitted for duplicate title");
		Assert.That(ugg0004[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning), "UGG0004 must be a Warning");

		// Location must be navigable — the diagnostic points to the later duplicate's declaration.
		Assert.That(ugg0004[0].Location, Is.Not.EqualTo(Location.None),
			"UGG0004 must carry a real source location");
		Assert.That(ugg0004[0].Location.SourceSpan.IsEmpty, Is.False,
			"UGG0004 location must span real source text");
		// SyntaxTree must be non-null so the location is in-compilation and
		// #pragma warning disable UGG0004 at the declaration site will suppress the warning.
		Assert.That(ugg0004[0].Location.SourceTree, Is.Not.Null,
			"UGG0004 location must have a SyntaxTree reference for pragma-disable to work");

		// Message must identify the title and the first-seen conflicting type.
		// Sorted by FQN: FirstDuplicate (F) < SecondDuplicate (S), so the diagnostic lands
		// on SecondDuplicate and names FirstDuplicate in the message.
		var msg = ugg0004[0].GetMessage();
		Assert.That(msg, Does.Contain("Duplicate Title"), "Message must include the duplicate title");
		Assert.That(msg, Does.Contain("FirstDuplicate"), "Message must name the earlier conflicting type");

		// Both samples ARE emitted; duplicate detection is warn-only.
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("FirstDuplicate"), "Both duplicates must still appear in output");
		Assert.That(generated, Does.Contain("SecondDuplicate"));
	}

	[Test]
	public void Generated_output_is_deterministic_across_two_independent_runs()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Determinism Check", SortOrder = 3)]
			    public class DeterministicSample { }
			}
			""";

		var r1 = RunGenerator([source]);
		var r2 = RunGenerator([source]);

		var text1 = GetGeneratedSource(r1);
		var text2 = GetGeneratedSource(r2);

		Assert.That(text1, Is.Not.Empty, "First run must produce output");
		Assert.That(text1, Is.EqualTo(text2), "Two independent runs must produce byte-identical output");
	}

	[Test]
	public void IsExpectedSamplePageAttributeShape_validates_correct_shape()
	{
		// Valid: four correctly-named params.
		var validCompilation = CSharpCompilation.Create("x",
			[CSharpSyntaxTree.ParseText("""
				namespace Uno.Gallery
				{
				    public enum SampleCategory { Controls }
				    public enum SourceSdk { WinUI }
				    public class SamplePageAttribute : System.Attribute
				    {
				        public SamplePageAttribute(SampleCategory category, string title,
				            SourceSdk source, string glyph) { }
				    }
				}
				""")],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var attrSymbol = (INamedTypeSymbol)validCompilation.GetTypeByMetadataName("Uno.Gallery.SamplePageAttribute")!;
		var ctor = attrSymbol.Constructors.Single();

		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(ctor), Is.True,
			"Correct shape must be recognized as valid");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(null), Is.False,
			"Null constructor must be invalid");
	}

	[Test]
	public void IsExpectedSamplePageAttributeShape_rejects_wrong_count_and_names()
	{
		const string attrBase = """
			namespace Uno.Gallery
			{
			    public enum SampleCategory { Controls }
			    public enum SourceSdk { WinUI }
			    public class SamplePageAttribute : System.Attribute
			    {
			        public SamplePageAttribute(PARAMS) { }
			    }
			}
			""";

		static IMethodSymbol GetCtor(IEnumerable<MetadataReference> refs, string paramList)
		{
			var code = attrBase.Replace("PARAMS", paramList);
			var comp = CSharpCompilation.Create("x",
				[CSharpSyntaxTree.ParseText(code)],
				refs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			return ((INamedTypeSymbol)comp.GetTypeByMetadataName("Uno.Gallery.SamplePageAttribute")!)
				.Constructors.Single();
		}

		var refs = GetMetadataReferences().ToList();

		// Three params (missing glyph): wrong count → false
		var ctor3 = GetCtor(refs, "SampleCategory category, string title, SourceSdk source");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(ctor3), Is.False,
			"3 params must be rejected");

		// Five params: wrong count → false
		var ctor5 = GetCtor(refs, "SampleCategory category, string title, SourceSdk source, string glyph, int extra");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(ctor5), Is.False,
			"5 params must be rejected");

		// Four params, wrong names (cat/name/sdk/icon): wrong names → false
		var ctorWrongNames = GetCtor(refs, "SampleCategory cat, string name, SourceSdk sdk, string icon");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(ctorWrongNames), Is.False,
			"4 params with wrong names must be rejected");

		// Four params, correct count but first name wrong: → false
		var ctorFirstWrong = GetCtor(refs, "SampleCategory kind, string title, SourceSdk source, string glyph");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(ctorFirstWrong), Is.False,
			"First param name mismatch must be rejected");
	}

	[Test]
	public void Generated_output_is_deterministic_regardless_of_file_order()
	{
		// Two classes in separate source strings; order A-B vs B-A must produce identical output.
		// The generator sorts by FullyQualifiedName so file-order has no effect on output.
		const string sourceZebra = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Zebra Sample")]
			    public class ZebraSample { }
			}
			""";
		const string sourceApple = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Layout, "Apple Sample")]
			    public class AppleSample { }
			}
			""";

		var r1 = RunGenerator([sourceZebra, sourceApple]);
		var r2 = RunGenerator([sourceApple, sourceZebra]);

		var text1 = GetGeneratedSource(r1);
		var text2 = GetGeneratedSource(r2);

		Assert.That(text1, Is.Not.Empty, "Generator must produce output");
		Assert.That(text1, Is.EqualTo(text2), "Output must be byte-identical regardless of syntax-tree order");

		// FQN sort: Uno.Gallery.AppleSample (A) before Uno.Gallery.ZebraSample (Z)
		var idxApple = text1.IndexOf("AppleSample", StringComparison.Ordinal);
		var idxZebra = text1.IndexOf("ZebraSample", StringComparison.Ordinal);
		Assert.That(idxApple, Is.GreaterThanOrEqualTo(0), "AppleSample must appear in output");
		Assert.That(idxApple, Is.LessThan(idxZebra), "AppleSample (A) must precede ZebraSample (Z) in sorted output");
	}

	// ─── Parameterized platform-conditional tests ─────────────────────────────
	// Covers WASM, SkiaDesktop, Android, iOS, and macOS using the real preprocessor symbols and
	// SampleConditionals flags that production code maps.  Windows inclusion/exclusion is already
	// covered by the dedicated tests above.

	[TestCase("__WASM__",      "Wasm",        "WasmSample")]
	[TestCase("HAS_UNO_SKIA", "SkiaDesktop", "SkiaSample")]
	[TestCase("__ANDROID__",  "Droid",       "AndroidSample")]
	[TestCase("__IOS__",      "iOS",         "IosSample")]
	[TestCase("__MACOS__",    "macOS",       "MacosSample")]
	public void SampleConditional_platform_included_when_symbol_defined(
		string preprocessorSymbol, string conditionalName, string className)
	{
		var source = $$"""
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "{{className}} Title")]
			    [SampleConditional(SampleConditionals.{{conditionalName}})]
			    public class {{className}} { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols: [preprocessorSymbol]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain(className),
			$"{className} must be emitted when {preprocessorSymbol} is defined");
	}

	[TestCase("__WASM__",      "Wasm",        "WasmSampleExcl")]
	[TestCase("HAS_UNO_SKIA", "SkiaDesktop", "SkiaSampleExcl")]
	[TestCase("__ANDROID__",  "Droid",       "AndroidSampleExcl")]
	[TestCase("__IOS__",      "iOS",         "IosSampleExcl")]
	[TestCase("__MACOS__",    "macOS",       "MacosSampleExcl")]
	public void SampleConditional_platform_excluded_when_other_symbol_defined(
		string ownSymbol, string conditionalName, string className)
	{
		// Run with a different platform symbol so the target platform is not active.
		// WINDOWS is an unrelated platform symbol that doesn't match any of the tested flags.
		var source = $$"""
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "{{className}} Title")]
			    [SampleConditional(SampleConditionals.{{conditionalName}})]
			    public class {{className}} { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols: ["WINDOWS"]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain(className),
			$"{className} must be excluded when {ownSymbol} is not the active platform");
	}

	[Test]
	public void Duplicate_sample_title_case_insensitive_produces_UGG0004()
	{
		// Runtime navigation/search compares titles case-insensitively; the generator must
		// detect "Button" vs "button" as a duplicate (OrdinalIgnoreCase).
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Button")]
			    public class ButtonSampleA { }

			    [SamplePage(SampleCategory.Layout, "button")]
			    public class ButtonSampleB { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0004"), Is.True,
			"UGG0004 must be emitted for case-insensitively duplicate titles (\"Button\" vs \"button\")");
	}

	[Test]
	public void Duplicate_sample_title_pragma_disable_suppresses_UGG0004()
	{
		// #pragma warning disable UGG0004 at the duplicate declaration must suppress the warning.
		// This requires the diagnostic location to carry a real SyntaxTree reference
		// (not a path-only Location.Create) so the Roslyn driver can match it to the pragma.
		// Roslyn marks pragma-suppressed diagnostics with IsSuppressed=true rather than
		// removing them from the collection — both states are tested here.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Shared Title")]
			    public class FirstSample { }

			#pragma warning disable UGG0004
			    [SamplePage(SampleCategory.Layout, "Shared Title")]
			    public class SecondSample { }
			#pragma warning restore UGG0004
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0004 = UggDiagnostics(result).Where(d => d.Id == "UGG0004").ToList();
		// Exactly one duplicate diagnostic exists (FirstSample < SecondSample in FQN order,
		// so the diagnostic lands on SecondSample).
		Assert.That(ugg0004, Has.Count.EqualTo(1), "Exactly one UGG0004 diagnostic expected");
		// The pragma covers SecondSample's declaration — the diagnostic must be suppressed.
		Assert.That(ugg0004[0].IsSuppressed, Is.True,
			"#pragma warning disable UGG0004 must mark the diagnostic as suppressed (IsSuppressed=true)");
	}
}
