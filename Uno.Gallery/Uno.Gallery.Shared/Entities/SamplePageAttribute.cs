using System;
using System.Collections.Generic;
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

		public Type DataType { get; set; }

		public SourceSdk Source { get; }

		/// <summary>
		/// Sort order with the same <see cref="Category"/>.
		/// </summary>
		public int SortOrder { get; set; } = int.MaxValue;
	}
}
