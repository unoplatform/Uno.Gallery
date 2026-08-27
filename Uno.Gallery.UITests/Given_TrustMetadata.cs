using System;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// Trust-metadata integration tests: source links, status badges, owner/review text,
	/// and the direct-link copy button.  These tests require a UITest build where the Canary
	/// category is included in the sample catalog (<c>USE_UITESTS</c> preprocessor symbol).
	/// </summary>
	public class Given_TrustMetadata : TestBase
	{
		// ─── Source-link visibility ───────────────────────────────────────────

		/// <summary>
		/// A generated sample (Button) carries a SourcePath set by the source generator,
		/// which computes a SourceLink → <c>PART_SourceLinkText</c> must be visible.
		/// The URL uses the commit SHA when available, or "master" for local builds.
		/// </summary>
		[Test]
		public void When_GeneratedSample_SourceLinkTextIsVisible()
		{
			NavigateToSample("Button", "Material");

			App.WaitForElement("Material_FilledButton");

			TakeScreenshot("SourceLink_Visible");

			App.WaitForElement("PART_SourceLinkText");
		}

		// ─── Direct-link copy button ──────────────────────────────────────────

		/// <summary>
		/// The direct-link copy button is visible for generated samples and its Tag
		/// contains the canonical gallery share URL (slug = "button").
		/// </summary>
		[Test]
		public void When_GeneratedSample_DirectLinkButtonHasCanonicalSlug()
		{
			NavigateToSample("Button", "Material");

			App.WaitForElement("Material_FilledButton");

			TakeScreenshot("DirectLink_Button");

			App.WaitForElement("PART_DirectLinkCopyButton");

			var tag = new QueryEx(x => x.All().Marked("PART_DirectLinkCopyButton"))
				.GetDependencyPropertyValue<string>("Tag");

			Assert.That(tag, Does.Contain("#button"),
				"Direct-link copy Tag must be the canonical share URL containing the sample's slug");
			Assert.That(tag, Does.StartWith("https://gallery.platform.uno/#"),
				"Direct-link copy Tag must use the gallery.platform.uno deep-link prefix");
		}

		// ─── Status badge: stable sample ─────────────────────────────────────

		/// <summary>
		/// For a Stable sample (Button), the status badge is Collapsed and must not
		/// surface as visible content.  On DOM/WASM renderers a Collapsed element
		/// can remain in the accessibility tree; the test asserts the WinUI
		/// <c>Visibility</c> DP is <c>Collapsed</c> rather than relying on element
		/// absence, which is unreliable across renderers.
		/// </summary>
		[Test]
		public void When_StableSample_StatusBadgeIsCollapsed()
		{
			NavigateToSample("Button", "Material");

			App.WaitForElement("Material_FilledButton");

			TakeScreenshot("StatusBadge_Stable");

			// On DOM renderers (WASM), Collapsed elements remain in the automation tree.
			// Assert the Visibility DP value rather than element absence.
			var results = App.Query("PART_StatusBadge");
			if (results.Length > 0)
			{
				var visibility = new QueryEx(x => x.All().Marked("PART_StatusBadge"))
					.GetDependencyPropertyValue<string>("Visibility");
				Assert.That(visibility, Is.EqualTo("Collapsed"),
					"PART_StatusBadge Visibility must be Collapsed for Stable samples");
			}
			// On native renderers, Collapsed elements are absent from the a11y tree — correct behavior.
		}

		// ─── Status badge: Experimental (Diagnostics canary) ─────────────────

		/// <summary>
		/// The Diagnostics sample is annotated Experimental with owner/review metadata.
		/// After wrapping in SamplePageLayout, the header must expose:
		/// <list type="bullet">
		///   <item><c>PART_StatusBadge</c> visible with label "Experimental"</item>
		///   <item><c>PART_OwnerText</c> = "unoplatform/maintainers"</item>
		///   <item><c>PART_ReviewedOnText</c> = "2026-08-25"</item>
		/// </list>
		/// This test requires a USE_UITESTS build so the Canary category is not filtered.
		/// </summary>
		[Test]
		public void When_DiagnosticsCanary_ExperimentalBadgeAndOwnerReviewAreVisible()
		{
			NavigateToSample("Diagnostics");

			TakeScreenshot("Diagnostics_Metadata");

			App.WaitForElement("PART_StatusBadge");

			var statusText = new QueryEx(x => x.All().Marked("PART_StatusLabel"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(statusText, Is.EqualTo("Experimental"),
				"Status label must read 'Experimental' for the Diagnostics canary sample");

			App.WaitForElement("PART_OwnerText");
			var ownerText = new QueryEx(x => x.All().Marked("PART_OwnerText"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(ownerText, Is.EqualTo("unoplatform/maintainers"),
				"Owner text must match the annotation on CanarySamplePage");

			App.WaitForElement("PART_ReviewedOnText");
			var reviewedText = new QueryEx(x => x.All().Marked("PART_ReviewedOnText"))
				.GetDependencyPropertyValue<string>("Text");
			Assert.That(reviewedText, Is.EqualTo("2026-08-25"),
				"ReviewedOn text must match the annotation on CanarySamplePage");
		}

		// ─── Metadata collapsed for samples without owner/review ──────────────

		/// <summary>
		/// Button has no Owner or ReviewedOn annotation; both rows must be collapsed
		/// (not surfacing readable content).
		/// <para>
		/// <c>PART_OwnerText</c> / <c>PART_ReviewedOnText</c> are TextBlocks whose
		/// own <c>Visibility</c> DP is not directly bound; it is their parent
		/// StackPanel containers (<c>PART_OwnerRow</c> / <c>PART_ReviewedOnRow</c>)
		/// that own the <c>Collapsed</c> binding.  The test therefore checks the
		/// container's Visibility DP as a stable test hook.  On native renderers
		/// the collapsed containers are absent from the a11y tree; on DOM/WASM
		/// renderers the Visibility DP is read directly.
		/// </para>
		/// </summary>
		[Test]
		public void When_StableSampleWithNoOwner_OwnerAndReviewRowsAreCollapsed()
		{
			NavigateToSample("Button", "Material");

			App.WaitForElement("Material_FilledButton");

			TakeScreenshot("NoOwner_Collapsed");

			// PART_OwnerRow is the StackPanel whose Visibility is bound to Owner.
			// On DOM renderers it remains in the tree even when Collapsed.
			var ownerRow = App.Query("PART_OwnerRow");
			if (ownerRow.Length > 0)
			{
				var visibility = new QueryEx(x => x.All().Marked("PART_OwnerRow"))
					.GetDependencyPropertyValue<string>("Visibility");
				Assert.That(visibility, Is.EqualTo("Collapsed"),
					"PART_OwnerRow Visibility must be Collapsed when Owner is null");
			}

			// PART_ReviewedOnRow: same contract.
			var reviewRow = App.Query("PART_ReviewedOnRow");
			if (reviewRow.Length > 0)
			{
				var visibility = new QueryEx(x => x.All().Marked("PART_ReviewedOnRow"))
					.GetDependencyPropertyValue<string>("Visibility");
				Assert.That(visibility, Is.EqualTo("Collapsed"),
					"PART_ReviewedOnRow Visibility must be Collapsed when ReviewedOn is null");
			}
		}

		[Test]
		public void When_ContractV1Sample_DetailMetadataIsVisible()
		{
			NavigateToSample("Accessibility");

			App.WaitForElement("PART_CompatibilitySection");
			App.WaitForElement("PART_RequirementsText");
			App.WaitForElement("PART_AccessibilityNotesText");
			App.WaitForElement("PART_VariantsText");
			App.WaitForElement("PART_ResetBehaviorText");

			var requirements = new QueryEx(x => x.All().Marked("PART_RequirementsText"))
				.GetDependencyPropertyValue<string>("Text");
			var accessibility = new QueryEx(x => x.All().Marked("PART_AccessibilityNotesText"))
				.GetDependencyPropertyValue<string>("Text");
			var variants = new QueryEx(x => x.All().Marked("PART_VariantsText"))
				.GetDependencyPropertyValue<string>("Text");

			Assert.Multiple(() =>
			{
				Assert.That(requirements, Does.Contain("No permissions"));
				Assert.That(accessibility, Does.Contain("screen reader"));
				Assert.That(variants, Does.Contain("Live announcements"));
			});
		}
	}
}
