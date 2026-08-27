using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Uno.Gallery.SourceGenerators;

namespace Uno.Gallery.SourceGenerators.Tests;

/// <summary>
/// Source-generator unit tests for <see cref="SamplesGenerator"/> and slug algorithm.
/// Generator tests drive the generator through <see cref="CSharpGeneratorDriver"/> over a small
/// in-memory compilation.  Slug tests validate <see cref="SlugHelper.DeriveSlug"/> directly
/// via the linked shared source file.
/// </summary>
[TestFixture]
public sealed class SamplesGeneratorTests
{
	// ─── Shared stubs ────────────────────────────────────────────────────────

	private const string GoodStubs = """
		using System;

		namespace Uno.Gallery
		{
		    public enum SampleCategory { Controls = 0, Layout = 1, Media = 2 }
		    public enum SourceSdk { WinUI = 0, UWP = 1 }
		    public enum SampleStatus { Stable = 0, Preview = 1, Experimental = 2, Deprecated = 3, Incomplete = 4 }
		    [Flags]
		    public enum SampleDesigns { None = 0, Material = 1, Fluent = 2, Cupertino = 4, Native = 8, Agnostic = 16 }
		    [Flags]
		    public enum SampleRenderers { None = 0, Native = 1, Skia = 2, DOM = 4 }

		    [Flags]
		    public enum SampleConditionals : uint
		    {
		        Windows    = 1 << 0,
		        Wasm       = 1 << 1,
		        SkiaDesktop = 1 << 2,
		        Droid      = 1 << 3,
		        iOS        = 1 << 4,
		        macOS      = 1 << 5,
		        SkiaRenderer = 1 << 6,
		        NativeRenderer = 1 << 7,
		        Desktop    = Windows | Wasm | SkiaDesktop | macOS,
		        Mobile     = Droid | iOS,
		        SkiaBased  = Wasm | SkiaDesktop,
		        Renderer   = SkiaRenderer | NativeRenderer,
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
		        public string? Slug { get; set; }
		        public string[]? Tags { get; set; }
		        public SampleStatus Status { get; set; } = SampleStatus.Stable;
		        public string? Owner { get; set; }
		        public string? ReviewedOn { get; set; }
		        public string[]? RelatedSamples { get; set; }
		        public int ContractVersion { get; set; }
		        public SampleDesigns SupportedDesigns { get; set; }
		        public SampleRenderers SupportedRenderers { get; set; }
		        public string[]? Requirements { get; set; }
		        public string[]? AccessibilityNotes { get; set; }
		        public string? ResetBehavior { get; set; }
		        public string[]? Variants { get; set; }
		        public string[]? KnownLimitations { get; set; }
		        public string? IssueLink { get; set; }
		        public string? ApiLink { get; set; }
		    }

		    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
		    public class SampleConditionalAttribute : Attribute
		    {
		        public SampleConditionalAttribute(SampleConditionals conditionals) =>
		            Conditionals = conditionals;
		        public SampleConditionals Conditionals { get; }
		    }

		    public class Page { }
		    public partial class App { }
		    public class Sample
		    {
		        public Sample(SamplePageAttribute a, Type t) { }
		        // Public here because the stubs compile in a separate test assembly; the production ctor is
		        // internal — generated code and the real Sample type share the Uno.Gallery assembly, so the
		        // internal ctor is always reachable from generated output in production.
		        public Sample(SamplePageAttribute a, Type t, Func<Page> pageFactory, Func<object?>? dataFactory) { }
		        public string? SourcePath { get; internal set; }
		    }
		}
		""";

	// ─── Helpers ─────────────────────────────────────────────────────────────

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

