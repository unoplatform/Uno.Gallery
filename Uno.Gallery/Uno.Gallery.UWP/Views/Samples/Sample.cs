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
			Title = attribute.Title;
			Description = attribute.Description;
			ViewModelType = attribute.ViewModelType;
			ViewType = viewType;
		}

		public string Title { get; }

		public string Description { get; }

		public Type ViewType { get; }

		public Type ViewModelType { get; }
	}
}
