using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Extensions;
using Uno.Logging;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery
{
	[Bindable]
	public class Sample
	{
		internal const DynamicallyAccessedMemberTypes ViewRequirements =
			  DynamicallyAccessedMemberTypes.PublicConstructors
			| DynamicallyAccessedMemberTypes.PublicProperties;

		public Sample(SamplePageAttribute attribute, [DynamicallyAccessedMembers(ViewRequirements)] Type viewType)
		{
			Category = attribute.Category;
			Title = attribute.Title;
			Description = attribute.Description;
			Glyph = attribute.Glyph;
			if (attribute.DocumentationLink != null)
			{
				DocumentationLink = new Uri(attribute.DocumentationLink);
			}

			ViewType = viewType;
			Data = CreateData(attribute.DataType);
			Source = attribute.Source;
			SortOrder = attribute.SortOrder;
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
				this.Log().Error($"Failed to initialize data for `{ViewType.Name}`. dataType: {dataType}. Exception: {e}");
				return null;
			}
		}

		public SampleCategory Category { get; set; }

		public string Title { get; }

		public string Description { get; }

		public string Glyph { get; }

		public Uri DocumentationLink { get; }

		public object Data { get; }

		public int? SortOrder { get; }

		public SourceSdk Source { get; }

		public Type ViewType { get; }
	}
}
