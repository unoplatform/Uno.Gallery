using System;
using System.Collections.Generic;

namespace Uno.Gallery.Helpers
{
	/// <summary>
	/// Pure, stateless scoring logic for sample search.
	/// No WinUI, Sample, or runtime-type dependencies — can be compiled into test
	/// assemblies via a shared-source <c>&lt;Compile Include&gt;</c> link.
	/// </summary>
	internal static class SampleSearchScorer
	{
		// ── Weights ────────────────────────────────────────────────────────────
		// Hierarchy (descending): TitleExact > TitlePrefix > TitleContain
		//   > SlugExact = TagExact > SlugContain = TagContain
		//   > DescContain > StatusExact > StatusContain > CatContain = SrcContain
		internal const int W_TitleExact    = 1000;
		internal const int W_TitlePrefix   = 500;
		internal const int W_TitleContain  = 200;
		internal const int W_SlugExact     = 150;
		internal const int W_TagExact      = 150;
		internal const int W_SlugContain   = 80;
		internal const int W_TagContain    = 80;
		internal const int W_DescContain   = 50;
		internal const int W_StatusExact   = 45;
		internal const int W_StatusContain = 35;
		internal const int W_CatContain    = 30;
		internal const int W_SrcContain    = 30;

		/// <summary>
		/// Splits a raw query into distinct lowercase terms.
		/// Returns <see cref="Array.Empty{T}"/> on null/blank input.
		/// </summary>
		internal static string[] SplitTerms(string? query)
		{
			if (string.IsNullOrWhiteSpace(query))
				return Array.Empty<string>();

			var raw = query.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			var seen = new List<string>(raw.Length);
			foreach (var part in raw)
			{
				var lower = part.ToLowerInvariant();
				if (!seen.Contains(lower))
					seen.Add(lower);
			}
			return seen.Count > 0 ? seen.ToArray() : Array.Empty<string>();
		}

		/// <summary>
		/// Scores a sample against pre-computed lowercase terms (from <see cref="SplitTerms"/>).
		/// Returns <c>-1</c> if any term matches no field (AND miss); otherwise returns the
		/// accumulated score (always &gt; 0 when all terms match).
		/// </summary>
		internal static int Score(
			string[] terms,
			string title,
			string slug,
			IReadOnlyList<string> tags,
			string? description,
			string? categoryCaption,
			string? sourceDescription,
			string? statusLabel)
		{
			if (terms.Length == 0)
				return -1;

			int total = 0;
			for (int t = 0; t < terms.Length; t++)
			{
				int termScore = ScoreTerm(terms[t], title, slug, tags, description, categoryCaption, sourceDescription, statusLabel);
				if (termScore < 0)
					return -1; // AND logic: one unmatched term excludes the sample
				total += termScore;
			}
			return total;
		}

		/// <summary>
		/// Returns the score contribution of a single lowercase <paramref name="term"/>
		/// across all fields. Returns <c>-1</c> if no field matches.
		/// </summary>
		private static int ScoreTerm(
			string term,
			string title,
			string slug,
			IReadOnlyList<string> tags,
			string? description,
			string? categoryCaption,
			string? sourceDescription,
			string? statusLabel)
		{
			int score = 0;

			// ── Title (exclusive tiers) ────────────────────────────────────────
			if (title.Equals(term, StringComparison.OrdinalIgnoreCase))
				score += W_TitleExact;
			else if (title.StartsWith(term, StringComparison.OrdinalIgnoreCase))
				score += W_TitlePrefix;
			else if (title.Contains(term, StringComparison.OrdinalIgnoreCase))
				score += W_TitleContain;

			// ── Slug (exclusive tiers; slug is already lowercase, term is lowercase) ──
			if (slug.Equals(term, StringComparison.Ordinal))
				score += W_SlugExact;
			else if (slug.Contains(term, StringComparison.Ordinal))
				score += W_SlugContain;

			// ── Tags (best single match wins) ──────────────────────────────────
			int tagScore = 0;
			for (int i = 0; i < tags.Count; i++)
			{
				var tag = tags[i];
				if (tag.Equals(term, StringComparison.OrdinalIgnoreCase))
				{
					tagScore = W_TagExact;
					break; // exact is the maximum possible tag score
				}
				if (tagScore == 0 && tag.Contains(term, StringComparison.OrdinalIgnoreCase))
					tagScore = W_TagContain;
			}
			score += tagScore;

			// ── Description ────────────────────────────────────────────────────
			if (!string.IsNullOrEmpty(description) && description.Contains(term, StringComparison.OrdinalIgnoreCase))
				score += W_DescContain;

			// ── Status label (exclusive tiers) ─────────────────────────────────
			if (!string.IsNullOrEmpty(statusLabel))
			{
				if (statusLabel.Equals(term, StringComparison.OrdinalIgnoreCase))
					score += W_StatusExact;
				else if (statusLabel.Contains(term, StringComparison.OrdinalIgnoreCase))
					score += W_StatusContain;
			}

			// ── Category caption ───────────────────────────────────────────────
			if (!string.IsNullOrEmpty(categoryCaption) && categoryCaption.Contains(term, StringComparison.OrdinalIgnoreCase))
				score += W_CatContain;

			// ── Source description ─────────────────────────────────────────────
			if (!string.IsNullOrEmpty(sourceDescription) && sourceDescription.Contains(term, StringComparison.OrdinalIgnoreCase))
				score += W_SrcContain;

			return score > 0 ? score : -1;
		}
	}
}
