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
		public SamplePageAttribute(string title, string description, SourceSdk source)
		{
			Category = SampleCategory.Components;
			Title = title;
			Description = description;
			Source = source;
		}

		public SamplePageAttribute(SampleCategory category, string title)
		{
			Category = category;
			Title = title;
		}

		/// <summary>
		/// Sample category with null reserved for Home/Overview.
		/// </summary>
		public SampleCategory Category { get; }
		
		public string Title { get; }

		public string Description { get; set; }

		public SourceSdk Source { get; set; }

		/// <summary>
		/// Sort order with the same <see cref="Category"/>.
		/// </summary>
		public int SortOrder { get; set; } = int.MaxValue;
	}
}
