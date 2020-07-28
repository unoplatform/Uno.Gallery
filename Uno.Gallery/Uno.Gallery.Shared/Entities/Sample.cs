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
			Source = attribute.Source;
			SortOrder = attribute.SortOrder;
			OverviewCtaText = attribute.OverviewCtaText;

			ViewType = viewType;
		}

		public SampleCategory Category { get; set; }

		public string Title { get; }

		public string Description { get; }

		public int? SortOrder { get; }

		public SourceSdk Source { get; }

		public string OverviewCtaText { get; set; }

		public Type ViewType { get; }
	}
}
