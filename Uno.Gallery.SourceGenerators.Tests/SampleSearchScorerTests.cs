using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.SourceGenerators.Tests;

/// <summary>
/// Focused unit tests for <see cref="SampleSearchScorer"/> covering weight ordering, AND logic,
/// empty/miss inputs, tag matching, description, category, source, ties, and case-insensitivity.
/// The scorer is linked into this project via a shared-source &lt;Compile Include&gt; so there
/// is no WinUI or Sample runtime dependency here.
/// </summary>
[TestFixture]
public sealed class SampleSearchScorerTests
{
	private static readonly IReadOnlyList<string> NoTags = Array.Empty<string>();

	/// <summary>Helper that pre-processes the query and calls Score.</summary>
	private static int Score(
		string query,
		string title,
		string slug = "",
		IReadOnlyList<string>? tags = null,
		string? description = null,
		string? category = null,
		string? source = null,
		string? statusLabel = null)
	{
		var terms = SampleSearchScorer.SplitTerms(query);
		return SampleSearchScorer.Score(terms, title, slug, tags ?? NoTags, description, category, source, statusLabel);
	}

	// ── SplitTerms ────────────────────────────────────────────────────────────

	[Test]
	public void SplitTerms_empty_returns_empty() =>
		Assert.That(SampleSearchScorer.SplitTerms(""), Is.Empty);

	[Test]
	public void SplitTerms_whitespace_returns_empty() =>
		Assert.That(SampleSearchScorer.SplitTerms("   "), Is.Empty);

	[Test]
	public void SplitTerms_null_returns_empty() =>
		Assert.That(SampleSearchScorer.SplitTerms(null), Is.Empty);

	[Test]
	public void SplitTerms_lowercases_terms() =>
		Assert.That(SampleSearchScorer.SplitTerms("Button"), Is.EqualTo(new[] { "button" }));

	[Test]
	public void SplitTerms_deduplicates_identical_terms() =>
		Assert.That(SampleSearchScorer.SplitTerms("button button button"), Has.Length.EqualTo(1));

	[Test]
	public void SplitTerms_deduplicates_case_variants() =>
		Assert.That(SampleSearchScorer.SplitTerms("Button BUTTON"), Has.Length.EqualTo(1));

	[Test]
	public void SplitTerms_splits_on_whitespace() =>
		Assert.That(SampleSearchScorer.SplitTerms("a b c"), Has.Length.EqualTo(3));

	// ── Empty / no-match guard ────────────────────────────────────────────────

	[Test]
	public void Score_with_empty_terms_array_returns_minus_one() =>
		Assert.That(SampleSearchScorer.Score(Array.Empty<string>(), "Button", "button", NoTags, null, null, null, null),
			Is.EqualTo(-1));

	[Test]
	public void Score_term_matching_no_field_returns_minus_one() =>
		Assert.That(Score("zzznomatch", "Button", "button"), Is.EqualTo(-1));

	// ── AND logic ─────────────────────────────────────────────────────────────

	[Test]
	public void AND_one_miss_term_excludes_sample()
	{
		// "button" matches title; "zzz" matches nothing → excluded
		Assert.That(Score("button zzz", "Button", "button"), Is.EqualTo(-1));
	}

	[Test]
	public void AND_both_terms_matching_different_fields_scores_positive()
	{
		// "button" matches title; "toolkit" matches source
		Assert.That(Score("button toolkit", "Button", "button", source: "Uno.Toolkit"), Is.GreaterThan(0));
	}

	// ── Title weight tiers ────────────────────────────────────────────────────

	[Test]
	public void Title_exact_scores_W_TitleExact()
	{
		// slug = "zzz" so no slug contribution; only title exact
		Assert.That(Score("button", "Button", "zzz"), Is.EqualTo(SampleSearchScorer.W_TitleExact));
	}

	[Test]
	public void Title_prefix_scores_W_TitlePrefix()
	{
		Assert.That(Score("butt", "Button", "zzz"), Is.EqualTo(SampleSearchScorer.W_TitlePrefix));
	}

