using System.Text;

namespace Uno.Gallery
{
	/// <summary>
	/// Provides URL-slug derivation for sample titles.
	/// This file is a shared source: it is compiled into both the app assembly and
	/// <c>Uno.Gallery.SourceGenerators</c> via <c>&lt;Compile Include="..."/&gt;</c>.
	/// </summary>
	internal static class SlugHelper
	{
		/// <summary>
		/// Derives a URL-friendly slug from <paramref name="title"/>.
		/// </summary>
		/// <remarks>
		/// Algorithm:
		/// <list type="bullet">
		///   <item>Convert ASCII uppercase letters to lowercase (a-z).</item>
		///   <item>Collapse each contiguous run of non-ASCII-alphanumeric characters
		///         (spaces, slashes, punctuation, or any code-point above U+007F)
		///         to a single hyphen.</item>
		///   <item>Trim any leading or trailing hyphens.</item>
		/// </list>
		/// Contract: current gallery titles are all ASCII; non-ASCII input is accepted
		/// but characters above U+007F are treated as word separators, not transliterated.
		/// An empty title or a title composed entirely of separator characters returns
		/// the deterministic fallback value <c>"sample"</c>.
		/// </remarks>
		internal static string DeriveSlug(string title)
		{
			if (string.IsNullOrEmpty(title))
				return "sample";

			var sb = new StringBuilder(title.Length);
			bool pendingSep = false;

			foreach (char c in title)
			{
				if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
				{
					if (pendingSep && sb.Length > 0)
						sb.Append('-');
					pendingSep = false;
					sb.Append(c);
				}
				else if (c >= 'A' && c <= 'Z')
				{
					if (pendingSep && sb.Length > 0)
						sb.Append('-');
					pendingSep = false;
					sb.Append((char)(c + 32)); // ASCII uppercase to lowercase
				}
				else
				{
					// Space, slash, punctuation, or non-ASCII: start a pending separator run.
					pendingSep = true;
				}
			}

			return sb.Length > 0 ? sb.ToString() : "sample";
		}
	}
}
