using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Extensions;
using Uno.Logging;

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
			Data = CreateData(attribute.DataType);
			Source = attribute.Source;
			SortOrder = attribute.SortOrder;
			IconData = attribute.IconData;

			ViewType = viewType;
		}

		private object CreateData(Type dataType)
		{
			if (dataType == null) return null;

			try
			{
				return Activator.CreateInstance(dataType);
			}
			catch (Exception e)
			{
				this.Log().Error($"Failed to initialize data for `{ViewType.Name}`:", e);
				return null;
			}
		}

		public SampleCategory Category { get; set; }

		public string Title { get; }

		public string Description { get; }

		public string DocumentationLink { get; }

		public object Data { get; }

		public int? SortOrder { get; }

		public SourceSdk Source { get; }

		public string IconData { get; }

		public Type ViewType { get; }
	}
}