	private static GeneratorRunResult RunGeneratorWithFilePaths(
		IEnumerable<(string Source, string Path)> sourcesWithPaths,
		IEnumerable<string>? preprocessorSymbols = null)
	{
		var parseOptions = preprocessorSymbols is not null
			? CSharpParseOptions.Default.WithPreprocessorSymbols(preprocessorSymbols)
			: CSharpParseOptions.Default;

		var trees = new[] { (GoodStubs, "") }
			.Concat(sourcesWithPaths)
			.Select(t => CSharpSyntaxTree.ParseText(t.Item1, parseOptions, path: t.Item2));

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

	// ─── Existing generator tests (preserved) ────────────────────────────────

	[Test]
	public void Valid_SamplePage_emits_GetSamples_and_typeof_entry()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Sample", SourceSdk.WinUI, "\uE8FA")]
			    public class MySamplePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("public static Sample[] GetSamples()"));
		Assert.That(generated, Does.Contain("typeof(Uno.Gallery.MySamplePage)"));
		Assert.That(generated, Does.Contain("My Sample"));
		AssertGeneratedCompiles(generated, source);
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
			    public class NamedArgSample : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("SortOrder = 5"));
		Assert.That(generated, Does.Contain("A description"));
		Assert.That(generated, Does.Contain("https://example.com"));
		AssertGeneratedCompiles(generated, source);
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
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("DisabledSample"));
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
			    public class WindowsSample : Page { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols: ["WINDOWS"]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain("WindowsSample"));
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

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("WindowsSampleNoSymbol"));
	}

	[Test]
	public void Malformed_SamplePage_constructor_arg_count_produces_UGG0001()
	{
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

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0001"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("BadSample"));
	}

	[Test]
	public void Malformed_SamplePage_wrong_parameter_names_produces_UGG0001()
	{
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
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0001"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("WrongNameSample"));
	}

	[Test]
	public void Non_class_target_produces_UGG0002()
	{
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

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0002"), Is.True);
	}

	[Test]
	public void Malformed_SampleConditional_constructor_produces_UGG0003()
	{
		const string stubs = """
			using System;
			namespace Uno.Gallery
			{
			    public enum SampleCategory { Controls = 0 }
			    public enum SourceSdk { WinUI = 0 }
			    [Flags] public enum SampleConditionals : uint
			    {
			        Windows = 1 << 0, Wasm = 1 << 1,
			        Disabled = 1U << 31, Always = uint.MaxValue ^ Disabled,
			    }

			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class SamplePageAttribute : Attribute
			    {
			        public SamplePageAttribute(SampleCategory category, string title,
			            SourceSdk source = SourceSdk.WinUI, string glyph = "") { }
			        public int SortOrder { get; set; } = int.MaxValue;
			    }

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

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0003"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("BadConditionalSample"));
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
		Assert.That(ugg0004, Is.Not.Empty);
		Assert.That(ugg0004[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
		Assert.That(ugg0004[0].Location, Is.Not.EqualTo(Location.None));
		Assert.That(ugg0004[0].Location.SourceSpan.IsEmpty, Is.False);
		Assert.That(ugg0004[0].Location.SourceTree, Is.Not.Null);

		var msg = ugg0004[0].GetMessage();
		Assert.That(msg, Does.Contain("Duplicate Title"));
		Assert.That(msg, Does.Contain("FirstDuplicate"));

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("FirstDuplicate"));
		Assert.That(generated, Does.Contain("SecondDuplicate"));
	}

	[Test]
	public void Duplicate_sample_title_case_insensitive_produces_UGG0004()
	{
		// "Button" and "button" differ only in case; OrdinalIgnoreCase comparison must detect them.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Button")]
			    public class ButtonPage { }

			    [SamplePage(SampleCategory.Layout, "button")]
			    public class ButtonLowerPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0004 = UggDiagnostics(result).Where(d => d.Id == "UGG0004").ToList();
		Assert.That(ugg0004, Is.Not.Empty, "Titles differing only in case must produce UGG0004");
		Assert.That(ugg0004[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
		var msg = ugg0004[0].GetMessage();
		Assert.That(msg, Does.Contain("Button"), "Message must include the title");
		Assert.That(msg, Does.Contain("ButtonLowerPage"), "Message must reference the first-seen class");
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

		Assert.That(GetGeneratedSource(r1), Is.Not.Empty);
		Assert.That(GetGeneratedSource(r1), Is.EqualTo(GetGeneratedSource(r2)));
	}

	[Test]
	public void IsExpectedSamplePageAttributeShape_validates_correct_shape()
	{
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

		var ctor = ((INamedTypeSymbol)validCompilation
			.GetTypeByMetadataName("Uno.Gallery.SamplePageAttribute")!)
			.Constructors.Single();

		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(ctor), Is.True);
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(null), Is.False);
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

		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(
			GetCtor(refs, "SampleCategory category, string title, SourceSdk source")), Is.False,
			"3 params must be rejected");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(
			GetCtor(refs, "SampleCategory category, string title, SourceSdk source, string glyph, int extra")), Is.False,
			"5 params must be rejected");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(
			GetCtor(refs, "SampleCategory cat, string name, SourceSdk sdk, string icon")), Is.False,
			"Wrong names must be rejected");
		Assert.That(SamplesGenerator.IsExpectedSamplePageAttributeShape(
			GetCtor(refs, "SampleCategory kind, string title, SourceSdk source, string glyph")), Is.False,
			"First name mismatch must be rejected");
	}

	[Test]
	public void Generated_output_is_deterministic_regardless_of_file_order()
	{
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

		Assert.That(GetGeneratedSource(r1), Is.Not.Empty);
		Assert.That(GetGeneratedSource(r1), Is.EqualTo(GetGeneratedSource(r2)));

		var text = GetGeneratedSource(r1);
		Assert.That(text.IndexOf("AppleSample", StringComparison.Ordinal),
			Is.LessThan(text.IndexOf("ZebraSample", StringComparison.Ordinal)),
			"AppleSample (A) must precede ZebraSample (Z)");
	}

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
		Assert.That(GetGeneratedSource(result), Does.Contain(className));
	}

	[TestCase("__WASM__",      "Wasm",        "WasmSampleExcl")]
	[TestCase("HAS_UNO_SKIA", "SkiaDesktop", "SkiaSampleExcl")]
	[TestCase("__ANDROID__",  "Droid",       "AndroidSampleExcl")]
	[TestCase("__IOS__",      "iOS",         "IosSampleExcl")]
	[TestCase("__MACOS__",    "macOS",       "MacosSampleExcl")]
	public void SampleConditional_platform_excluded_when_other_symbol_defined(
		string ownSymbol, string conditionalName, string className)
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
		var result = RunGenerator([source], preprocessorSymbols: ["WINDOWS"]);
		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain(className));
	}

	[TestCase(new[] { "__WASM__", "HAS_SKIA_RENDERER" }, true)]
	[TestCase(new[] { "__WASM__" }, false)]
	[TestCase(new[] { "HAS_UNO_SKIA", "HAS_SKIA_RENDERER" }, true)]
	[TestCase(new[] { "WINDOWS", "HAS_SKIA_RENDERER" }, false)]
	public void SampleConditional_SkiaRenderer_requires_supported_platform_and_renderer(
		string[] preprocessorSymbols,
		bool shouldBeIncluded)
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Skia renderer")]
			    [SampleConditional(SampleConditionals.SkiaBased | SampleConditionals.SkiaRenderer)]
			    public class SkiaRendererSample { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols);
		var generated = GetGeneratedSource(result);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(generated.Contains("SkiaRendererSample"), Is.EqualTo(shouldBeIncluded));
	}

	[TestCase(new[] { "__WASM__", "HAS_SKIA_RENDERER" }, true)]
	[TestCase(new[] { "__WASM__" }, false)]
	[TestCase(new[] { "__ANDROID__", "HAS_SKIA_RENDERER" }, true)]
	public void SampleConditional_renderer_only_applies_across_platforms(
		string[] preprocessorSymbols,
		bool shouldBeIncluded)
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Skia renderer")]
			    [SampleConditional(SampleConditionals.SkiaRenderer)]
			    public class SkiaRendererSample { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols);

		Assert.That(result.Exception, Is.Null);
		Assert.That(
			GetGeneratedSource(result).Contains("SkiaRendererSample"),
			Is.EqualTo(shouldBeIncluded));
	}

	// ─── Phase 2: metadata forwarding ────────────────────────────────────────

	[Test]
	public void Derived_slug_emitted_from_title()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Control")]
			    public class MyControlPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain(@"Slug = @""my-control"""),
			"Slug derived from title must be emitted into the attribute");
	}

	[Test]
	public void Explicit_slug_forwarded_verbatim()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Renamed Control", Slug = "old-name")]
			    public class MyRenamedControlPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain(@"Slug = @""old-name"""),
			"Explicit slug must be forwarded without modification");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("my-renamed-control"),
			"Derived slug must not appear when explicit slug is set");
	}

	[Test]
	public void Status_preview_emitted_as_int_cast()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Preview Control", Status = SampleStatus.Preview)]
			    public class PreviewControlPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain("(global::Uno.Gallery.SampleStatus)(1)"),
			"SampleStatus.Preview (1) must be emitted as integer cast");
	}

	[Test]
	public void Default_status_stable_emitted_as_zero()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Button")]
			    public class StableControlPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result), Does.Contain("(global::Uno.Gallery.SampleStatus)(0)"),
			"Omitted Status must default to Stable (0)");
	}

	[Test]
	public void UGG0011_new_implicit_stable_requires_contract_v1()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "New Stable Control")]
			    public class NewStableControlPage : Page { }
			}
			""";

		var result = RunGenerator([source], ["ENFORCE_SAMPLE_CONTRACT"]);
		var diagnostic = UggDiagnostics(result).Single(d => d.Id == "UGG0011");

		Assert.That(diagnostic.GetMessage(), Does.Contain("ContractVersion (must be 1)"));
	}

	[Test]
	public void UGG0011_undefined_status_is_rejected()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Undefined Status", Status = (SampleStatus)99)]
			    public class UndefinedStatusPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var diagnostic = UggDiagnostics(result).Single(d => d.Id == "UGG0011");

		Assert.That(diagnostic.GetMessage(), Does.Contain("Status (undefined or inconsistent value 99/99)"));
	}

	[Test]
	public void Tags_emitted_as_array()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Tagged Control",
			                Tags = new[] { "input", "layout" })]
			    public class TaggedControlPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("\"input\""));
		Assert.That(generated, Does.Contain("\"layout\""));
		Assert.That(generated, Does.Contain("Tags = new[]"));
	}

	[Test]
	public void Empty_tags_emit_null()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "No Tags")]
			    public class NoTagsPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(GetGeneratedSource(result), Does.Contain("Tags = null"));
	}

	[Test]
	public void Owner_and_ReviewedOn_emitted()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Owned Control",
			                Owner = "alice", ReviewedOn = "2024-06-01")]
			    public class OwnedControlPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("alice"));
		Assert.That(generated, Does.Contain("2024-06-01"));
	}

	[Test]
	public void Null_owner_emits_null_literals()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "No Owner")]
			    public class NoOwnerPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("Owner = null"));
		Assert.That(generated, Does.Contain("ReviewedOn = null"));
	}

	[Test]
	public void RelatedSamples_emitted_as_array_no_UGG0007_for_known_slugs()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample A",
			                RelatedSamples = new[] { "sample-b", "sample-c" })]
			    public class SampleAPage { }

			    [SamplePage(SampleCategory.Controls, "Sample B")]
			    public class SampleBPage { }

			    [SamplePage(SampleCategory.Controls, "Sample C")]
			    public class SampleCPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Where(d => d.Id == "UGG0007"), Is.Empty,
			"Known slugs in RelatedSamples must not trigger UGG0007");
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("\"sample-b\""));
		Assert.That(generated, Does.Contain("\"sample-c\""));
	}

	[Test]
	public void SourcePath_emitted_for_Views_prefixed_file()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Control")]
			    public class MyControlSamplePage { }
			}
			""";

		var result = RunGeneratorWithFilePaths([
			(source, "/repo/Uno.Gallery/Views/SamplePages/MyControlSamplePage.xaml.cs")
		]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(GetGeneratedSource(result),
			Does.Contain("Views/SamplePages/MyControlSamplePage.xaml.cs"),
			"SourcePath must be anchored at Views/ with forward slashes");
	}

	[Test]
	public void SourcePath_normalizes_backslashes()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Control2")]
			    public class Control2SamplePage { }
			}
			""";

		var result = RunGeneratorWithFilePaths([
			(source, @"C:\repo\Uno.Gallery\Views\SamplePages\Control2SamplePage.xaml.cs")
		]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(GetGeneratedSource(result),
			Does.Contain("Views/SamplePages/Control2SamplePage.xaml.cs"));
	}

	[Test]
	public void SourcePath_absent_for_in_memory_tree()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "In Memory")]
			    public class InMemoryPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("SourcePath"),
			"SourcePath must be omitted when syntax tree has no file path");
	}

	// ─── Phase 2: UGG0005 ────────────────────────────────────────────────────

	[Test]
	public void Invalid_slug_uppercase_produces_UGG0005()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Sample", Slug = "My-Slug")]
			    public class BadSlugSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0005"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("BadSlugSample"));
	}

	[Test]
	public void Invalid_slug_leading_hyphen_produces_UGG0005()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Leading Hyphen", Slug = "-my-slug")]
			    public class LeadingHyphenSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0005"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("LeadingHyphenSample"));
	}

	[Test]
	public void Invalid_slug_double_hyphen_produces_UGG0005()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Double Hyphen", Slug = "my--slug")]
			    public class DoubleHyphenSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0005"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("DoubleHyphenSample"));
	}

	[Test]
	public void Invalid_slug_trailing_hyphen_produces_UGG0005()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Trailing Hyphen", Slug = "my-slug-")]
			    public class TrailingHyphenSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0005"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("TrailingHyphenSample"));
	}

	[Test]
	public void Valid_explicit_slug_no_UGG0005()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Control", Slug = "my-control")]
			    public class GoodSlugSample { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0005"), Is.False);
		Assert.That(GetGeneratedSource(result), Does.Contain("GoodSlugSample"));
	}

	[Test]
	public void UGG0005_message_identifies_the_bad_slug()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample", Slug = "Bad Slug!")]
			    public class BadSlugMsgSample { }
			}
			""";

		var result = RunGenerator([source]);
		var diag = UggDiagnostics(result).First(d => d.Id == "UGG0005");
		Assert.That(diag.GetMessage(), Does.Contain("Bad Slug!"));
	}

	// ─── Phase 2: UGG0006 ────────────────────────────────────────────────────

	[Test]
	public void Duplicate_derived_slug_produces_UGG0006()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Hello World")]
			    public class SlugFirstDuplicate { }

			    [SamplePage(SampleCategory.Layout, "hello world")]
			    public class SlugSecondDuplicate { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0006 = UggDiagnostics(result).Where(d => d.Id == "UGG0006").ToList();
		Assert.That(ugg0006, Is.Not.Empty);
		Assert.That(ugg0006[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
		Assert.That(ugg0006[0].Location.SourceTree, Is.Not.Null);

		var msg = ugg0006[0].GetMessage();
		Assert.That(msg, Does.Contain("hello-world"));
		Assert.That(msg, Does.Contain("SlugFirstDuplicate"));

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("SlugFirstDuplicate"));
		Assert.That(generated, Does.Contain("SlugSecondDuplicate"));
	}

	[Test]
	public void Duplicate_explicit_slug_produces_UGG0006()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Alpha", Slug = "shared-slug")]
			    public class AlphaPage { }

			    [SamplePage(SampleCategory.Layout, "Beta", Slug = "shared-slug")]
			    public class BetaPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0006"), Is.True);
		Assert.That(GetGeneratedSource(result), Does.Contain("AlphaPage"));
		Assert.That(GetGeneratedSource(result), Does.Contain("BetaPage"));
	}

	// ─── Phase 2: UGG0007 ────────────────────────────────────────────────────

	[Test]
	public void Unknown_related_slug_produces_UGG0007()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample A",
			                RelatedSamples = new[] { "nonexistent-slug" })]
			    public class SampleAWithBadRef { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0007 = UggDiagnostics(result).Where(d => d.Id == "UGG0007").ToList();
		Assert.That(ugg0007, Is.Not.Empty);
		Assert.That(ugg0007[0].Severity, Is.EqualTo(DiagnosticSeverity.Warning));
		Assert.That(ugg0007[0].GetMessage(), Does.Contain("nonexistent-slug"));
		Assert.That(GetGeneratedSource(result), Does.Contain("SampleAWithBadRef"));
	}

	[Test]
	public void Known_related_slug_no_UGG0007()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample X",
			                RelatedSamples = new[] { "sample-y" })]
			    public class SampleXPage { }

			    [SamplePage(SampleCategory.Controls, "Sample Y")]
			    public class SampleYPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0007"), Is.False);
	}

	[Test]
	public void UGG0007_sample_still_emits_with_dead_reference()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Has Dead Ref",
			                RelatedSamples = new[] { "dead-link" })]
			    public class HasDeadRefPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0007"), Is.True);
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("HasDeadRefPage"));
		Assert.That(generated, Does.Contain("\"dead-link\""));
	}

	// ─── Slug algorithm: DeriveSlug ──────────────────────────────────────────

	[Test] public void DeriveSlug_empty_returns_fallback() =>
		Assert.That(SlugHelper.DeriveSlug(""), Is.EqualTo("sample"));

	[Test] public void DeriveSlug_whitespace_only_returns_fallback() =>
		Assert.That(SlugHelper.DeriveSlug("   "), Is.EqualTo("sample"));

	[Test] public void DeriveSlug_all_separators_returns_fallback() =>
		Assert.That(SlugHelper.DeriveSlug("//---!!"), Is.EqualTo("sample"));

	[Test] public void DeriveSlug_lowercase_input_unchanged() =>
		Assert.That(SlugHelper.DeriveSlug("hello"), Is.EqualTo("hello"));

	[Test] public void DeriveSlug_uppercase_lowercased() =>
		Assert.That(SlugHelper.DeriveSlug("Hello"), Is.EqualTo("hello"));

	[Test] public void DeriveSlug_PascalCase_stays_one_word() =>
		Assert.That(SlugHelper.DeriveSlug("AutoSuggestBox"), Is.EqualTo("autosuggestbox"));

	[Test] public void DeriveSlug_spaces_become_single_hyphen() =>
		Assert.That(SlugHelper.DeriveSlug("Hello World"), Is.EqualTo("hello-world"));

	[Test] public void DeriveSlug_repeated_spaces_collapse() =>
		Assert.That(SlugHelper.DeriveSlug("Hello  World"), Is.EqualTo("hello-world"));

	[Test] public void DeriveSlug_leading_separator_trimmed() =>
		Assert.That(SlugHelper.DeriveSlug(" Hello"), Is.EqualTo("hello"));

	[Test] public void DeriveSlug_trailing_separator_trimmed() =>
		Assert.That(SlugHelper.DeriveSlug("Hello "), Is.EqualTo("hello"));

	[Test] public void DeriveSlug_leading_and_trailing_trimmed() =>
		Assert.That(SlugHelper.DeriveSlug("  Hello World  "), Is.EqualTo("hello-world"));

	[Test] public void DeriveSlug_slash_becomes_hyphen() =>
		Assert.That(SlugHelper.DeriveSlug("A/B"), Is.EqualTo("a-b"));

	[Test] public void DeriveSlug_mixed_separators_collapse() =>
		Assert.That(SlugHelper.DeriveSlug("A / B"), Is.EqualTo("a-b"));

	[Test] public void DeriveSlug_punctuation_becomes_hyphen() =>
		Assert.That(SlugHelper.DeriveSlug("Hello, World!"), Is.EqualTo("hello-world"));

	[Test] public void DeriveSlug_digits_preserved() =>
		Assert.That(SlugHelper.DeriveSlug("OAuth2 Login"), Is.EqualTo("oauth2-login"));

	[Test] public void DeriveSlug_non_ASCII_treated_as_separator()
	{
		// 'i' with diaeresis (U+00EF) is above U+007F — treated as a separator, not transliterated.
		// "Naive" with accent: N,a,i(non-ASCII),v,e => "na-ve"
		Assert.That(SlugHelper.DeriveSlug("Na\u00EFve"), Is.EqualTo("na-ve"),
			"Non-ASCII character must act as a word separator");
	}

	[Test] public void DeriveSlug_progress_ring_slash_bar() =>
		Assert.That(SlugHelper.DeriveSlug("Progress Ring/Bar"), Is.EqualTo("progress-ring-bar"));

	// ─── IsValidSlug parameterized cases ─────────────────────────────────────

	[TestCase("a",           true)]
	[TestCase("abc",         true)]
	[TestCase("abc-def",     true)]
	[TestCase("a1b2",        true)]
	[TestCase("my-control",  true)]
	[TestCase("",            false, Description = "empty")]
	[TestCase("-abc",        false, Description = "leading hyphen")]
	[TestCase("abc-",        false, Description = "trailing hyphen")]
	[TestCase("abc--def",    false, Description = "consecutive hyphens")]
	[TestCase("Abc",         false, Description = "uppercase")]
	[TestCase("abc def",     false, Description = "space")]
	[TestCase("abc/def",     false, Description = "slash")]
	[TestCase("-",           false, Description = "hyphen only")]
	public void IsValidSlug_validates(string slug, bool expected) =>
		Assert.That(SamplesGenerator.IsValidSlugPublicForTest(slug), Is.EqualTo(expected));

	// ─── Issue 1: positional Title/Glyph escaping ────────────────────────────

	[Test]
	public void Title_with_embedded_double_quote_emits_verbatim_escaped_literal()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Say \"Hello\"")]
			    public class QuotedTitlePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		// StringLiteral uses @"..." verbatim syntax; inner double-quotes are doubled.
		// "Say \"Hello\"" → @"Say ""Hello""  (each " doubled; entire arg is verbatim)
		Assert.That(generated, Does.Contain("@\"Say \"\"Hello\"\""),
			"Embedded double-quote in Title must be doubled inside verbatim literal");
		// The generated C# itself must compile successfully (round-trip check)
		AssertGeneratedCompiles(generated, source);
	}

	[Test]
	public void Glyph_with_backslash_emits_verbatim_literal_and_compiles()
	{
		// A glyph Unicode escape like \uE001 is a single char at source level
		// but some glyphs could be raw chars that include backslash-like sequences.
		// Verify the generator handles a glyph whose char value needs verbatim escaping.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Backslash Glyph", glyph: "\uE001")]
			    public class BackslashGlyphPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		// glyph is passed through StringLiteral (@"...") — verbatim strings handle all chars safely
		Assert.That(generated, Does.Contain("glyph:"),
			"Glyph must appear in the generated attribute");
		AssertGeneratedCompiles(generated, source);
	}

	[Test]
	public void Title_with_backslash_char_emits_and_compiles()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, @"Path\Separator")]
			    public class BackslashTitlePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		// In a verbatim string, backslash is literal — no escaping needed
		Assert.That(generated, Does.Contain(@"@""Path\Separator"""),
			"Backslash in Title must be preserved as-is inside verbatim literal");
		AssertGeneratedCompiles(generated, source);
	}

	[Test]
	public void Tags_with_special_chars_emit_and_compile()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Special Tags",
			                Tags = new[] { "c#", "has \"quotes\"" })]
			    public class SpecialTagsPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("Tags = new[]"),
			"Tags array must be emitted");
		// "c#" goes through StringLiteral → @"c#"
		Assert.That(generated, Does.Contain(@"@""c#"""),
			"Tag with special char must be emitted as verbatim literal");
		AssertGeneratedCompiles(generated, source);
	}

	// ─── Issue 2: StringSequence equality and hashing ────────────────────────

	[Test]
	public void StringSequence_empty_equal_to_default()
	{
		var seq1 = new StringSequenceTestHelper(ImmutableArray<string>.Empty);
		var seq2 = new StringSequenceTestHelper(ImmutableArray<string>.Empty);
		var def1 = new StringSequenceTestHelper(default(ImmutableArray<string>));
		Assert.That(seq1.Equals(seq2), Is.True, "two empty sequences must be equal");
		Assert.That(seq1.Equals(def1), Is.True, "empty equals default-initialised");
		Assert.That(seq1.GetHashCode(), Is.EqualTo(def1.GetHashCode()), "empty and default must have same hash");
	}

	[Test]
	public void StringSequence_same_elements_equal()
	{
		var a = new StringSequenceTestHelper(ImmutableArray.Create("x", "y"));
		var b = new StringSequenceTestHelper(ImmutableArray.Create("x", "y"));
		Assert.That(a.Equals(b), Is.True, "sequences with same ordinal elements must be equal");
		Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()), "equal sequences must have same hash");
	}

	[Test]
	public void StringSequence_different_order_not_equal()
	{
		var a = new StringSequenceTestHelper(ImmutableArray.Create("x", "y"));
		var b = new StringSequenceTestHelper(ImmutableArray.Create("y", "x"));
		Assert.That(a.Equals(b), Is.False, "order-different sequences must not be equal");
	}

	[Test]
	public void StringSequence_case_sensitive()
	{
		var a = new StringSequenceTestHelper(ImmutableArray.Create("Tag"));
		var b = new StringSequenceTestHelper(ImmutableArray.Create("tag"));
		Assert.That(a.Equals(b), Is.False, "ordinal comparison must be case-sensitive");
	}

	[Test]
	public void StringSequence_different_lengths_not_equal()
	{
		var a = new StringSequenceTestHelper(ImmutableArray.Create("x"));
		var b = new StringSequenceTestHelper(ImmutableArray.Create("x", "y"));
		Assert.That(a.Equals(b), Is.False, "sequences of different length must not be equal");
	}

	[Test]
	public void Generator_output_deterministic_with_tags_array()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Determinism With Tags",
			                Tags = new[] { "alpha", "beta" })]
			    public class DeterminismTagPage { }
			}
			""";

		var r1 = RunGenerator([source]);
		var r2 = RunGenerator([source]);

		Assert.That(GetGeneratedSource(r1), Is.Not.Empty);
		Assert.That(GetGeneratedSource(r1), Is.EqualTo(GetGeneratedSource(r2)),
			"Output must be identical across two independent runs (caching stability)");
	}

	// ─── Issue 3: UGG0007 ordinal / mixed-case ────────────────────────────────

	[Test]
	public void UGG0007_mixed_case_related_slug_warns()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample Target")]
			    public class TargetPage { }

			    [SamplePage(SampleCategory.Controls, "Sample Ref",
			                RelatedSamples = new[] { "Sample-Target" })]
			    public class RefPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0007 = UggDiagnostics(result).Where(d => d.Id == "UGG0007").ToList();
		Assert.That(ugg0007, Is.Not.Empty,
			"Mixed-case slug 'Sample-Target' must not match final slug 'sample-target' (ordinal)");
		Assert.That(ugg0007[0].GetMessage(), Does.Contain("Sample-Target"));
	}

	[Test]
	public void UGG0007_exact_lowercase_slug_no_warning()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample Target")]
			    public class TargetPage2 { }

			    [SamplePage(SampleCategory.Controls, "Sample Ref",
			                RelatedSamples = new[] { "sample-target" })]
			    public class RefPage2 { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0007"), Is.False,
			"Exact ordinal lowercase match must not warn");
	}

	// ─── Issue 7: UGG0005 narrow source location ──────────────────────────────

	[Test]
	public void UGG0005_location_is_narrower_than_class_declaration_when_available()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Sample", Slug = "Bad Slug")]
			    public class NarrowLocSample { }
			}
			""";

		// We need a real file path so the syntax tree is usable from ApplicationSyntaxReference.
		// Using RunGeneratorWithFilePaths provides a file path and thus a non-null SourceTree.
		var result = RunGeneratorWithFilePaths(
			[(source, "/repo/Views/SamplePages/NarrowLocSample.xaml.cs")]);

		var diag = UggDiagnostics(result).First(d => d.Id == "UGG0005");

		// The class keyword starts at a wide span; the slug expression starts at a much narrower one.
		// At minimum the span must not be zero and must come from the expected source file.
		Assert.That(diag.Location, Is.Not.EqualTo(Location.None));
		Assert.That(diag.Location.SourceTree, Is.Not.Null);
		Assert.That(diag.Location.SourceSpan.IsEmpty, Is.False);

		// The diagnostic must point at the slug value ("Bad Slug"), which is much narrower
		// than the full class declaration.  The slug value literal is < 15 chars; the full
		// class decl including attributes easily exceeds 80 chars.
		Assert.That(diag.Location.SourceSpan.Length, Is.LessThan(80),
			"Diagnostic span must point at the slug expression, not the full class");
	}

	// ─── Issue 8: UGG0008 null/empty array elements ───────────────────────────

	[Test]
	public void Null_tag_element_produces_UGG0008()
	{
		// We inject null as a constant expression in the array initializer.
		// The Roslyn semantic model stores it as a TypedConstant with null Value.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Null Tag",
			                Tags = new string[] { "valid", null })]
			    public class NullTagPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0008 = UggDiagnostics(result).Where(d => d.Id == "UGG0008").ToList();
		Assert.That(ugg0008, Is.Not.Empty, "Null element in Tags must produce UGG0008");
		Assert.That(ugg0008[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(ugg0008[0].GetMessage(), Does.Contain("Tags"));
		// Sample must NOT be emitted when UGG0008 fires
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("NullTagPage"),
			"Sample must be excluded when a metadata array has a null element");
	}

	[Test]
	public void Empty_string_tag_element_produces_UGG0008()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Empty Tag",
			                Tags = new[] { "valid", "" })]
			    public class EmptyTagPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0008 = UggDiagnostics(result).Where(d => d.Id == "UGG0008").ToList();
		Assert.That(ugg0008, Is.Not.Empty, "Empty string element in Tags must produce UGG0008");
		Assert.That(ugg0008[0].GetMessage(), Does.Contain("Tags"));
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("EmptyTagPage"),
			"Sample must be excluded when a metadata array has an empty element");
	}

	[Test]
	public void Null_related_samples_element_produces_UGG0008()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Related With Null",
			                RelatedSamples = new string[] { "valid-slug", null })]
			    public class RelatedNullPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0008 = UggDiagnostics(result).Where(d => d.Id == "UGG0008").ToList();
		Assert.That(ugg0008, Is.Not.Empty, "Null element in RelatedSamples must produce UGG0008");
		Assert.That(ugg0008[0].GetMessage(), Does.Contain("RelatedSamples"));
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("RelatedNullPage"),
			"Sample must be excluded when RelatedSamples has a null element");
	}

	[Test]
	public void Valid_tags_array_no_UGG0008()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Clean Tags",
			                Tags = new[] { "layout", "input" })]
			    public class CleanTagsPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0008"), Is.False);
		Assert.That(GetGeneratedSource(result), Does.Contain("CleanTagsPage"));
	}

	[Test]
	public void Tags_null_explicit_treated_as_empty_no_UGG0008()
	{
		// Explicit Tags = null must be treated as omitted (empty) and must not throw or produce UGG0008.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Explicit Null Tags",
			                Tags = null)]
			    public class ExplicitNullTagsPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null, "Generator must not throw for explicit null Tags");
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0008"), Is.False,
			"Explicit null Tags must not produce UGG0008");
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("ExplicitNullTagsPage"),
			"Sample must be emitted when Tags is explicitly null");
		Assert.That(generated, Does.Contain("Tags = null"),
			"Null Tags must be forwarded as null in the generated output");
	}

	[Test]
	public void RelatedSamples_null_explicit_treated_as_empty_no_UGG0008()
	{
		// Explicit RelatedSamples = null must be treated as omitted (empty) and must not throw.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Explicit Null Related",
			                RelatedSamples = null)]
			    public class ExplicitNullRelatedPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null, "Generator must not throw for explicit null RelatedSamples");
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0008"), Is.False,
			"Explicit null RelatedSamples must not produce UGG0008");
		Assert.That(GetGeneratedSource(result), Does.Contain("ExplicitNullRelatedPage"),
			"Sample must be emitted when RelatedSamples is explicitly null");
	}

	[Test]
	public void Multi_invalid_elements_produces_single_UGG0008_listing_all_indices()
	{
		// Three invalid entries across both arrays: Tags[1], Tags[2], RelatedSamples[0].
		// Must produce exactly ONE aggregated UGG0008 whose message names every bad index.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Multi Invalid",
			                Tags = new string[] { "good", null, "" },
			                RelatedSamples = new string[] { null, "good-slug" })]
			    public class MultiInvalidPage { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var ugg0008 = UggDiagnostics(result).Where(d => d.Id == "UGG0008").ToList();
		Assert.That(ugg0008.Count, Is.EqualTo(1), "Must emit exactly one aggregated UGG0008 per sample");
		Assert.That(ugg0008[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));

		var msg = ugg0008[0].GetMessage();
		Assert.That(msg, Does.Contain("Tags[1]"), "Aggregated message must list Tags[1]");
		Assert.That(msg, Does.Contain("Tags[2]"), "Aggregated message must list Tags[2]");
		Assert.That(msg, Does.Contain("RelatedSamples[0]"), "Aggregated message must list RelatedSamples[0]");

		Assert.That(GetGeneratedSource(result), Does.Not.Contain("MultiInvalidPage"),
			"Sample must be excluded when metadata arrays have invalid elements");
	}

	// ─── Pragma suppression ───────────────────────────────────────────────────

	[Test]
	public void Pragma_disable_UGG0004_sets_IsSuppressed()
	{
		// PragmaAlphaPage (A < B) is seen first; the diagnostic lands on PragmaBetaPage.
		// #pragma warning disable UGG0004 before PragmaBetaPage must suppress it.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Shared Title")]
			    public class PragmaAlphaPage { }

			#pragma warning disable UGG0004
			    [SamplePage(SampleCategory.Layout, "Shared Title")]
			    public class PragmaBetaPage { }
			#pragma warning restore UGG0004
			}
			""";

		// Use a real file path so the pragma trivia is linked to a source tree with a location.
		var result = RunGeneratorWithFilePaths(
			[(source, "/repo/Views/SamplePages/PragmaUgg0004Test.xaml.cs")]);

		var ugg0004 = UggDiagnostics(result).Where(d => d.Id == "UGG0004").ToList();
		Assert.That(ugg0004, Is.Not.Empty, "UGG0004 must still appear in diagnostic list (with IsSuppressed)");
		Assert.That(ugg0004[0].IsSuppressed, Is.True,
			"UGG0004 at a pragma-suppressed location must have IsSuppressed = true");
	}

	[Test]
	public void Pragma_disable_UGG0008_sets_IsSuppressed()
	{
		// UGG0008 is an Error; pragma suppression still applies and sets IsSuppressed.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			#pragma warning disable UGG0008
			    [SamplePage(SampleCategory.Controls, "Pragma Null Tag",
			                Tags = new string[] { "good", null })]
			    public class PragmaUgg0008Page { }
			#pragma warning restore UGG0008
			}
			""";

		var result = RunGeneratorWithFilePaths(
			[(source, "/repo/Views/SamplePages/PragmaUgg0008Test.xaml.cs")]);

		var ugg0008 = UggDiagnostics(result).Where(d => d.Id == "UGG0008").ToList();
		Assert.That(ugg0008, Is.Not.Empty, "UGG0008 must still appear in diagnostic list (with IsSuppressed)");
		Assert.That(ugg0008[0].IsSuppressed, Is.True,
			"UGG0008 at a pragma-suppressed location must have IsSuppressed = true");
	}

	// ─── Emitted-array assertion strengthening ────────────────────────────────

	[Test]
	public void Emitted_tags_array_uses_verbatim_literals()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Emitted Tags",
			                Tags = new[] { "alpha", "beta" })]
			    public class EmittedTagsPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		// StringLiteral produces @"..." syntax for each element
		Assert.That(generated, Does.Contain(@"Tags = new[] { @""alpha"", @""beta"" }"),
			"Tags array elements must be emitted as verbatim string literals");
		AssertGeneratedCompiles(generated, source);
	}

	[Test]
	public void Emitted_related_samples_array_uses_verbatim_literals()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Emitter A",
			                RelatedSamples = new[] { "emitter-b" })]
			    public class EmitterAPage : Page { }

			    [SamplePage(SampleCategory.Controls, "Emitter B")]
			    public class EmitterBPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);
		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain(@"@""emitter-b"""),
			"RelatedSamples entry must be emitted as verbatim string literal");
		AssertGeneratedCompiles(generated, source);
	}

	// ─── Helpers ─────────────────────────────────────────────────────────────

	// ─── AOT factory lambdas ─────────────────────────────────────────────────

	[Test]
	public void Generated_entry_contains_static_page_factory_lambda()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Factory Page")]
			    public class FactoryPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("static () => new global::Uno.Gallery.FactoryPage()"),
			"Generated entry must include a static page factory lambda");
	}

	[Test]
	public void Generated_entry_has_null_data_factory_when_no_DataType()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "No Data")]
			    public class NoDataPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var generated = GetGeneratedSource(result);
		// Page factory lambda is followed by ", null)" for no-data samples.
		Assert.That(generated, Does.Contain(", null)"),
			"Generated entry must pass null as the data factory when DataType is absent");
	}

	[Test]
	public void Generated_entry_has_static_data_factory_lambda_when_DataType_set()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class MyViewModel { }
			    [SamplePage(SampleCategory.Controls, "With Data", DataType = typeof(MyViewModel))]
			    public class WithDataPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var generated = GetGeneratedSource(result);
		Assert.That(generated, Does.Contain("static () => new global::Uno.Gallery.MyViewModel()"),
			"Generated entry must include a static data factory lambda when DataType is set");
	}

	[Test]
	public void Generated_factories_compile_no_DataType()
	{
		// Verifies that the page factory lambda is valid C# in a fresh compilation.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Compile Factory")]
			    public class CompileFactoryPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		AssertGeneratedCompiles(GetGeneratedSource(result), source);
	}

	[Test]
	public void Generated_factories_compile_with_DataType()
	{
		// Verifies that both page and data factory lambdas produce valid C#.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class CompileDataModel { }
			    [SamplePage(SampleCategory.Controls, "Compile Factory Data",
			                DataType = typeof(CompileDataModel))]
			    public class CompileFactoryDataPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		AssertGeneratedCompiles(GetGeneratedSource(result), source);
	}

	[Test]
	public void Generated_output_still_deterministic_with_factories()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Factory Determinism")]
			    public class FactoryDeterminismPage : Page { }
			}
			""";

		var r1 = RunGenerator([source]);
		var r2 = RunGenerator([source]);

		Assert.That(GetGeneratedSource(r1), Is.Not.Empty);
		Assert.That(GetGeneratedSource(r1), Is.EqualTo(GetGeneratedSource(r2)),
			"Factory-bearing output must remain deterministic across independent runs");
	}

	// ─── UGG0009: abstract or no-accessible-ctor ─────────────────────────────

	[Test]
	public void Abstract_page_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Abstract Page")]
			    public abstract class AbstractPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "Abstract page type must produce UGG0009");
		Assert.That(diag[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(diag[0].GetMessage(), Does.Contain("Page type"));
		Assert.That(diag[0].GetMessage(), Does.Contain("AbstractPage"));
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("AbstractPage"),
			"Abstract page sample must be excluded from generated output");
	}

	[Test]
	public void Page_with_only_parameterized_ctor_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Param Ctor Page")]
			    public class ParamCtorPage : Page
			    {
			        public ParamCtorPage(string name) { }
			    }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "Page with only a parameterized ctor must produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("ParamCtorPage"));
	}

	[Test]
	public void Page_with_only_private_ctor_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Private Ctor Page")]
			    public class PrivateCtorPage : Page
			    {
			        private PrivateCtorPage() { }
			    }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "Page with only a private parameterless ctor must produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("PrivateCtorPage"));
	}

	[Test]
	public void Page_with_internal_ctor_no_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Internal Ctor Page")]
			    public class InternalCtorPage : Page
			    {
			        internal InternalCtorPage() { }
			    }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0009"), Is.False,
			"Page with an internal parameterless ctor must not produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Contain("InternalCtorPage"),
			"Page with accessible internal ctor must be emitted");
	}

	[Test]
	public void Page_with_protected_internal_ctor_no_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "ProtectedInternal Ctor Page")]
			    public class ProtectedInternalCtorPage : Page
			    {
			        protected internal ProtectedInternalCtorPage() { }
			    }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0009"), Is.False,
			"Page with a protected-internal parameterless ctor must not produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Contain("ProtectedInternalCtorPage"),
			"Page with accessible protected-internal ctor must be emitted");
	}

	[Test]
	public void Page_with_only_protected_ctor_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Protected Ctor Page")]
			    public class ProtectedCtorPage : Page
			    {
			        protected ProtectedCtorPage() { }
			    }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "Page with only a protected parameterless ctor must produce UGG0009");
		Assert.That(diag[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("ProtectedCtorPage"),
			"Protected-ctor-only page sample must be excluded from generated output");
	}

	[Test]
	public void Abstract_DataType_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public abstract class AbstractViewModel { }
			    [SamplePage(SampleCategory.Controls, "Abstract DataType",
			                DataType = typeof(AbstractViewModel))]
			    public class AbstractDataTypePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "Abstract DataType must produce UGG0009");
		Assert.That(diag[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(diag[0].GetMessage(), Does.Contain("DataType"));
		Assert.That(diag[0].GetMessage(), Does.Contain("AbstractViewModel"));
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("AbstractDataTypePage"),
			"Sample with abstract DataType must be excluded from generated output");
	}

	[Test]
	public void DataType_with_only_private_ctor_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class PrivateCtorViewModel
			    {
			        private PrivateCtorViewModel() { }
			    }
			    [SamplePage(SampleCategory.Controls, "Private DataType",
			                DataType = typeof(PrivateCtorViewModel))]
			    public class PrivateDataTypePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "DataType with only a private ctor must produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("PrivateDataTypePage"));
	}

	[Test]
	public void DataType_with_internal_ctor_no_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class InternalCtorViewModel
			    {
			        internal InternalCtorViewModel() { }
			    }
			    [SamplePage(SampleCategory.Controls, "Internal DataType",
			                DataType = typeof(InternalCtorViewModel))]
			    public class InternalDataTypePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0009"), Is.False,
			"DataType with an internal parameterless ctor must not produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Contain("InternalDataTypePage"),
			"Sample with accessible internal DataType ctor must be emitted");
	}

	[Test]
	public void DataType_with_protected_internal_ctor_no_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class ProtectedInternalCtorViewModel
			    {
			        protected internal ProtectedInternalCtorViewModel() { }
			    }
			    [SamplePage(SampleCategory.Controls, "ProtectedInternal DataType",
			                DataType = typeof(ProtectedInternalCtorViewModel))]
			    public class ProtectedInternalDataTypePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0009"), Is.False,
			"DataType with a protected-internal parameterless ctor must not produce UGG0009");
		Assert.That(GetGeneratedSource(result), Does.Contain("ProtectedInternalDataTypePage"),
			"Sample with accessible protected-internal DataType ctor must be emitted");
	}

	[Test]
	public void DataType_with_protected_ctor_produces_UGG0009()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    public class ProtectedCtorViewModel
			    {
			        protected ProtectedCtorViewModel() { }
			    }
			    [SamplePage(SampleCategory.Controls, "Protected DataType",
			                DataType = typeof(ProtectedCtorViewModel))]
			    public class ProtectedDataTypePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		var diag = UggDiagnostics(result).Where(d => d.Id == "UGG0009").ToList();
		Assert.That(diag, Is.Not.Empty, "DataType with only a protected ctor must produce UGG0009");
		Assert.That(diag[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("ProtectedDataTypePage"),
			"Sample with protected-ctor-only DataType must be excluded from generated output");
	}

	// ─── Sentinel null-viewType path ─────────────────────────────────────────

	[Test]
	public void Two_arg_ctor_null_viewType_sentinel_compiles()
	{
		// Verifies that the no-suggestions sentinel construction used by Shell compiles.
		// Shell creates: new Sample(new SamplePageAttribute(category, "No suggestions found"), null)
		// The null viewType must be accepted at the call site — the ctor must not throw at construction.
		const string shellLike = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    class ShellSearchBox
			    {
			        static readonly string NoSuggestionsFoundText = "No suggestions found";
			        static Sample CreateSentinel() =>
			            new(new SamplePageAttribute(SampleCategory.Controls, NoSuggestionsFoundText), null);
			    }
			}
			""";

		// AssertGeneratedCompiles compiles with GoodStubs which has Sample(SamplePageAttribute, Type).
		// Passing null for a reference-type parameter is not a compile error; only a nullable warning.
		AssertGeneratedCompiles(string.Empty, shellLike);
	}

	/// <summary>
	/// Verifies that the generated C# source compiles without errors when re-compiled
	/// together with the stub declarations and any additional user sources.
	/// </summary>
	private static void AssertGeneratedCompiles(string generatedSource, params string[] additionalSources)
	{
		var trees = new[] { GoodStubs }
			.Concat(additionalSources)
			.Append(generatedSource)
			.Select(s => CSharpSyntaxTree.ParseText(s));

		var compilation = CSharpCompilation.Create(
			"GeneratedCompilation",
			trees,
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var errors = compilation.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.ToList();

		Assert.That(errors, Is.Empty,
			$"Generated C# must compile without errors. First error: {errors.FirstOrDefault()?.GetMessage()}");
	}
	/// Test-visible façade that exposes <c>StringSequence</c>'s equality and hash behaviour.
	/// The inner <c>StringSequence</c> type is private; this helper creates an equivalent
	/// equatable wrapper by forwarding to the same ordinal-element logic used in the generator.
	/// </summary>
	private sealed class StringSequenceTestHelper(ImmutableArray<string> values)
	{
		private readonly ImmutableArray<string> _values = values;
		private ImmutableArray<string> Values =>
			_values.IsDefault ? ImmutableArray<string>.Empty : _values;

		public bool Equals(StringSequenceTestHelper other)
		{
			var a = Values;
			var b = other.Values;
			if (a.Length != b.Length) return false;
			for (int i = 0; i < a.Length; i++)
				if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
					return false;
			return true;
		}

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
	}

	// ─── SampleRoutes: SlugToPascalIdentifier ────────────────────────────────

	[TestCase("button",        "Button")]
	[TestCase("my-control",    "MyControl")]
	[TestCase("oauth2-login",  "Oauth2Login")]
	[TestCase("a1b",           "A1b",  Description = "digit-mid: no prefix needed")]
	[TestCase("a-1b",          "A1b",  Description = "same as a1b — the collision case")]
	[TestCase("2cool",         "_2cool", Description = "digit-start gets _ prefix")]
	[TestCase("2-cool",        "_2Cool", Description = "digit-start segment + letter segment")]
	[TestCase("sample",        "Sample")]
	[TestCase("progress-ring-bar", "ProgressRingBar")]
	public void SlugToPascalIdentifier_converts_correctly(string slug, string expected) =>
		Assert.That(SamplesGenerator.SlugToPascalIdentifier(slug), Is.EqualTo(expected));

	[Test]
	public void SampleRoutes_emits_PascalCase_constant_for_derived_slug()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Control")]
			    public class MyControlPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var routes = GetRoutesSource(result);
		Assert.That(routes, Is.Not.Empty, "SampleRoutes.g.cs must be emitted");
		Assert.That(routes, Does.Contain("class SampleRoutes"));
		Assert.That(routes, Does.Contain("public const string MyControl = @\"my-control\";"),
			"Derived slug 'my-control' → identifier 'MyControl'");
	}

	[Test]
	public void SampleRoutes_emits_explicit_slug_constant()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Renamed Sample", Slug = "old-name")]
			    public class RenamedPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var routes = GetRoutesSource(result);
		Assert.That(routes, Does.Contain("public const string OldName = @\"old-name\";"),
			"Explicit slug 'old-name' → identifier 'OldName'");
	}

	[Test]
	public void SampleRoutes_digit_start_slug_gets_underscore_prefix()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Sample", Slug = "2cool")]
			    public class TwoCoolPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);

		var routes = GetRoutesSource(result);
		Assert.That(routes, Does.Contain("public const string _2cool = @\"2cool\";"),
			"Digit-start slug must get _ prefix on identifier");
	}

	[Test]
	public void SampleRoutes_ugg0006_pair_emits_single_constant()
	{
		// Two samples share the same slug (UGG0006) but should still produce one route constant.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Hello World")]
			    public class SlugDupFirst : Page { }

			    [SamplePage(SampleCategory.Layout, "hello world")]
			    public class SlugDupSecond : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0006"), Is.True, "Expect UGG0006");
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0010"), Is.False, "No UGG0010 — same slug");

		var routes = GetRoutesSource(result);
		// One constant for the shared slug value
		var count = CountOccurrences(routes, "HelloWorld");
		Assert.That(count, Is.EqualTo(1), "Exactly one HelloWorld constant for a UGG0006 pair");
	}

	// ─── UGG0010: identifier collision ───────────────────────────────────────

	[Test]
	public void UGG0010_fires_when_different_slugs_produce_same_identifier()
	{
		// 'a1b' → A1b  and  'a-1b' → A + 1b = A1b  (digit-start segment prevents disambiguation)
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Alpha", Slug = "a1b")]
			    public class AlphaCollide : Page { }

			    [SamplePage(SampleCategory.Layout, "Beta", Slug = "a-1b")]
			    public class BetaCollide : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);

		// Two-way collision → two UGG0010 diagnostics: one for each colliding declaration.
		var ugg0010 = UggDiagnostics(result).Where(d => d.Id == "UGG0010").ToList();
		Assert.That(ugg0010, Has.Count.EqualTo(2),
			"Two-way slug collision must produce exactly two UGG0010 diagnostics (one per declaration)");

		// Locate each diagnostic by the type name it mentions — order-independent.
		var alphaDiag = ugg0010.Single(d => d.GetMessage().Contains("AlphaCollide"));
		var betaDiag  = ugg0010.Single(d => d.GetMessage().Contains("BetaCollide"));

		Assert.That(alphaDiag.Severity, Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(betaDiag.Severity,  Is.EqualTo(DiagnosticSeverity.Error));
		Assert.That(alphaDiag.Location, Is.Not.EqualTo(Location.None));
		Assert.That(betaDiag.Location,  Is.Not.EqualTo(Location.None));

		var msgAlpha = alphaDiag.GetMessage();
		Assert.That(msgAlpha, Does.Contain("A1b"),  "AlphaCollide diagnostic must name the colliding identifier");
		Assert.That(msgAlpha, Does.Contain("a1b"),  "AlphaCollide diagnostic must name its own slug");
		Assert.That(msgAlpha, Does.Contain("a-1b"), "AlphaCollide diagnostic must name the colliding slug");

		var msgBeta = betaDiag.GetMessage();
		Assert.That(msgBeta, Does.Contain("A1b"),  "BetaCollide diagnostic must name the colliding identifier");
		Assert.That(msgBeta, Does.Contain("a-1b"), "BetaCollide diagnostic must name its own slug");
		Assert.That(msgBeta, Does.Contain("a1b"),  "BetaCollide diagnostic must name the other slug");

		// Both samples must still appear in GetSamples — UGG0010 does NOT corrupt App output.
		var appSource = GetGeneratedSource(result);
		Assert.That(appSource, Does.Contain("AlphaCollide"), "AlphaCollide must remain in GetSamples");
		Assert.That(appSource, Does.Contain("BetaCollide"),  "BetaCollide must remain in GetSamples");

		// Neither route constant may be emitted.
		var routes = GetRoutesSource(result);
		Assert.That(routes, Does.Not.Contain("A1b"),
			"Colliding identifier A1b must be omitted from SampleRoutes");
	}

	[Test]
	public void UGG0010_omits_both_colliding_constants_leaves_others_intact()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Alpha", Slug = "a1b")]
			    public class AlphaCollide2 : Page { }

			    [SamplePage(SampleCategory.Layout, "Beta", Slug = "a-1b")]
			    public class BetaCollide2 : Page { }

			    [SamplePage(SampleCategory.Controls, "Gamma", Slug = "gamma")]
			    public class GammaPage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result).Any(d => d.Id == "UGG0010"), Is.True);

		var routes = GetRoutesSource(result);
		Assert.That(routes, Does.Not.Contain("A1b"), "Colliding A1b must be absent");
		Assert.That(routes, Does.Contain("Gamma"),    "Non-colliding Gamma must still be present");
	}

	[Test]
	public void UGG0010_diagnostic_has_source_location()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Alpha", Slug = "a1b")]
			    public class LocAlpha : Page { }

			    [SamplePage(SampleCategory.Layout, "Beta", Slug = "a-1b")]
			    public class LocBeta : Page { }
			}
			""";

		var result = RunGeneratorWithFilePaths(
			[(source, "/repo/Views/SamplePages/LocAlphaBeta.xaml.cs")]);

		// Both declarations get a diagnostic; verify all have source locations.
		var diags = UggDiagnostics(result).Where(d => d.Id == "UGG0010").ToList();
		Assert.That(diags, Has.Count.EqualTo(2), "Two-way collision must produce two UGG0010 diagnostics");
		foreach (var diag in diags)
		{
			Assert.That(diag.Location.SourceTree, Is.Not.Null,
				"UGG0010 must be source-located");
			Assert.That(diag.Location.SourceSpan.IsEmpty, Is.False,
				"Span must not be empty");
		}
	}

	[Test]
	public void UGG0010_three_way_collision_first_reported_once_each_later_reported()
	{
		// a1b2c, a1b-2c, a-1b2c all produce identifier "A1b2c".
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "First",  Slug = "a1b2c")]
			    public class ThreewayFirst  : Page { }

			    [SamplePage(SampleCategory.Layout,   "Second", Slug = "a1b-2c")]
			    public class ThreewaySecond : Page { }

			    [SamplePage(SampleCategory.Media,    "Third",  Slug = "a-1b2c")]
			    public class ThreewayThird  : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);

		var ugg0010 = UggDiagnostics(result).Where(d => d.Id == "UGG0010").ToList();
		// First declaration reported once (when second collision is detected), then second and third.
		Assert.That(ugg0010, Has.Count.EqualTo(3),
			"3-way collision: first declaration once + two later declarations = 3 UGG0010 diagnostics");

		// All three messages must name the shared identifier.
		Assert.That(ugg0010.All(d => d.GetMessage().Contains("A1b2c")), Is.True,
			"All three diagnostics must name the colliding identifier A1b2c");

		// The colliding identifier must be absent from routes.
		var routes = GetRoutesSource(result);
		Assert.That(routes, Does.Not.Contain("A1b2c"),
			"Colliding identifier must be omitted from SampleRoutes");

		// All three samples must remain in GetSamples.
		var appSource = GetGeneratedSource(result);
		Assert.That(appSource, Does.Contain("ThreewayFirst"),  "ThreewayFirst must remain in GetSamples");
		Assert.That(appSource, Does.Contain("ThreewaySecond"), "ThreewaySecond must remain in GetSamples");
		Assert.That(appSource, Does.Contain("ThreewayThird"),  "ThreewayThird must remain in GetSamples");
	}

	[Test]
	public void Pragma_disable_UGG0010_on_later_declaration_sets_IsSuppressed()
	{
		// #pragma warning disable UGG0010 before BetaCollide suppresses the later diagnostic
		// but leaves the first declaration's diagnostic unsuppressed.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Alpha", Slug = "a1b")]
			    public class PragmaAlphaCollide : Page { }

			#pragma warning disable UGG0010
			    [SamplePage(SampleCategory.Layout, "Beta", Slug = "a-1b")]
			    public class PragmaBetaCollide : Page { }
			#pragma warning restore UGG0010
			}
			""";

		var result = RunGeneratorWithFilePaths(
			[(source, "/repo/Views/SamplePages/PragmaUgg0010Test.xaml.cs")]);

		var ugg0010 = UggDiagnostics(result).Where(d => d.Id == "UGG0010").ToList();
		Assert.That(ugg0010, Has.Count.EqualTo(2), "Both declarations must get UGG0010");

		// Exactly one must be suppressed (the one on PragmaBetaCollide).
		Assert.That(ugg0010.Count(d => d.IsSuppressed), Is.EqualTo(1),
			"Exactly the later declaration's UGG0010 must be suppressed by the pragma");
		Assert.That(ugg0010.Count(d => !d.IsSuppressed), Is.EqualTo(1),
			"The first declaration's UGG0010 must remain unsuppressed");

		var suppressed = ugg0010.Single(d => d.IsSuppressed);
		Assert.That(suppressed.GetMessage(), Does.Contain("PragmaBetaCollide"),
			"Suppressed diagnostic must name the pragma-covered declaration");
	}

	// ─── SampleManifest ───────────────────────────────────────────────────────

	[Test]
	public void UGG0011_explicit_stable_lists_every_missing_contract_field()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Incomplete", Status = SampleStatus.Stable)]
			    public class IncompleteStablePage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var diagnostic = UggDiagnostics(result).Single(d => d.Id == "UGG0011");

		Assert.Multiple(() =>
		{
			Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Error));
			Assert.That(diagnostic.GetMessage(), Does.Contain("ContractVersion (must be 1)"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("Description"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("DocumentationLink"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("SupportedDesigns"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("SupportedRenderers"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("AccessibilityNotes"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("Variants"));
		});
		Assert.That(GetGeneratedSource(result), Does.Not.Contain("IncompleteStablePage"));
	}

	[Test]
	public void UGG0011_contract_v1_applies_to_non_stable_status_and_validates_iso_date()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Preview Contract",
			        ContractVersion = 1,
			        Status = SampleStatus.Preview,
			        Description = "Description",
			        DocumentationLink = "https://example.com",
			        Tags = new[] { "preview" },
			        Owner = "team",
			        ReviewedOn = "2026-02-30",
			        SupportedDesigns = SampleDesigns.Fluent,
			        SupportedRenderers = SampleRenderers.DOM,
			        Requirements = new[] { "None" },
			        AccessibilityNotes = new[] { "Keyboard supported" },
			        ResetBehavior = "Reload",
			        Variants = new[] { "Default" })]
			    public class InvalidDateContractPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var diagnostic = UggDiagnostics(result).Single(d => d.Id == "UGG0011");
		Assert.That(diagnostic.GetMessage(), Does.Contain("ReviewedOn (expected YYYY-MM-DD)"));
	}

	[Test]
	public void UGG0011_rejects_unknown_contract_version()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Future Contract",
			        ContractVersion = 2,
			        Status = SampleStatus.Preview)]
			    public class FutureContractPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var diagnostic = UggDiagnostics(result).Single(d => d.Id == "UGG0011");
		Assert.That(diagnostic.GetMessage(), Does.Contain("ContractVersion (supported values are 0 and 1)"));
	}

	[Test]
	public void UGG0011_rejects_malformed_contract_links()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Bad Links",
			        ContractVersion = 1,
			        Status = SampleStatus.Preview,
			        Description = "Description",
			        DocumentationLink = "relative-docs",
			        Tags = new[] { "links" },
			        Owner = "team",
			        ReviewedOn = "2026-08-27",
			        SupportedDesigns = SampleDesigns.Fluent,
			        SupportedRenderers = SampleRenderers.DOM,
			        Requirements = new[] { "None" },
			        AccessibilityNotes = new[] { "Keyboard supported" },
			        ResetBehavior = "Reload",
			        Variants = new[] { "Default" },
			        IssueLink = "not a URI",
			        ApiLink = "/relative-api")]
			    public class BadLinksPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var diagnostic = UggDiagnostics(result).Single(d => d.Id == "UGG0011");
		Assert.Multiple(() =>
		{
			Assert.That(diagnostic.GetMessage(), Does.Contain("DocumentationLink (expected absolute URI)"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("IssueLink (expected absolute URI)"));
			Assert.That(diagnostic.GetMessage(), Does.Contain("ApiLink (expected absolute URI)"));
		});
	}

	[Test]
	public void Contract_v1_emits_all_fields_and_exact_flag_values_and_names()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Contract Sample",
			        ContractVersion = 1,
			        Status = SampleStatus.Stable,
			        Description = "A \"quoted\" description",
			        DocumentationLink = "https://example.com/docs?q=a&b=c",
			        Tags = new[] { "contract" },
			        Owner = "team",
			        ReviewedOn = "2026-08-27",
			        SupportedDesigns = SampleDesigns.Material | SampleDesigns.Agnostic,
			        SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
			        Requirements = new[] { "Network: none", "Setup: C:\\demo" },
			        AccessibilityNotes = new[] { "Screen reader says \"ready\"" },
			        ResetBehavior = "Choose Reset.",
			        Variants = new[] { "Default", "Dense" },
			        KnownLimitations = new[] { "None known." },
			        IssueLink = "https://example.com/issues/1",
			        ApiLink = "https://example.com/api")]
			    public class ContractSamplePage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var sample = doc.RootElement.GetProperty("samples")[0];
		Assert.Multiple(() =>
		{
			Assert.That(sample.GetProperty("contractVersion").GetInt32(), Is.EqualTo(1));
			Assert.That(sample.GetProperty("statusExplicit").GetBoolean(), Is.True);
			Assert.That(sample.GetProperty("supportedDesigns").GetProperty("value").GetInt32(), Is.EqualTo(17));
			Assert.That(sample.GetProperty("supportedDesigns").GetProperty("name").GetString(), Is.EqualTo("Material, Agnostic"));
			Assert.That(sample.GetProperty("supportedRenderers").GetProperty("value").GetInt32(), Is.EqualTo(7));
			Assert.That(sample.GetProperty("supportedRenderers").GetProperty("name").GetString(), Is.EqualTo("Native, Skia, DOM"));
			Assert.That(sample.GetProperty("requirements")[1].GetString(), Is.EqualTo(@"Setup: C:\demo"));
			Assert.That(sample.GetProperty("accessibilityNotes")[0].GetString(), Is.EqualTo("Screen reader says \"ready\""));
			Assert.That(sample.GetProperty("knownLimitations")[0].GetString(), Is.EqualTo("None known."));
			Assert.That(sample.GetProperty("issueLink").GetString(), Is.EqualTo("https://example.com/issues/1"));
			Assert.That(sample.GetProperty("apiLink").GetString(), Is.EqualTo("https://example.com/api"));
		});
	}

	[Test]
	public void UGG0011_accepts_case_insensitive_http_schemes()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Upper Scheme",
			        ContractVersion = 1,
			        Status = SampleStatus.Stable,
			        Description = "Description",
			        DocumentationLink = "HTTPS://example.com/docs",
			        Tags = new[] { "contract" },
			        Owner = "team",
			        ReviewedOn = "2026-08-27",
			        SupportedDesigns = SampleDesigns.Fluent,
			        SupportedRenderers = SampleRenderers.DOM,
			        Requirements = new[] { "None" },
			        AccessibilityNotes = new[] { "Keyboard" },
			        ResetBehavior = "Reload",
			        Variants = new[] { "Default" },
			        IssueLink = "HTTP://example.com/issues/1")]
			    public class UpperSchemePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(UggDiagnostics(result), Is.Empty);
		Assert.That(InvokeGetJson(result, source), Does.Contain("HTTPS://example.com/docs"));
	}

	[Test]
	public void SampleManifest_emits_valid_json_with_schema_version_2()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Sample", SourceSdk.WinUI, "\uE8FA")]
			    public class ManifestSamplePage : Page { }
			}
			""";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var manifestSource = GetManifestSource(result);
		Assert.That(manifestSource, Is.Not.Empty, "SampleManifest.g.cs must be emitted");
		Assert.That(manifestSource, Does.Contain("class SampleManifest"));
		Assert.That(manifestSource, Does.Contain("GetJson()"));

		// Compile the manifest together with App.Samples.g.cs and verify GetJson() is invokable.
		var json = InvokeGetJson(result, source);
		Assert.That(json, Is.Not.Null.And.Not.Empty, "GetJson() must return a non-empty string");

		// Parse with System.Text.Json.
		using var doc = System.Text.Json.JsonDocument.Parse(json!);
		var root = doc.RootElement;
		Assert.That(root.GetProperty("schemaVersion").GetInt32(), Is.EqualTo(2),
			"schemaVersion must be 2");

		var samples = root.GetProperty("samples");
		Assert.That(samples.GetArrayLength(), Is.EqualTo(1), "Exactly one sample in catalog");
	}

	[Test]
	public void SampleManifest_all_required_fields_present()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "My Sample",
			                Description = "A description.",
			                DocumentationLink = "https://example.com",
			                SortOrder = 5,
			                Status = SampleStatus.Preview,
			                Owner = "alice",
			                ReviewedOn = "2024-06-01",
			                Tags = new[] { "input", "layout" },
			                RelatedSamples = new[] { "my-sample" })]
			    public class FullFieldsPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		Assert.That(result.Exception, Is.Null);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		Assert.That(s.GetProperty("fqn").GetString(), Is.EqualTo("Uno.Gallery.FullFieldsPage"));
		Assert.That(s.GetProperty("slug").GetString(), Is.EqualTo("my-sample"));
		Assert.That(s.GetProperty("title").GetString(), Is.EqualTo("My Sample"));
		Assert.That(s.GetProperty("description").GetString(), Is.EqualTo("A description."));
		Assert.That(s.GetProperty("documentationLink").GetString(), Is.EqualTo("https://example.com"));
		Assert.That(s.GetProperty("sortOrder").GetInt32(), Is.EqualTo(5));
		Assert.That(s.GetProperty("owner").GetString(), Is.EqualTo("alice"));
		Assert.That(s.GetProperty("reviewedOn").GetString(), Is.EqualTo("2024-06-01"));

		// category
		var cat = s.GetProperty("category");
		Assert.That(cat.GetProperty("value").GetInt32(), Is.EqualTo(0)); // Controls = 0
		Assert.That(cat.GetProperty("name").GetString(), Is.EqualTo("Controls"));

		// sourceSdk
		var sdk = s.GetProperty("sourceSdk");
		Assert.That(sdk.GetProperty("value").GetInt32(), Is.EqualTo(0)); // WinUI = 0
		Assert.That(sdk.GetProperty("name").GetString(), Is.EqualTo("WinUI"));

		// status
		var status = s.GetProperty("status");
		Assert.That(status.GetProperty("value").GetInt32(), Is.EqualTo(1)); // Preview = 1
		Assert.That(status.GetProperty("name").GetString(), Is.EqualTo("Preview"));

		// tags
		var tags = s.GetProperty("tags");
		Assert.That(tags.GetArrayLength(), Is.EqualTo(2));
		Assert.That(tags[0].GetString(), Is.EqualTo("input"));
		Assert.That(tags[1].GetString(), Is.EqualTo("layout"));

		// relatedSamples
		var related = s.GetProperty("relatedSamples");
		Assert.That(related.GetArrayLength(), Is.EqualTo(1));
		Assert.That(related[0].GetString(), Is.EqualTo("my-sample"));
	}

	[Test]
	public void SampleManifest_null_fields_represented_as_json_null()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Minimal")]
			    public class MinimalPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		Assert.That(s.GetProperty("description").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("documentationLink").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("owner").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("reviewedOn").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("sourcePath").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("platformConditionals").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("contractVersion").GetInt32(), Is.Zero);
		Assert.That(s.GetProperty("statusExplicit").GetBoolean(), Is.False);
		Assert.That(s.GetProperty("supportedDesigns").GetProperty("value").GetInt32(), Is.Zero);
		Assert.That(s.GetProperty("supportedDesigns").GetProperty("name").GetString(), Is.EqualTo("None"));
		Assert.That(s.GetProperty("supportedRenderers").GetProperty("value").GetInt32(), Is.Zero);
		Assert.That(s.GetProperty("supportedRenderers").GetProperty("name").GetString(), Is.EqualTo("None"));
		Assert.That(s.GetProperty("resetBehavior").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("issueLink").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
		Assert.That(s.GetProperty("apiLink").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));

		// Empty arrays, not null
		Assert.That(s.GetProperty("tags").GetArrayLength(), Is.EqualTo(0));
		Assert.That(s.GetProperty("relatedSamples").GetArrayLength(), Is.EqualTo(0));
		Assert.That(s.GetProperty("requirements").GetArrayLength(), Is.EqualTo(0));
		Assert.That(s.GetProperty("accessibilityNotes").GetArrayLength(), Is.EqualTo(0));
		Assert.That(s.GetProperty("variants").GetArrayLength(), Is.EqualTo(0));
		Assert.That(s.GetProperty("knownLimitations").GetArrayLength(), Is.EqualTo(0));

		// Default sourceSdk when unspecified is WinUI (value 0, name "WinUI").
		var sdk = s.GetProperty("sourceSdk");
		Assert.That(sdk.GetProperty("value").GetInt32(), Is.EqualTo(0),
			"Default SourceSdk must be WinUI (0)");
		Assert.That(sdk.GetProperty("name").GetString(), Is.EqualTo("WinUI"),
			"Default SourceSdk name must be 'WinUI'");
	}

	[Test]
	public void SampleManifest_platformConditionals_emitted_as_number_when_set()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Windows Only")]
			    [SampleConditional(SampleConditionals.Windows)]
			    public class WindowsOnlyPage : Page { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols: ["WINDOWS"]);
		Assert.That(result.Exception, Is.Null);

		var json = InvokeGetJson(result, source, ["WINDOWS"])!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		var conditionals = s.GetProperty("platformConditionals");
		Assert.That(conditionals.ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Number),
			"platformConditionals must be a number when SampleConditionalAttribute is present");
		Assert.That(conditionals.GetUInt32(), Is.EqualTo(1u), // SampleConditionals.Windows = 1
			"Windows flag value is 1");
	}

	[Test]
	public void SampleManifest_special_chars_in_title_and_description_survive_round_trip()
	{
		// Title with embedded double-quote, description with backslash.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Say \"Hello\"",
			                Description = "Path\\Separator")]
			    public class SpecialCharPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		Assert.That(s.GetProperty("title").GetString(), Is.EqualTo("Say \"Hello\""),
			"Embedded double-quote must survive JSON round-trip");
		Assert.That(s.GetProperty("description").GetString(), Is.EqualTo("Path\\Separator"),
			"Backslash must survive JSON round-trip");
	}

	[Test]
	public void SampleManifest_control_char_in_title_is_escaped()
	{
		// Embed a tab (U+0009) in the title — must be emitted as \t in JSON.
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Tab\tChar")]
			    public class TabCharPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		Assert.That(result.Exception, Is.Null);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];
		// System.Text.Json unescapes \t → actual tab char
		Assert.That(s.GetProperty("title").GetString(), Is.EqualTo("Tab\tChar"),
			"Tab character must survive JSON round-trip via \\t escape");
	}

	[Test]
	public void SampleManifest_deterministic_across_reversed_file_order()
	{
		const string sourceApple = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Apple")]
			    public class AppleManifestPage : Page { }
			}
			""";
		const string sourceZebra = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Layout, "Zebra")]
			    public class ZebraManifestPage : Page { }
			}
			""";

		var r1 = RunGenerator([sourceApple, sourceZebra]);
		var r2 = RunGenerator([sourceZebra, sourceApple]);

		var json1 = InvokeGetJson(r1, sourceApple, null, sourceZebra)!;
		var json2 = InvokeGetJson(r2, sourceZebra, null, sourceApple)!;

		Assert.That(json1, Is.EqualTo(json2),
			"Manifest JSON must be identical regardless of source file order");

		// Apple (A) must precede Zebra (Z) — FQN-sorted
		using var doc = System.Text.Json.JsonDocument.Parse(json1);
		var samples = doc.RootElement.GetProperty("samples");
		Assert.That(samples.GetArrayLength(), Is.EqualTo(2));
		Assert.That(samples[0].GetProperty("fqn").GetString(), Does.Contain("Apple"),
			"Apple (lower FQN) must appear first");
		Assert.That(samples[1].GetProperty("fqn").GetString(), Does.Contain("Zebra"),
			"Zebra must appear second");
	}

	[Test]
	public void SampleManifest_default_sortOrder_is_MaxValue()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Default Order")]
			    public class DefaultOrderPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];
		Assert.That(s.GetProperty("sortOrder").GetInt32(), Is.EqualTo(int.MaxValue),
			"Default sortOrder must be int.MaxValue");
	}

	[Test]
	public void SampleManifest_sample_count_matches_GetSamples_for_platform()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "A")]
			    public class PageA : Page { }

			    [SamplePage(SampleCategory.Layout, "B")]
			    public class PageB : Page { }

			    [SamplePage(SampleCategory.Controls, "C")]
			    [SampleConditional(SampleConditionals.Disabled)]
			    public class PageC : Page { }
			}
			""";

		var result = RunGenerator([source]);
		Assert.That(result.Exception, Is.Null);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var manifestCount = doc.RootElement.GetProperty("samples").GetArrayLength();

		// PageC is Disabled so GetSamples has 2 entries; manifest must match.
		Assert.That(manifestCount, Is.EqualTo(2),
			"Manifest count must match GetSamples filtered count");

		var appSource = GetGeneratedSource(result);
		Assert.That(appSource, Does.Not.Contain("PageC"), "PageC must be absent from GetSamples");
	}

	[Test]
	public void SampleManifest_contains_only_samples_compatible_with_compilation_target()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "Wasm")]
			    [SampleConditional(SampleConditionals.Wasm)]
			    public class WasmPage : Page { }

			    [SamplePage(SampleCategory.Controls, "Windows")]
			    [SampleConditional(SampleConditionals.Windows)]
			    public class WindowsPage : Page { }

			    [SamplePage(SampleCategory.Controls, "Everywhere")]
			    public class EverywherePage : Page { }
			}
			""";

		var result = RunGenerator([source], preprocessorSymbols: ["__WASM__"]);
		var json = InvokeGetJson(result, source, ["__WASM__"])!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var fqns = doc.RootElement.GetProperty("samples")
			.EnumerateArray()
			.Select(sample => sample.GetProperty("fqn").GetString())
			.ToArray();

		Assert.That(fqns, Does.Contain("Uno.Gallery.WasmPage"));
		Assert.That(fqns, Does.Contain("Uno.Gallery.EverywherePage"));
		Assert.That(fqns, Does.Not.Contain("Uno.Gallery.WindowsPage"));
	}

	[Test]
	public void SampleManifest_sourcePath_absent_for_in_memory_tree()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "In Memory")]
			    public class InMemoryManifestPage : Page { }
			}
			""";

		var result = RunGenerator([source]);
		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];
		Assert.That(s.GetProperty("sourcePath").ValueKind,
			Is.EqualTo(System.Text.Json.JsonValueKind.Null),
			"sourcePath must be null for in-memory trees");
	}

	[Test]
	public void SampleManifest_sourcePath_emitted_for_file_backed_tree()
	{
		const string source = """
			using Uno.Gallery;
			namespace Uno.Gallery
			{
			    [SamplePage(SampleCategory.Controls, "File Backed")]
			    public class FileBackedManifestPage : Page { }
			}
			""";

		// Supply an explicit file path so ComputeSourcePath can anchor at Views/.
		var result = RunGeneratorWithFilePaths([
			(source, @"C:\repo\Uno.Gallery\Views\SamplePages\FileBackedManifestPage.xaml.cs")
		]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		Assert.That(s.GetProperty("sourcePath").GetString(),
			Is.EqualTo("Views/SamplePages/FileBackedManifestPage.xaml.cs"),
			"sourcePath in manifest must be repo-relative with forward slashes, anchored at Views/");
	}

	[Test]
	public void SampleManifest_control_heavy_title_compiles_and_chunks_safely()
	{
		// 1,400 repetitions of \\u0001 (Roslyn parses each as U+0001).
		// Each U+0001 expands to 6 C#-escaped chars (\\u0001).
		// Total ≈ 1,400 × 6 = 8,400 > MaxEscapedChunkLen (8,000), so the generated manifest
		// must split into at least two sb.Append() calls for this sample's JSON fragment.
		var controlEscapes = string.Concat(Enumerable.Repeat("\\u0001", 1400));
		var source = "using Uno.Gallery;\nnamespace Uno.Gallery\n{\n" +
		             "    [SamplePage(SampleCategory.Controls, \"X:" + controlEscapes + "\")]\n" +
		             "    public class ControlHeavyPage : Page { }\n}";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		// Must compile and produce parseable JSON.
		var json = InvokeGetJson(result, source)!;
		Assert.That(json, Is.Not.Empty);
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];
		// Title has 1,402 chars: "X:" + 1,400 × U+0001.
		var expectedTitle = "X:" + new string('\x01', 1400);
		Assert.That(s.GetProperty("title").GetString(), Is.EqualTo(expectedTitle),
			"Control-char title must survive manifest JSON round-trip");

		// Verify no single sb.Append("...") literal exceeds 8,000 escaped chars.
		var manifestSource = GetManifestSource(result);
		int maxLiteralLen = 0;
		int searchPos = 0;
		const string appendPrefix = "sb.Append(\"";
		while (true)
		{
			int start = manifestSource.IndexOf(appendPrefix, searchPos, StringComparison.Ordinal);
			if (start < 0) break;
			start += appendPrefix.Length;
			int end = start;
			while (end < manifestSource.Length)
			{
				if (manifestSource[end] == '\\') { end += 2; continue; }
				if (manifestSource[end] == '"') break;
				end++;
			}
			maxLiteralLen = Math.Max(maxLiteralLen, end - start);
			searchPos = end + 1;
		}
		Assert.That(maxLiteralLen, Is.LessThanOrEqualTo(8_000),
			"No generated sb.Append() literal must exceed 8,000 escaped characters");
	}

	[Test]
	public void SampleManifest_valid_surrogate_pair_emoji_survives_json_round_trip()
	{
		// \\uD83D\\uDD0E in the source → Roslyn parses as surrogate pair (emoji U+1F50E 🔎).
		// AppendJsonString must emit \\uD83D\\uDD0E JSON escapes; System.Text.Json decodes the emoji.
		var source = "using Uno.Gallery;\nnamespace Uno.Gallery\n{\n" +
		             "    [SamplePage(SampleCategory.Controls, \"Search: \\uD83D\\uDD0E\")]\n" +
		             "    public class EmojiManifestPage : Page { }\n}";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);
		Assert.That(UggDiagnostics(result), Is.Empty);

		var json = InvokeGetJson(result, source)!;
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		// System.Text.Json decodes \uD83D\uDD0E → the actual emoji char pair.
		var expected = "Search: \uD83D\uDD0E"; // valid surrogate pair in C# string
		Assert.That(s.GetProperty("title").GetString(), Is.EqualTo(expected),
			"Valid surrogate-pair emoji must survive JSON round-trip unchanged");
	}

	[Test]
	public void SampleManifest_lone_high_surrogate_replaced_with_replacement_char()
	{
		// \\uD83D alone → Roslyn parses as lone high surrogate (no following low surrogate).
		// AppendJsonString must replace it with \\uFFFD so the output is valid JSON.
		var source = "using Uno.Gallery;\nnamespace Uno.Gallery\n{\n" +
		             "    [SamplePage(SampleCategory.Controls, \"Lone: \\uD83D\")]\n" +
		             "    public class LoneSurrogateManifestPage : Page { }\n}";

		var result = RunGenerator([source]);

		Assert.That(result.Exception, Is.Null);

		var json = InvokeGetJson(result, source)!;
		Assert.That(json, Is.Not.Empty, "GetJson() must not throw for a lone-surrogate title");
		using var doc = System.Text.Json.JsonDocument.Parse(json);
		var s = doc.RootElement.GetProperty("samples")[0];

		var title = s.GetProperty("title").GetString()!;
		Assert.That(title, Does.Contain("\uFFFD"),
			"Lone surrogate must be replaced with U+FFFD in the JSON output");
		Assert.That(title, Does.Not.Contain("\uD83D"),
			"The lone high surrogate must not appear in the parsed title");
	}

	// ─── Helpers ─────────────────────────────────────────────────────────────

	private static string GetRoutesSource(GeneratorRunResult result)
	{
		var entry = result.GeneratedSources
			.FirstOrDefault(s => s.HintName == "SampleRoutes.g.cs");
		return entry.SourceText?.ToString() ?? string.Empty;
	}

	private static string GetManifestSource(GeneratorRunResult result)
	{
		var entry = result.GeneratedSources
			.FirstOrDefault(s => s.HintName == "SampleManifest.g.cs");
		return entry.SourceText?.ToString() ?? string.Empty;
	}

	/// <summary>
	/// Compiles the generator outputs together with the stubs, then locates and invokes
	/// <c>SampleManifest.GetJson()</c> via reflection to get the runtime JSON value.
	/// </summary>
	private static string? InvokeGetJson(
		GeneratorRunResult result,
		string primarySource,
		IEnumerable<string>? preprocessorSymbols = null,
		params string[] extraSources)
	{
		var generated = result.GeneratedSources
			.Select(s => s.SourceText.ToString())
			.ToArray();

		var allSources = new[] { GoodStubs, primarySource }
			.Concat(extraSources)
			.Concat(generated);

		var parseOptions = preprocessorSymbols is not null
			? CSharpParseOptions.Default.WithPreprocessorSymbols(preprocessorSymbols)
			: CSharpParseOptions.Default;

		var trees = allSources.Select(s => CSharpSyntaxTree.ParseText(s, parseOptions));

		var compilation = CSharpCompilation.Create(
			"InvokeCompilation",
			trees,
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var ms = new System.IO.MemoryStream();
		var emitResult = compilation.Emit(ms);
		if (!emitResult.Success)
		{
			var errs = emitResult.Diagnostics
				.Where(d => d.Severity == DiagnosticSeverity.Error)
				.Select(d => d.GetMessage())
				.Take(3);
			Assert.Fail($"Compilation for InvokeGetJson failed: {string.Join("; ", errs)}");
			return null;
		}

		ms.Seek(0, System.IO.SeekOrigin.Begin);
		var asm = System.Reflection.Assembly.Load(ms.ToArray());
		var type = asm.GetType("Uno.Gallery.SampleManifest");
		Assert.That(type, Is.Not.Null, "SampleManifest type must exist in compiled assembly");
		var method = type!.GetMethod("GetJson", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		Assert.That(method, Is.Not.Null, "GetJson() method must be accessible");
		return (string?)method!.Invoke(null, null);
	}

	private static int CountOccurrences(string text, string needle)
	{
		int count = 0, pos = 0;
		while ((pos = text.IndexOf(needle, pos, StringComparison.Ordinal)) >= 0)
		{
			count++;
			pos += needle.Length;
		}
		return count;
	}
}
