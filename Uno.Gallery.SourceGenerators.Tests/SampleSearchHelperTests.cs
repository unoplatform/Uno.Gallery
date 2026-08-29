using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uno.Gallery;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.SourceGenerators.Tests;

/// <summary>
/// Unit tests for <see cref="SampleSearchHelper.RankAndFilter"/>, the app-level adapter that
/// converts a query string and a sample catalog into a ranked, filtered array.
/// <para>
/// The helper and the scorer are linked as shared source; the <see cref="Sample"/> type is
/// supplied by <c>SampleStub.cs</c> — no WinUI or app-project dependency.
/// </para>
/// </summary>
[TestFixture]
public sealed class SampleSearchHelperTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

	private static Sample S(
		string title, string slug = "",
		string[]? tags = null,
		string? description = null,
		string? categoryCaption = null,
		string? sourceDescription = null,
		int? sortOrder = null,
		string? statusLabel = null)
		=> new Sample(
			title, slug.Length > 0 ? slug : title.ToLowerInvariant().Replace(' ', '-'),
			tags, description, categoryCaption, sourceDescription, sortOrder, statusLabel);

	private static IReadOnlyList<Sample> Catalog(params Sample[] samples)
		=> Array.AsReadOnly(samples);

	// ── Blank / whitespace / no-match → Array.Empty ───────────────────────────

	[Test]
	public void Blank_query_returns_empty()
	{
		var samples = Catalog(S("Button"));
		var result = SampleSearchHelper.RankAndFilter(samples, "");
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void Whitespace_query_returns_empty()
	{
		var samples = Catalog(S("Button"));
		var result = SampleSearchHelper.RankAndFilter(samples, "   ");
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void No_match_query_returns_empty()
	{
		var samples = Catalog(S("Button"), S("CheckBox"));
		var result = SampleSearchHelper.RankAndFilter(samples, "zzznomatch");
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void Empty_catalog_returns_empty()
	{
		var result = SampleSearchHelper.RankAndFilter(Array.Empty<Sample>(), "button");
		Assert.That(result, Is.Empty);
	}

	// ── Ranking: title exact outranks prefix/contains/description/category/source ──

	[Test]
	public void Title_exact_ranks_above_title_contains()
	{
		// "button-group" contains "button" as a substring; "Button" exact-matches.
		var exact    = S("Button",       "button");
		var contains = S("Button Group", "button-group");
		var catalog  = Catalog(contains, exact); // intentionally reversed

		var result = SampleSearchHelper.RankAndFilter(catalog, "Button");
		Assert.That(result[0].Slug, Is.EqualTo("button"),
			"Title exact match must be ranked first");
	}

	[Test]
	public void Title_contains_ranks_above_description_only()
	{
		// "alpha" in title → TitleContain; "beta" only in description → DescContain.
		var titleMatch = S("Alpha Control",  "alpha");
		var descMatch  = S("Other Control",  "other", description: "has alpha here");
		var catalog    = Catalog(descMatch, titleMatch);

		var result = SampleSearchHelper.RankAndFilter(catalog, "alpha");
		Assert.That(result[0].Slug, Is.EqualTo("alpha"),
			"Title contain must outrank description-only match");
	}

	[Test]
	public void Description_ranks_above_category_only()
	{
		var descMatch = S("Other A", "other-a", description: "mentions theming here");
		var catMatch  = S("Other B", "other-b", categoryCaption: "Theming");
		var catalog   = Catalog(catMatch, descMatch);

		var result = SampleSearchHelper.RankAndFilter(catalog, "theming");
		Assert.That(result[0].Slug, Is.EqualTo("other-a"),
			"Description match must outrank category-only match");
	}

	[Test]
	public void Category_and_source_rank_below_description()
	{
		// W_DescContain=50 > W_CatContain=W_SrcContain=30
		Assert.That(SampleSearchScorer.W_DescContain,
			Is.GreaterThan(SampleSearchScorer.W_CatContain));
		Assert.That(SampleSearchScorer.W_DescContain,
			Is.GreaterThan(SampleSearchScorer.W_SrcContain));
	}

	// ── Tie-breaking: SortOrder asc, then Title OrdinalIgnoreCase asc ─────────

	[Test]
	public void Tie_broken_by_SortOrder_ascending()
	{
		// Same title prefix → equal title score; SortOrder distinguishes.
		var low  = S("Alfa", "alfa", sortOrder: 1);
		var high = S("Beta", "beta", sortOrder: 2);
		// "a" matches both titles via TitleContain; neither has category/source.
		// Make them equal score by putting "a" in both titles at same position.
		var a = S("Aa", "aa", sortOrder: 2);
		var b = S("Ab", "ab", sortOrder: 1);
		var catalog = Catalog(a, b);

		var result = SampleSearchHelper.RankAndFilter(catalog, "a");
		// Both match via TitleContain; b has lower SortOrder so comes first.
		Assert.That(result[0].Slug, Is.EqualTo("ab"), "Lower SortOrder should rank first on tie");
	}

	[Test]
	public void Tie_broken_by_Title_OrdinalIgnoreCase_when_SortOrder_equal()
	{
		var apple  = S("apple",  "apple",  sortOrder: 1);
		var banana = S("banana", "banana", sortOrder: 1);
		// "a" matches both; same SortOrder; title "apple" < "banana" alphabetically.
		var catalog = Catalog(banana, apple);

		var result = SampleSearchHelper.RankAndFilter(catalog, "a");
		Assert.That(result[0].Slug, Is.EqualTo("apple"),
			"Title OrdinalIgnoreCase ascending should resolve a SortOrder tie");
	}

	[Test]
	public void Null_SortOrder_sorts_last_among_tied_scores()
	{
		var withOrder    = S("Aardvark", "aardvark",   sortOrder: 5);
		var withoutOrder = S("Aardwolf", "aardwolf",   sortOrder: null);
		var catalog = Catalog(withoutOrder, withOrder);

		var result = SampleSearchHelper.RankAndFilter(catalog, "a");
		Assert.That(result[0].Slug, Is.EqualTo("aardvark"),
			"Explicit SortOrder must precede null SortOrder on a score tie");
	}

	// ── AND logic ──────────────────────────────────────────────────────────────

	[Test]
	public void AND_all_terms_must_match()
	{
		// "button" matches title; "toolkit" matches source on second; no match on first.
		var first  = S("Button",        "button");
		var second = S("Toolkit Button","toolkit-button", sourceDescription: "Uno.Toolkit");
		var catalog = Catalog(first, second);

		var result = SampleSearchHelper.RankAndFilter(catalog, "button toolkit");
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0].Slug, Is.EqualTo("toolkit-button"));
	}

	[Test]
	public void AND_duplicate_query_terms_deduplicated_deterministically()
	{
		// "button button" should behave like "button" — duplicate term is dropped.
		var catalog = Catalog(S("Button", "button"), S("CheckBox", "checkbox"));

		var r1 = SampleSearchHelper.RankAndFilter(catalog, "button");
		var r2 = SampleSearchHelper.RankAndFilter(catalog, "button button");
		Assert.That(r1.Length, Is.EqualTo(r2.Length));
		Assert.That(r1[0].Slug, Is.EqualTo(r2[0].Slug));
	}

	[Test]
	public void Deterministic_on_repeated_invocations()
	{
		var catalog = Catalog(
			S("CheckBox",    "checkbox",    sortOrder: 2),
			S("Button",      "button",      sortOrder: 1),
			S("RadioButton", "radiobutton", sortOrder: 3));

		var r1 = SampleSearchHelper.RankAndFilter(catalog, "button");
		var r2 = SampleSearchHelper.RankAndFilter(catalog, "button");

		Assert.That(r1.Length, Is.EqualTo(r2.Length));
		for (int i = 0; i < r1.Length; i++)
			Assert.That(r1[i].Slug, Is.EqualTo(r2[i].Slug),
				$"Position {i} must be stable across identical calls");
	}

	// ── Category-only matches ─────────────────────────────────────────────────

	[Test]
	public void Category_only_query_returns_categorized_samples()
	{
		// "theming" matches CategoryCaption="Theming" but not any title/slug/description/source.
		var theming    = S("Palette",    "palette",    categoryCaption: "Theming");
		var styling    = S("Styles",     "styles",     categoryCaption: "Theming");
		var unrelated  = S("Button",     "button",     categoryCaption: "UI Components");
		var catalog    = Catalog(unrelated, theming, styling);

		var result = SampleSearchHelper.RankAndFilter(catalog, "theming");
		var slugs = new HashSet<string>(StringComparer.Ordinal);
		foreach (var s in result) slugs.Add(s.Slug);

		Assert.That(slugs, Contains.Item("palette"));
		Assert.That(slugs, Contains.Item("styles"));
		Assert.That(slugs, Does.Not.Contain("button"),
			"Category 'UI Components' must not match query 'theming'");
	}

	// ── Source-only matches ───────────────────────────────────────────────────

	[Test]
	public void Source_only_query_with_punctuation_returns_sourced_samples()
	{
		// "uno.winui" contains a dot — present in SourceDescription "Uno.WinUI" but in no title.
		var winui     = S("Slider",  "slider",   sourceDescription: "Uno.WinUI");
		var toolkit   = S("TabBar",  "tabbar",   sourceDescription: "Uno.Toolkit");
		var catalog   = Catalog(toolkit, winui);

		var result = SampleSearchHelper.RankAndFilter(catalog, "uno.winui");
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0].Slug, Is.EqualTo("slider"),
			"Dot-punctuated source query must match only the Uno.WinUI sample");
	}

	[Test]
	public void Source_only_query_uno_toolkit_matches_toolkit_samples()
	{
		var toolkit = S("TabBar", "tabbar", sourceDescription: "Uno.Toolkit");
		var winui   = S("Button", "button", sourceDescription: "Uno.WinUI");
		var catalog = Catalog(winui, toolkit);

		var result = SampleSearchHelper.RankAndFilter(catalog, "uno.toolkit");
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0].Slug, Is.EqualTo("tabbar"));
	}

	// ── Status-only matches ───────────────────────────────────────────────────

	[Test]
	public void Status_exact_query_returns_matching_status_sample()
	{
		// "experimental" exact-matches StatusLabel="Experimental" on the first sample only.
		var experimental = S("Diagnostics", "diagnostics", statusLabel: "Experimental");
		var stable       = S("Button",      "button");
		var catalog      = Catalog(stable, experimental);

		var result = SampleSearchHelper.RankAndFilter(catalog, "experimental");
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0].Slug, Is.EqualTo("diagnostics"));
	}

	[Test]
	public void Status_contains_query_returns_matching_status_sample()
	{
		// "experi" is not an exact match but is contained in "Experimental".
		var experimental = S("Diagnostics", "diagnostics", statusLabel: "Experimental");
		var stable       = S("Button",      "button");
		var catalog      = Catalog(stable, experimental);

		var result = SampleSearchHelper.RankAndFilter(catalog, "experi");
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0].Slug, Is.EqualTo("diagnostics"));
	}

	[Test]
	public void Status_query_excludes_stable_samples()
	{
		// "preview" matches Preview sample; Stable sample must be excluded.
		var preview = S("Feature A", "feature-a", statusLabel: "Preview");
		var stable  = S("Button",    "button");
		var catalog = Catalog(stable, preview);

		var result = SampleSearchHelper.RankAndFilter(catalog, "preview");
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0].Slug, Is.EqualTo("feature-a"),
			"Stable samples (empty StatusLabel) must not match a status query");
	}

	[Test]
	public void Status_ranks_below_description_above_category()
	{
		// W_StatusExact(45) < W_DescContain(50): sample with description match outranks status match.
		var descMatch   = S("Other A", "other-a", description: "experimental feature set");
		var statusMatch = S("Other B", "other-b", statusLabel: "Experimental");
		var catalog     = Catalog(statusMatch, descMatch);

		var result = SampleSearchHelper.RankAndFilter(catalog, "experimental");
		Assert.That(result[0].Slug, Is.EqualTo("other-a"),
			"Description match (W_DescContain=50) must outrank status-exact match (W_StatusExact=45)");
	}

	// ── SearchAccessibleName ──────────────────────────────────────────────────

	[Test]
	public void SearchAccessibleName_title_only_when_no_category_no_status()
	{
		var sample = S("Button", "button");
		Assert.That(sample.SearchAccessibleName, Is.EqualTo("Button"));
	}

	[Test]
	public void SearchAccessibleName_includes_categoryCaption_when_present()
	{
		var sample = S("Palette", "palette", categoryCaption: "Theming");
		Assert.That(sample.SearchAccessibleName, Is.EqualTo("Palette, Theming"));
	}

	[Test]
	public void SearchAccessibleName_includes_statusLabel_when_non_stable()
	{
		var sample = S("Diagnostics", "diagnostics", statusLabel: "Experimental");
		Assert.That(sample.SearchAccessibleName, Is.EqualTo("Diagnostics, Experimental"));
	}

	[Test]
	public void SearchAccessibleName_includes_all_three_parts()
	{
		var sample = S("Diagnostics", "diagnostics", categoryCaption: "Canary", statusLabel: "Experimental");
		Assert.That(sample.SearchAccessibleName, Is.EqualTo("Diagnostics, Canary, Experimental"));
	}

	[Test]
	public void SearchAccessibleName_sentinel_returns_title_only()
	{
		// Sentinel: SampleCategory.None → empty categoryCaption, Stable → empty statusLabel.
		var sentinel = S("No suggestions found", "no-suggestions-found");
		Assert.That(sentinel.SearchAccessibleName, Is.EqualTo("No suggestions found"));
	}
}
