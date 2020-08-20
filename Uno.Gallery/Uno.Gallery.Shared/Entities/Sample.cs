using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno.Gallery
{
	public class Sample
	{
		public Sample(SamplePageAttribute attribute, Type viewType)
		{
			Category = attribute.Category;
			Title = attribute.Title;
			Description = attribute.Description;
			DocumentationLink = attribute.DocumentationLink;
			Source = attribute.Source;
			SortOrder = attribute.SortOrder;

			ViewType = viewType;
		}

		public SampleCategory Category { get; set; }

		public string Title { get; }

		public string Description { get; }

		public string DocumentationLink { get; }

		public int? SortOrder { get; }

		public SourceSdk Source { get; }

		public Type ViewType { get; }
	}
}
