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

			Slug = attribute.Slug ?? SlugHelper.DeriveSlug(attribute.Title);
			Status = attribute.Status;
			Tags = attribute.Tags is { Length: > 0 } t
				? Array.AsReadOnly(t)
				: (IReadOnlyList<string>)Array.Empty<string>();
			RelatedSamples = attribute.RelatedSamples is { Length: > 0 } rs
				? Array.AsReadOnly(rs)
				: (IReadOnlyList<string>)Array.Empty<string>();
			Owner = attribute.Owner;
			ReviewedOn = attribute.ReviewedOn;
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

		/// <summary>
		/// URL-friendly identifier derived from <see cref="Title"/> when
		/// <see cref="SamplePageAttribute.Slug"/> is not set.
		/// Always non-null; all-separator or empty titles fall back to <c>"sample"</c>.
		/// </summary>
		public string Slug { get; }

		/// <summary>
		/// Source file path relative to the repository root.
		/// Populated by the source generator for generator-created (attribute-decorated) samples.
		/// Reflection-based and manually registered samples may be null until catalog lookup
		/// migration is complete; do not assume parity with attribute-registered samples.
		/// Kept internal so the WinUI XAML type-info generator does not emit a public setter.
		/// </summary>
		public string? SourcePath { get; internal set; }

		/// <summary>Production-readiness indicator for this sample.</summary>
		public SampleStatus Status { get; }

		/// <summary>Categorization tags; never null.</summary>
		public IReadOnlyList<string> Tags { get; }

		/// <summary>Titles of related samples for cross-linking in the catalog; never null.</summary>
		public IReadOnlyList<string> RelatedSamples { get; }

		/// <summary>
		/// GitHub user or team slug of the owner responsible for this sample, or null if unset.
		/// </summary>
		public string? Owner { get; }

		/// <summary>
		/// Date of the last quality review in ISO 8601 format (<c>YYYY-MM-DD</c>), or null if not reviewed.
		/// </summary>
		public string? ReviewedOn { get; }
	}
}
