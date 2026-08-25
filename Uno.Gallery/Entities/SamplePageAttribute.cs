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
		/// Titles of related samples for cross-linking in the catalog.
		/// </summary>
		public string[]? RelatedSamples { get; set; }
	}
}
