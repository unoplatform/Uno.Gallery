using Newtonsoft.Json;

namespace Uno.Gallery.UITests;

internal static class SmokeCatalog
{
	public static IReadOnlyList<SmokeSample> ParseStableSamples(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			throw new InvalidDataException("The running app returned an empty sample manifest.");
		}

		var manifest = JsonConvert.DeserializeObject<SmokeManifest>(json)
			?? throw new InvalidDataException("The running app returned an invalid sample manifest.");

		if (manifest.SchemaVersion != 1)
		{
			throw new InvalidDataException(
				$"Unsupported sample manifest schema {manifest.SchemaVersion}; expected 1.");
		}

		if (manifest.Samples is null)
		{
			throw new InvalidDataException("The sample manifest has no samples array.");
		}

		var previousFqn = string.Empty;
		var seenSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var stable = new List<SmokeSample>();

		foreach (var sample in manifest.Samples)
		{
			if (string.IsNullOrWhiteSpace(sample.Fqn) || string.IsNullOrWhiteSpace(sample.Slug))
			{
				throw new InvalidDataException("Every manifest sample must have a non-empty fqn and slug.");
			}

			if (string.CompareOrdinal(previousFqn, sample.Fqn) > 0)
			{
				throw new InvalidDataException(
					$"The target manifest is not deterministically ordered: '{sample.Fqn}' follows '{previousFqn}'.");
			}

			previousFqn = sample.Fqn;

			if (!seenSlugs.Add(sample.Slug))
			{
				throw new InvalidDataException($"Duplicate target manifest slug '{sample.Slug}'.");
			}

			if (sample.Status?.Value == 0 &&
				string.Equals(sample.Status.Name, "Stable", StringComparison.Ordinal))
			{
				stable.Add(sample);
			}
		}

		return stable;
	}

	public static string FormatFailures(IReadOnlyCollection<SmokeFailure> failures)
		=> failures.Count == 0
			? string.Empty
			: $"{failures.Count} sample(s) failed:{Environment.NewLine}" +
			  string.Join(
				  Environment.NewLine,
				  failures.Select(f => $"[{f.Slug}/{f.Design}] {f.Details}"));
}

internal sealed class SmokeManifest
{
	[JsonProperty("schemaVersion")]
	public int SchemaVersion { get; set; }

	[JsonProperty("samples")]
	public List<SmokeSample>? Samples { get; set; }
}

internal sealed class SmokeSample
{
	[JsonProperty("fqn")]
	public string Fqn { get; set; } = string.Empty;

	[JsonProperty("slug")]
	public string Slug { get; set; } = string.Empty;

	[JsonProperty("title")]
	public string Title { get; set; } = string.Empty;

	[JsonProperty("status")]
	public SmokeEnumValue? Status { get; set; }
}

internal sealed class SmokeEnumValue
{
	[JsonProperty("value")]
	public int Value { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;
}

internal sealed record SmokeFailure(string Slug, string Design, string Details);
