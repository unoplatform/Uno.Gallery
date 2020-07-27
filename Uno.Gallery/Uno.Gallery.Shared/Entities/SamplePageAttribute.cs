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
		public SamplePageAttribute(SampleCategory category, string title, SourceSdk source = SourceSdk.WinUI)
		{
			Category = category;
			Title = title;
			Source = source;
		}

		/// <summary>
		/// Sample category with null reserved for Home/Overview.
		/// </summary>
		public SampleCategory Category { get; }
		
		public string Title { get; }

		public string Description { get; set; }

		public SourceSdk Source { get; }

		public string OverviewCtaText { get; set; }

		/// <summary>
		/// Sort order with the same <see cref="Category"/>.
		/// </summary>
		public int SortOrder { get; set; } = int.MaxValue;
	}
}
