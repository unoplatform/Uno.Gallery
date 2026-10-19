using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Uno.Gallery.SourceGenerators;

[Generator]
public sealed class SamplesGenerator : IIncrementalGenerator
{
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
		int SortOrder
		);

	private record struct PlatformModel(string? TargetFramework, SampleConditionals Platform, bool IsKnown);

#pragma warning disable RS2008 // Enable analyzer release tracking
	private static readonly DiagnosticDescriptor UnsupportedTargetFramework = new(
		id: "GALLERY0001",
		title: "Unsupported TargetFramework",
		messageFormat: "Unsupported TargetFramework '{0}': it does not map to any SampleConditionals value, falling back to Always",
		category: "Uno.Gallery.SourceGenerators",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);
#pragma warning restore RS2008

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var currentPlatformProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
		{
			options.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework);
			var platform = GetSampleConditionalsFromTargetFramework(targetFramework);

			return new PlatformModel(targetFramework, platform ?? SampleConditionals.Always, platform is not null);
		});
		var samplesProvider = context.SyntaxProvider.ForAttributeWithMetadataName("Uno.Gallery.SamplePageAttribute",
			predicate: (_, _) => true,
			transform: Transform);

		context.RegisterSourceOutput(currentPlatformProvider, ReportUnsupportedTargetFramework);

		var filteredSamples = samplesProvider.Combine(currentPlatformProvider).Select(FilterSamples).Where(m => m is not null).Collect();
		context.RegisterSourceOutput(filteredSamples, GenerateCode);
	}

	private static void ReportUnsupportedTargetFramework(SourceProductionContext context, PlatformModel platform)
	{
		if (!platform.IsKnown)
		{
			var targetFramework = string.IsNullOrEmpty(platform.TargetFramework) ? "(empty)" : platform.TargetFramework;
			context.ReportDiagnostic(Diagnostic.Create(UnsupportedTargetFramework, Location.None, targetFramework));
		}
	}

	private static void GenerateCode(SourceProductionContext context, ImmutableArray<SamplesModel?> samples)
	{
		var builder = new StringBuilder();
		builder.AppendLine("""
			namespace Uno.Gallery
			{
				public partial class App
				{
					private static Sample[] GetAllSamples()
					{
						return new[]
						{
			""");

		foreach (var sample in samples)
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

	private static SamplesModel? FilterSamples((SamplesModel Left, PlatformModel Right) tuple, CancellationToken token)
	{
		var (sampleModel, platform) = tuple;
		if (ShouldBeDisplayed(sampleModel.Conditionals, platform.Platform))
		{
			return sampleModel;
		}

		return null;
	}

	private static bool ShouldBeDisplayed(SampleConditionals? conditionals, SampleConditionals compilationConditionals)
	{
		if (conditionals is null)
		{
			return true;
		}

		if (conditionals.Value.HasFlag(SampleConditionals.Disabled))
		{
			return false;
		}

		// note: the platform is a single flag, or Always when the target framework is not supported (see GALLERY0001)
		return (conditionals.Value & compilationConditionals) != 0;
	}

	private static SampleConditionals? GetSampleConditionalsFromTargetFramework(string? targetFramework)
	{
		// eg: net10.0-windows10.0.19041 -> windows
		var separatorIndex = targetFramework?.IndexOf('-') ?? -1;
		if (separatorIndex < 0)
		{
			return null;
		}

		var platform = targetFramework!.Substring(separatorIndex + 1);
		var versionIndex = platform.IndexOfAny("0123456789.".ToCharArray());
		if (versionIndex >= 0)
		{
			platform = platform.Substring(0, versionIndex);
		}

		return platform.ToLowerInvariant() switch
		{
			"android" => SampleConditionals.Droid,
			"ios" => SampleConditionals.iOS,
			"windows" => SampleConditionals.Windows,
			"desktop" => SampleConditionals.SkiaDesktop,
			"browserwasm" => SampleConditionals.Wasm,
			_ => null,
		};
	}

	private static SamplesModel Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		var attributedSymbol = (INamedTypeSymbol)context.TargetSymbol;
		SampleConditionals? conditionals = null;
		foreach (var attribute in attributedSymbol.GetAttributes())
		{
			if (attribute.AttributeClass?.Name == "SampleConditionalAttribute")
			{
				conditionals = (SampleConditionals)attribute.ConstructorArguments.Single().Value!;
			}
		}

		var samplePageAttribute = context.Attributes[0];
		Debug.Assert(samplePageAttribute.AttributeConstructor!.Parameters.Length == 4);
		Debug.Assert(samplePageAttribute.AttributeConstructor!.Parameters[0].Name == "category");
		Debug.Assert(samplePageAttribute.AttributeConstructor!.Parameters[1].Name == "title");
		Debug.Assert(samplePageAttribute.AttributeConstructor!.Parameters[2].Name == "source");
		Debug.Assert(samplePageAttribute.AttributeConstructor!.Parameters[3].Name == "glyph");
		var category = $"(global::Uno.Gallery.SampleCategory)({((int)samplePageAttribute.ConstructorArguments[0].Value!).ToString(CultureInfo.InvariantCulture)})";
		var title = (string)samplePageAttribute.ConstructorArguments[1].Value!;
		var source = $"(global::Uno.Gallery.SourceSdk)({((int)samplePageAttribute.ConstructorArguments[2].Value!).ToString(CultureInfo.InvariantCulture)})";
		var glyph = (string)samplePageAttribute.ConstructorArguments[3].Value!;

		var description = GetNamedArgumentOrDefault<string>(samplePageAttribute, "Description", null);
		var documentationLink = GetNamedArgumentOrDefault<string>(samplePageAttribute, "DocumentationLink", null);
		var dataType = GetNamedArgumentOrDefault<ISymbol>(samplePageAttribute, "DataType", null)?.ToDisplayString();
		var sortOrder = GetNamedArgumentOrDefault<int>(samplePageAttribute, "SortOrder", int.MaxValue);
		return new SamplesModel(conditionals, attributedSymbol.ToDisplayString(), category, title, description, documentationLink, dataType, source, glyph, sortOrder);
	}

	private static T? GetNamedArgumentOrDefault<T>(AttributeData samplePageAttribute, string argumentName, T? defaultValue)
	{
		foreach (var namedArgument in samplePageAttribute.NamedArguments)
		{
			if (namedArgument.Key == argumentName)
			{
				return (T)namedArgument.Value.Value!;
			}
		}

		return defaultValue;
	}
}
