using System;
using NUnit.Framework;

namespace Uno.Gallery.UITests;

/// <summary>
/// Tests for the shell search box: ranked results by slug, category, and source;
/// suggestion AutomationId binding; no-match sentinel; and keyboard/tap navigation.
/// All element IDs are stable slug values bound via AutomationProperties.AutomationId on the
/// suggestion Grid root, so they survive title renames as long as slugs remain unchanged.
/// </summary>
public class Given_SearchNavigation : TestBase
{
	[SetUp]
	public void EnsureOverviewIsVisible() => NavigateToSample("Overview", "Material");

	// ── Slug-based search ─────────────────────────────────────────────────────

	[Test]
	public void When_Search_By_Exact_Title_Shows_Slug_Suggestion()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		App.EnterText("SamplesSearchBox", "CheckBox");

		// AutomationId is bound to Sample.Slug; "checkbox" is the stable slug for "CheckBox"
		App.WaitForElement("checkbox");
	}

	[Test]
	public void When_Slug_Search_Tapped_Navigates_To_FAB_Page()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		// "floating-action-button" is the slug for "Floating Action Button" (DeriveSlug of title).
		// Slug-exact scoring places it top of the suggestion list.
		App.EnterText("SamplesSearchBox", "floating-action-button");
		App.WaitThenTap("floating-action-button");

		// Assert the FAB sample page is loaded: Material_FAB_Create is the primary FAB button.
		App.WaitForElement("Material_FAB_Create", timeout: TimeSpan.FromSeconds(60));
	}

	// ── Category-based search ─────────────────────────────────────────────────

	[Test]
	public void When_Search_By_Category_Caption_Word_Shows_Theming_Sample()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		// "Theming" is the exact CategoryCaption for SampleCategory.Theming;
		// suggestions that appear include any sample whose CategoryCaption contains this word.
		App.EnterText("SamplesSearchBox", "Theming");

		// "lightweight-styling" is the stable slug for "Lightweight Styling" (SampleCategory.Theming)
		App.WaitForElement("lightweight-styling");
	}

	// ── Source-based search ───────────────────────────────────────────────────

	[Test]
	public void When_Search_By_SourceDescription_With_Dot_Shows_Toolkit_Sample()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		// "Uno.Toolkit" is the SourceDescription for SourceSdk.UnoToolkit.
		// The dot in "uno.toolkit" makes this query unique to SourceDescription — no sample
		// title contains a dot — so suggestions that appear are Toolkit samples.
		App.EnterText("SamplesSearchBox", "uno.toolkit");

		// "tabbar" is the stable slug for "TabBar" (SourceSdk.UnoToolkit, SampleCategory.Toolkit)
		App.WaitForElement("tabbar");
	}

	// ── No-match sentinel ─────────────────────────────────────────────────────

	[Test]
	public void When_Search_Matches_Nothing_Shows_Sentinel()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		App.EnterText("SamplesSearchBox", "zzznomatchqueryyy");

		// The shell injects a sentinel sample when the search helper returns nothing.
		// Its slug is derived from "No suggestions found" → "no-suggestions-found".
		App.WaitForElement("no-suggestions-found");
	}

	// ── Accessibility: AutomationId from slug ─────────────────────────────────

	[Test]
	public void When_Search_Suggestion_Has_AutomationId_Equal_To_Slug()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		App.EnterText("SamplesSearchBox", "Button");

		// The suggestion Grid carries AutomationId="{x:Bind Slug}"; the slug for "Button" is "button"
		App.WaitForElement("button");
	}

	// ── Status-based search ───────────────────────────────────────────────────

	/// <summary>
	/// In DEBUG, IS_CANARY_BUILD, and USE_UITESTS builds the Canary category is not filtered
	/// out of the sample catalog.  "Diagnostics" has Status = SampleStatus.Experimental, so
	/// StatusLabel = "Experimental".  Querying "experimental" must return it via StatusExact scoring.
	/// </summary>
	[Test]
	public void When_Search_Experimental_Shows_Diagnostics_In_UITest_Catalog()
	{
		OpenNavView();
		App.WaitThenTap("SamplesSearchBox");
		App.ClearText("SamplesSearchBox");
		App.EnterText("SamplesSearchBox", "experimental");

		// "diagnostics" is the slug for the Diagnostics sample (SampleCategory.Canary, Status=Experimental).
		App.WaitForElement("diagnostics");
	}
}
