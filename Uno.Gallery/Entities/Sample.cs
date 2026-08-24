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

		// Sentinel object: distinguishes "not yet created" from a cached null/failure result.
		private static readonly object _noData = new();

		[DynamicallyAccessedMembers(ViewRequirements)]
		private readonly Type? _dataType;

		// Access is UI-thread-only (driven by the XAML binding engine), so no lock or volatile is needed.
		// Starts as _noData for DataType-backed samples; null for no-data samples (fast path).
		private object? _data;

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
			_dataType = attribute.DataType;
			_data = _dataType is null ? null : _noData;
			Source = attribute.Source;
			SortOrder = attribute.SortOrder;
		}

		private object? CreateData([DynamicallyAccessedMembers(ViewRequirements)] Type? dataType)
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

		/// <summary>
		/// Lazily-constructed instance of <see cref="SamplePageAttribute.DataType"/>.
		/// Created on first access and cached afterwards (including a null result on failure).
		/// Repeated accesses return the same cached reference; no-data samples always return null cheaply.
		/// </summary>
		public object? Data
		{
			get
			{
				if (ReferenceEquals(_data, _noData))
				{
					_data = CreateData(_dataType);
				}
				return _data;
			}
		}

		public int? SortOrder { get; }

		public SourceSdk Source { get; }

		[DynamicallyAccessedMembers(ViewRequirements)]
		public Type ViewType { get; }
	}
}
