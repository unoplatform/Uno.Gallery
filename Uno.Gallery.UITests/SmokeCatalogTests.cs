using NUnit.Framework;

namespace Uno.Gallery.UITests;

[Category("Smoke")]
public sealed class SmokeCatalogTests
{
	[Test]
	public void Parse_filters_non_stable_entries_and_preserves_manifest_order()
	{
		const string json = """
			{
			  "schemaVersion": 2,
			  "samples": [
			    { "fqn": "Gallery.A", "slug": "z-slug", "title": "A", "status": { "value": 0, "name": "Stable" } },
			    { "fqn": "Gallery.B", "slug": "preview", "title": "B", "status": { "value": 1, "name": "Preview" } },
			    { "fqn": "Gallery.C", "slug": "a-slug", "title": "C", "status": { "value": 0, "name": "Stable" } }
			  ]
			}
			""";

		var samples = SmokeCatalog.ParseStableSamples(json);

		Assert.That(samples.Select(x => x.Slug), Is.EqualTo(new[] { "z-slug", "a-slug" }));
	}

	[Test]
	public void Parse_rejects_non_deterministic_target_manifest()
	{
		const string json = """
			{
			  "schemaVersion": 1,
			  "samples": [
			    { "fqn": "Gallery.Z", "slug": "z", "status": { "value": 0, "name": "Stable" } },
			    { "fqn": "Gallery.A", "slug": "a", "status": { "value": 0, "name": "Stable" } }
			  ]
			}
			""";

		Assert.That(
			() => SmokeCatalog.ParseStableSamples(json),
			Throws.TypeOf<InvalidDataException>().With.Message.Contains("deterministically ordered"));
	}

	[TestCase(99, "99")]
	[TestCase(0, "Preview")]
	public void Parse_rejects_unknown_or_inconsistent_status(int value, string name)
	{
		var json = $$"""
			{
			  "schemaVersion": 2,
			  "samples": [
			    { "fqn": "Gallery.A", "slug": "a", "status": { "value": {{value}}, "name": "{{name}}" } }
			  ]
			}
			""";

		Assert.That(
			() => SmokeCatalog.ParseStableSamples(json),
			Throws.TypeOf<InvalidDataException>().With.Message.Contains("unknown or inconsistent status"));
	}

	[Test]
	public void FormatFailures_aggregates_every_sample_with_slug_and_design()
	{
		var failures = new[]
		{
			new SmokeFailure("button", "Material", "first"),
			new SmokeFailure("textbox", "Material", "second"),
		};

		var message = SmokeCatalog.FormatFailures(failures);

		Assert.That(message, Does.StartWith("2 sample(s) failed:"));
		Assert.That(message, Does.Contain("[button/Material] first"));
		Assert.That(message, Does.Contain("[textbox/Material] second"));
	}
}
