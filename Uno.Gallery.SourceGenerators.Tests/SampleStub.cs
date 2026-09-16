using System.Collections.Generic;

// Test-only stub that satisfies the Sample references compiled via Compile-link from
// SampleSearchHelper.cs.  Contains exactly the properties that RankAndFilter accesses;
// no WinUI or app-project dependency.
namespace Uno.Gallery
{
	internal sealed class Sample
	{
		public Sample(string title, string slug, IReadOnlyList<string>? tags = null,
			string? description = null, string? categoryCaption = null,
			string? sourceDescription = null, int? sortOrder = null,
			string? statusLabel = null)
		{
			Title            = title;
			Slug             = slug;
			Tags             = tags ?? System.Array.Empty<string>();
			Description      = description;
			CategoryCaption  = categoryCaption ?? string.Empty;
			SourceDescription = sourceDescription;
			SortOrder        = sortOrder;
			StatusLabel      = statusLabel ?? string.Empty;
			SearchAccessibleName = BuildSearchAccessibleName(title, CategoryCaption, StatusLabel);
		}

		public string Title             { get; }
		public string Slug              { get; }
		public IReadOnlyList<string> Tags { get; }
		public string? Description      { get; }
		public string CategoryCaption   { get; }
		public string? SourceDescription { get; }
		public int?    SortOrder        { get; }
		public string  StatusLabel      { get; }
		public string  SearchAccessibleName { get; }

		private static string BuildSearchAccessibleName(string title, string categoryCaption, string statusLabel)
		{
			if (categoryCaption.Length > 0 && statusLabel.Length > 0)
				return $"{title}, {categoryCaption}, {statusLabel}";
			if (categoryCaption.Length > 0)
				return $"{title}, {categoryCaption}";
			if (statusLabel.Length > 0)
				return $"{title}, {statusLabel}";
			return title;
		}
	}
}