	[Test]
	public void Title_contain_scores_W_TitleContain()
	{
		// "utt" is contained in "Button" but does not start it
		Assert.That(Score("utt", "Button", "zzz"), Is.EqualTo(SampleSearchScorer.W_TitleContain));
	}

	[Test]
	public void Title_exact_gt_prefix_gt_contain_gt_slug_exact()
	{
		Assert.That(SampleSearchScorer.W_TitleExact,   Is.GreaterThan(SampleSearchScorer.W_TitlePrefix));
		Assert.That(SampleSearchScorer.W_TitlePrefix,  Is.GreaterThan(SampleSearchScorer.W_TitleContain));
		Assert.That(SampleSearchScorer.W_TitleContain, Is.GreaterThan(SampleSearchScorer.W_SlugExact));
	}
	// ── Slug weight tiers ─────────────────────────────────────────────────────

	[Test]
	public void Slug_exact_scores_W_SlugExact_when_title_misses()
	{
		// title = "My Control", slug = "my-slug", term = "my-slug"
		// title doesn't match "my-slug" → only slug exact
		Assert.That(Score("my-slug", "My Control", "my-slug"), Is.EqualTo(SampleSearchScorer.W_SlugExact));
	}

	[Test]
	public void Slug_contain_scores_W_SlugContain_when_title_misses()
	{
		// "slug" is in slug "my-slug" but not in title "Other Title"
		Assert.That(Score("slug", "Other Title", "my-slug"), Is.EqualTo(SampleSearchScorer.W_SlugContain));
	}

	[Test]
	public void Slug_exact_gt_slug_contain()
	{
		Assert.That(SampleSearchScorer.W_SlugExact, Is.GreaterThan(SampleSearchScorer.W_SlugContain));
	}

	// ── Tag weight tiers ──────────────────────────────────────────────────────

	[Test]
	public void Tag_exact_scores_W_TagExact_when_other_fields_miss()
	{
		Assert.That(Score("input", "Other", "zzz", new[] { "input" }), Is.EqualTo(SampleSearchScorer.W_TagExact));
	}

	[Test]
	public void Tag_contain_scores_W_TagContain_when_other_fields_miss()
	{
		// "inp" is in tag "input" but doesn't exact-match
		Assert.That(Score("inp", "Other", "zzz", new[] { "input" }), Is.EqualTo(SampleSearchScorer.W_TagContain));
	}

	[Test]
	public void Tag_exact_beats_tag_contain()
	{
		int exact   = Score("input", "Other", "zzz", new[] { "input" });
		int contain = Score("inp",   "Other", "zzz", new[] { "input" });
		Assert.That(exact, Is.GreaterThan(contain));
	}

	[Test]
	public void Tag_exact_wins_over_earlier_contain()
	{
		// First tag "input-event" contains "input"; second tag "input" exact-matches
		int scoreWithExact   = Score("input", "Other", "zzz", new[] { "input-event", "input" });
		int scoreContainOnly = Score("input", "Other", "zzz", new[] { "input-event" });
		Assert.That(scoreWithExact, Is.GreaterThan(scoreContainOnly));
	}

	// ── Description weight ────────────────────────────────────────────────────

	[Test]
	public void Description_match_scores_W_DescContain()
	{
		Assert.That(Score("xyzword", "Other", "zzz", description: "has xyzword here"),
			Is.EqualTo(SampleSearchScorer.W_DescContain));
	}

	// ── Category weight ───────────────────────────────────────────────────────

	[Test]
	public void Category_match_scores_W_CatContain()
	{
		// "theming" is contained in category caption "Theming"
		Assert.That(Score("theming", "Other", "zzz", category: "Theming"),
			Is.EqualTo(SampleSearchScorer.W_CatContain));
	}

	// ── Source weight ─────────────────────────────────────────────────────────

	[Test]
	public void Source_match_scores_W_SrcContain()
	{
		// "material" is contained in source description "Uno.Material"
		Assert.That(Score("material", "Other", "zzz", source: "Uno.Material"),
			Is.EqualTo(SampleSearchScorer.W_SrcContain));
	}

	// ── Weight hierarchy (slug/tag tier below title; desc below slug/tag; cat/src below desc) ──

