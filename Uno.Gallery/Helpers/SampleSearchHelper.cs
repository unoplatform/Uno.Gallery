using System;
using System.Collections.Generic;
using Uno.Gallery;

namespace Uno.Gallery.Helpers
{
	/// <summary>
	/// App-specific search helper that applies <see cref="SampleSearchScorer"/> over a
	/// <see cref="Sample"/> catalog and returns a ranked, filtered result array.
	/// </summary>
	internal static class SampleSearchHelper
	{
		/// <summary>
		/// Returns samples whose every query term matches at least one field, ordered by
		/// score (desc), <see cref="Sample.SortOrder"/> (asc, null last), then
		/// <see cref="Sample.Title"/> (OrdinalIgnoreCase asc).
		/// Returns <see cref="Array.Empty{T}"/> for an empty/blank query or no matches.
		/// </summary>
		internal static Sample[] RankAndFilter(IReadOnlyList<Sample> samples, string query)
		{
			var terms = SampleSearchScorer.SplitTerms(query);
			if (terms.Length == 0)
				return Array.Empty<Sample>();

			var scored = new List<(int Score, Sample Sample)>(samples.Count);
			for (int i = 0; i < samples.Count; i++)
			{
				var s = samples[i];
				int score = SampleSearchScorer.Score(
					terms,
					s.Title,
					s.Slug,
					s.Tags,
					s.Description,
					s.CategoryCaption,
						s.SourceDescription,
						s.StatusLabel);
				if (score > 0)
					scored.Add((score, s));
			}

			if (scored.Count == 0)
				return Array.Empty<Sample>();

			scored.Sort(static (a, b) =>
			{
				// 1. Score descending
				int cmp = b.Score.CompareTo(a.Score);
				if (cmp != 0) return cmp;

				// 2. SortOrder ascending (null → int.MaxValue)
				int aOrder = a.Sample.SortOrder ?? int.MaxValue;
				int bOrder = b.Sample.SortOrder ?? int.MaxValue;
				cmp = aOrder.CompareTo(bOrder);
				if (cmp != 0) return cmp;

				// 3. Title OrdinalIgnoreCase ascending
				return string.Compare(a.Sample.Title, b.Sample.Title, StringComparison.OrdinalIgnoreCase);
			});

			var result = new Sample[scored.Count];
			for (int i = 0; i < scored.Count; i++)
				result[i] = scored[i].Sample;
			return result;
		}
	}
}
