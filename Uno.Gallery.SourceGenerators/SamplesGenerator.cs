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

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var currentPlatformProvider = context.ParseOptionsProvider.Select((options, _) => GetSampleConditionalsFromPreprocessorSymbolNames(options.PreprocessorSymbolNames));
		var samplesProvider = context.SyntaxProvider.ForAttributeWithMetadataName("Uno.Gallery.SamplePageAttribute",
			predicate: (_, _) => true,
			transform: Transform);

		var filteredSamples = samplesProvider.Combine(currentPlatformProvider).Select(FilterSamples).Where(m => m is not null).Collect();
		context.RegisterSourceOutput(filteredSamples, GenerateCode);
	}

	private static void GenerateCode(SourceProductionContext context, ImmutableArray<SamplesModel?> samples)
	{
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

	private static SamplesModel? FilterSamples((SamplesModel Left, SampleConditionals Right) tuple, CancellationToken token)
	{
		var (sampleModel, compilationConditional) = tuple;
		if (ShouldBeDisplayed(sampleModel.Conditionals, compilationConditional))
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

		return conditionals.Value.HasFlag(compilationConditionals);
	}

	private static SampleConditionals GetSampleConditionalsFromPreprocessorSymbolNames(IEnumerable<string> preprocessorSymbolNames)
	{
		foreach (var preprocessorSymbol in preprocessorSymbolNames)
		{
			if (preprocessorSymbol == "WINDOWS")
			{
				return SampleConditionals.Windows;
			}
			else if (preprocessorSymbol == "__MACOS__")
			{
				return SampleConditionals.macOS;
			}
			else if (preprocessorSymbol == "__IOS__")
			{
				return SampleConditionals.iOS;
			}
			else if (preprocessorSymbol == "__WASM__")
			{
				return SampleConditionals.Wasm;
			}
			else if (preprocessorSymbol == "HAS_UNO_SKIA_GTK")
			{
				return SampleConditionals.SkiaGtk;
			}
		}

		return SampleConditionals.Always;
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