	[Test]
	public void Slug_exact_greater_than_description()
	{
		int slugScore = Score("foo", "OtherTitle", "foo");   // slug-exact only
		int descScore = Score("foo", "OtherTitle", "zzz", description: "mentions foo here");
		Assert.That(slugScore, Is.GreaterThan(descScore));
	}

	[Test]
	public void Description_greater_than_category_and_source()
	{
		Assert.That(SampleSearchScorer.W_DescContain, Is.GreaterThan(SampleSearchScorer.W_CatContain));
		Assert.That(SampleSearchScorer.W_DescContain, Is.GreaterThan(SampleSearchScorer.W_SrcContain));
	}

	// ── Status weight ─────────────────────────────────────────────────────────

	[Test]
	public void Status_exact_scores_W_StatusExact()
	{
		// title, slug, tags don't match "experimental"; only statusLabel does
		Assert.That(Score("experimental", "Other", "zzz", statusLabel: "Experimental"),
			Is.EqualTo(SampleSearchScorer.W_StatusExact));
	}

	[Test]
	public void Status_contain_scores_W_StatusContain()
	{
		// "experi" is not an exact match for "Experimental" but is contained in it
		Assert.That(Score("experi", "Other", "zzz", statusLabel: "Experimental"),
			Is.EqualTo(SampleSearchScorer.W_StatusContain));
	}

	[Test]
	public void Status_exact_beats_status_contain()
	{
		int exact   = Score("experimental", "Other", "zzz", statusLabel: "Experimental");
		int contain = Score("experi",       "Other", "zzz", statusLabel: "Experimental");
		Assert.That(exact, Is.GreaterThan(contain));
	}

	[Test]
	public void Status_hierarchy_desc_gt_status_gt_cat_src()
	{
		// DescContain > StatusExact > StatusContain > CatContain = SrcContain
		Assert.That(SampleSearchScorer.W_DescContain,   Is.GreaterThan(SampleSearchScorer.W_StatusExact));
		Assert.That(SampleSearchScorer.W_StatusExact,   Is.GreaterThan(SampleSearchScorer.W_StatusContain));
		Assert.That(SampleSearchScorer.W_StatusContain, Is.GreaterThan(SampleSearchScorer.W_CatContain));
		Assert.That(SampleSearchScorer.W_StatusContain, Is.GreaterThan(SampleSearchScorer.W_SrcContain));
	}

	[Test]
	public void Status_miss_does_not_contribute()
	{
		// Stable sample has empty statusLabel → no status score at all
		int withoutStatus = Score("preview", "Other", "zzz", statusLabel: null);
		Assert.That(withoutStatus, Is.EqualTo(-1));
	}

	// ── Case insensitivity ────────────────────────────────────────────────────

	[Test]
	public void Title_match_is_case_insensitive()
	{
		int lower = Score("button", "Button", "zzz");
		int upper = Score("BUTTON", "Button", "zzz");
		Assert.That(lower, Is.EqualTo(upper));
	}

	[Test]
	public void Tag_match_is_case_insensitive()
	{
		int lower = Score("input", "Other", "zzz", new[] { "Input" });
		int upper = Score("INPUT", "Other", "zzz", new[] { "input" });
		Assert.That(lower, Is.EqualTo(upper));
	}

	// ── Combined / stacking ───────────────────────────────────────────────────

	[Test]
	public void Title_exact_and_slug_exact_stack()
	{
		// "checkbox" exact-matches title "CheckBox" and slug "checkbox"
		int combined = Score("checkbox", "CheckBox", "checkbox");
		Assert.That(combined, Is.EqualTo(SampleSearchScorer.W_TitleExact + SampleSearchScorer.W_SlugExact));
	}

	[Test]
	public void Multi_term_scores_accumulate_across_fields()
	{
		// "button toolkit" → "button" hits title exact, "toolkit" hits source
		int combined   = Score("button toolkit", "Button", "zzz", source: "Uno.Toolkit");
		int titleOnly  = Score("button",          "Button", "zzz");
		int sourceOnly = Score("toolkit",          "Other",  "zzz", source: "Uno.Toolkit");
		Assert.That(combined, Is.EqualTo(titleOnly + sourceOnly));
	}
}
