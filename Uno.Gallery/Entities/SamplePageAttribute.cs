using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Uno.Gallery
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class SamplePageAttribute : Attribute
	{
		public SamplePageAttribute(SampleCategory category, string title, SourceSdk source = SourceSdk.WinUI, string glyph = "")
		{
			Category = category;
			Title = title;
			Glyph = glyph;
			Source = source;
		}

		/// <summary>
		/// Sample category with null reserved for Home/Overview.
		/// </summary>
		public SampleCategory Category { get; }
		
		public string Title { get; }

		public string Description { get; set; }

		public string Glyph { get; }

		public string DocumentationLink { get; set; }

		[DynamicallyAccessedMembers(Sample.ViewRequirements)]
		public Type DataType { get; set; }

		public SourceSdk Source { get; }

		/// <summary>
		/// Sort order with the same <see cref="Category"/>.
		/// </summary>
		public int SortOrder { get; set; } = int.MaxValue;

		/// <summary>
		/// URL-friendly identifier for this sample.
		/// Must be lowercase ASCII alphanumeric with interior hyphens only
		/// (e.g. <c>"my-control"</c>). Derived from <see cref="Title"/> if not set.
		/// </summary>
		public string? Slug { get; set; }

		/// <summary>
		/// Categorization tags for filtering and search (e.g. <c>"layout"</c>, <c>"input"</c>).
		/// </summary>
		public string[]? Tags { get; set; }

		/// <summary>
		/// Production-readiness indicator. Defaults to <see cref="SampleStatus.Stable"/>.
		/// </summary>
		public SampleStatus Status { get; set; } = SampleStatus.Stable;

		/// <summary>
		/// GitHub user or team slug of the person or group responsible for maintaining this sample
		/// (e.g. <c>"username"</c> or <c>"org/team-name"</c>).
		/// </summary>
		public string? Owner { get; set; }

		/// <summary>
		/// Date of the last quality review in ISO 8601 format (<c>YYYY-MM-DD</c>).
		/// </summary>
		public string? ReviewedOn { get; set; }

		/// <summary>
		/// Version of the enforceable sample-detail contract authored by this sample.
		/// Zero identifies legacy metadata that has not yet been reviewed.
		/// </summary>
		public int ContractVersion { get; set; }

		/// <summary>Design systems intentionally demonstrated by this sample.</summary>
		public SampleDesigns SupportedDesigns { get; set; }

		/// <summary>Rendering backends on which this sample is intentionally supported.</summary>
		public SampleRenderers SupportedRenderers { get; set; }

		/// <summary>Permissions, network access, hardware, or setup required to use the sample.</summary>
		public string[]? Requirements { get; set; }

		/// <summary>Keyboard, screen-reader, contrast, motion, or other accessibility guidance.</summary>
		public string[]? AccessibilityNotes { get; set; }

		/// <summary>How a user restores the sample to its initial state.</summary>
		public string? ResetBehavior { get; set; }

		/// <summary>Distinct states, configurations, or interaction paths demonstrated by the sample.</summary>
		public string[]? Variants { get; set; }

		/// <summary>Known platform, renderer, or behavior limitations. Empty when none are known.</summary>
		public string[]? KnownLimitations { get; set; }

		/// <summary>Optional issue tracking a known limitation or follow-up.</summary>
		public string? IssueLink { get; set; }

		/// <summary>Optional API reference when <see cref="DocumentationLink"/> is broader guidance.</summary>
		public string? ApiLink { get; set; }

		/// <summary>
		/// URL-friendly slugs of related samples for cross-linking in the catalog.
		/// Each entry must exactly match (ordinal, lowercase) the final slug of another
		/// sample — either the slug derived from its title or its explicit <see cref="Slug"/>.
		/// </summary>
		public string[]? RelatedSamples { get; set; }
	}

	/// <summary>Reusable contract text for facts that are identical across multiple samples.</summary>
	public static class SampleContractDefaults
	{
		public const string NoExternalRequirements = "No permissions, network access, external services, or additional setup are required.";
		public const string ReloadToReset = "Navigate away and reopen the sample to restore its initial state.";
	}
}
