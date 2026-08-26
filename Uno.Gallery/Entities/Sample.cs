using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Extensions;
using Uno.Logging;
using Uno.Gallery.Entities;
using Uno.Gallery.Helpers;
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

		private readonly Func<Page> _pageFactory;
		private readonly Func<object?>? _dataFactory;

		// Access is UI-thread-only (driven by the XAML binding engine), so no lock or volatile is needed.
		// Starts as _noData when any configured data source/factory is present; null otherwise (fast path).
		private object? _data;

		/// <summary>
		/// Legacy public constructor preserved for reflection-created callers such as
		/// <see cref="OverviewSampleView"/> and rollback paths.
		/// Uses <see cref="Activator.CreateInstance"/> internally; not AOT-safe.
		/// </summary>
		public Sample(SamplePageAttribute attribute, [DynamicallyAccessedMembers(ViewRequirements)] Type viewType)
			: this(attribute, viewType, CreateLegacyPageFactory(viewType), null)
		{
		}

		/// <summary>
		/// Factory constructor used by the source-generated catalog.
		/// <paramref name="pageFactory"/> and <paramref name="dataFactory"/> are static,
		/// AOT-safe lambdas emitted at compile time by <c>SamplesGenerator</c>.
		/// </summary>
		internal Sample(
			SamplePageAttribute attribute,
			[DynamicallyAccessedMembers(ViewRequirements)] Type viewType,
			Func<Page> pageFactory,
			Func<object?>? dataFactory)
		{
			ArgumentNullException.ThrowIfNull(attribute);
			ArgumentNullException.ThrowIfNull(pageFactory);

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
			_pageFactory = pageFactory;
			_dataFactory = dataFactory;
			// Start with sentinel when any data source is present; null otherwise (fast path).
			_data = (_dataType is not null || _dataFactory is not null) ? _noData : null;
			Source = attribute.Source;
			SortOrder = attribute.SortOrder;

			Slug = attribute.Slug ?? SlugHelper.DeriveSlug(attribute.Title);
			ShareUri = "https://gallery.platform.uno/#" + Uri.EscapeDataString(Slug);
			Status = attribute.Status;

			// Computed-once display/search helpers — no per-call reflection.
			CategoryCaption = attribute.Category.GetAttribute<SampleCategoryInfoAttribute>()?.Caption ?? string.Empty;
			IsCategorized = attribute.Category != SampleCategory.None;
			SourceDescription = attribute.Source.GetDescription() ?? string.Empty;
			IsNotStable = attribute.Status != SampleStatus.Stable;
			StatusLabel = attribute.Status != SampleStatus.Stable ? attribute.Status.ToString() : string.Empty;
			SearchAccessibleName = BuildSearchAccessibleName(Title, CategoryCaption, StatusLabel);

			Tags = attribute.Tags is { Length: > 0 } t
				? Array.AsReadOnly(t)
				: (IReadOnlyList<string>)Array.Empty<string>();
			RelatedSamples = attribute.RelatedSamples is { Length: > 0 } rs
				? Array.AsReadOnly(rs)
				: (IReadOnlyList<string>)Array.Empty<string>();
			Owner = attribute.Owner;
			ReviewedOn = attribute.ReviewedOn;
		}

		// Wraps Activator for the legacy reflection path so the DynamicallyAccessedMembers
		// annotation flows from the calling constructor's viewType parameter into this method.
		// A null viewType (the Shell "no suggestions" sentinel) produces a factory that throws
		// on invocation — the sentinel is never navigated to, so CreatePage is never called on it.
		private static Func<Page> CreateLegacyPageFactory([DynamicallyAccessedMembers(ViewRequirements)] Type? viewType)
			=> viewType is null
				? static () => throw new InvalidOperationException("Cannot navigate to a null-viewType sentinel sample.")
				: () => (Page)Activator.CreateInstance(viewType)!;

		/// <summary>
		/// Builds the accessible name: "Title[, CategoryCaption][, StatusLabel]".
		/// Called once in the constructor after all three parts are set.
		/// </summary>
		private static string BuildSearchAccessibleName(string title, string categoryCaption, string statusLabel)
		{
			if (categoryCaption.Length > 0 && statusLabel.Length > 0)
				return $"{title}, {categoryCaption}, {statusLabel}";
			if (categoryCaption.Length > 0)
				return $"{title}, {categoryCaption}";
			if (statusLabel.Length > 0)
				return $"{title}, {statusLabel}";
			return title;
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

		private object? TryInvokeDataFactory()
		{
			try
			{
				return _dataFactory!();
			}
			catch (Exception e)
			{
				this.Log().Error($"Failed to initialize data for `{ViewType?.Name ?? "(null)"}`. _dataType: {_dataType}. Exception: {e}");
				return null;
			}
		}

		/// <summary>Creates a fresh page instance for navigation.</summary>
		internal Page CreatePage() => _pageFactory();

		public SampleCategory Category { get; set; }

		public string Title { get; }

		public string Description { get; }

		public string Glyph { get; }

		public Uri DocumentationLink { get; }

		/// <summary>
		/// Lazily-constructed instance of <see cref="SamplePageAttribute.DataType"/>.
		/// Created on first access and cached afterwards (including a null result on failure).
		/// Repeated accesses return the same cached reference; no-data samples always return null cheaply.
		/// Generated-catalog samples use the injected <c>dataFactory</c>; legacy-constructor samples
		/// fall back to <see cref="Activator.CreateInstance"/>.
		/// </summary>
		public object? Data
		{
			get
			{
				if (ReferenceEquals(_data, _noData))
				{
					_data = _dataFactory is not null ? TryInvokeDataFactory() : CreateData(_dataType);
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
		/// Source file path relative to the repository root (e.g. <c>Views/SamplePages/ButtonSamplePage.xaml.cs</c>).
		/// Populated by the source generator for generator-created (attribute-decorated) samples.
		/// Reflection-based and manually registered samples may be null until catalog lookup
		/// migration is complete; do not assume parity with attribute-registered samples.
		/// Kept internal so the WinUI XAML type-info generator does not emit a public setter.
		/// Setting this property also computes <see cref="SourceLink"/>.
		/// </summary>
		public string? SourcePath
		{
			get => _sourcePath;
			internal set
			{
				_sourcePath = value;
				SourceLink = value is not null ? BuildSourceLink(value) : null;
			}
		}

		private string? _sourcePath;

		/// <summary>
		/// Canonical deep-link URL for sharing this sample.
		/// Format: <c>https://gallery.platform.uno/#&lt;slug&gt;</c>.
		/// Computed once in the constructor from <see cref="Slug"/>.
		/// Never null.
		/// </summary>
		public string ShareUri { get; }

		/// <summary>
		/// Direct link to this sample's source file on GitHub.
		/// <c>https://github.com/unoplatform/Uno.Gallery/blob/{revision}/Uno.Gallery/{SourcePath}</c>
		/// where <c>revision</c> is <see cref="BuildInfo.CommitSha"/> when available,
		/// or <c>"master"</c> for local developer builds without a commit SHA in the
		/// <c>AssemblyInformationalVersion</c> (e.g. builds from source without NBGV tagging).
		/// Null when <see cref="SourcePath"/> is null (manually-registered or sentinel samples).
		/// </summary>
		public Uri? SourceLink { get; private set; }

		/// <summary>
		/// Builds the GitHub blob URL for a repo-relative <paramref name="sourcePath"/>.
		/// Uses <see cref="BuildInfo.CommitSha"/> as the revision; falls back to <c>"master"</c>
		/// for local builds where no commit SHA is embedded in the assembly version metadata.
		/// Each path segment is percent-encoded individually; forward-slash separators are preserved.
		/// </summary>
		private static Uri BuildSourceLink(string sourcePath)
		{
			var revision = string.IsNullOrEmpty(BuildInfo.CommitSha) ? "master" : BuildInfo.CommitSha;
			var safeRevision = Uri.EscapeDataString(revision);
			// Normalize Windows backslashes before splitting so local dev builds on Windows
			// produce the same forward-slash GitHub URL as CI builds on Linux/macOS.
			var safePath = string.Join("/", sourcePath.Replace('\\', '/').Split('/').Select(Uri.EscapeDataString));
			return new Uri("https://github.com/unoplatform/Uno.Gallery/blob/" + safeRevision + "/Uno.Gallery/" + safePath);
		}

		/// <summary>Production-readiness indicator for this sample.</summary>
		public SampleStatus Status { get; }

		/// <summary>
		/// Display caption for <see cref="Category"/> from <see cref="SampleCategoryInfoAttribute"/>.
		/// Empty string for <see cref="SampleCategory.None"/> (which has no attribute).
		/// Computed once in the constructor; no per-keystroke reflection.
		/// </summary>
		public string CategoryCaption { get; }

		/// <summary>Whether this sample has a non-<see cref="SampleCategory.None"/> category.</summary>
		public bool IsCategorized { get; }

		/// <summary>
		/// Display description for <see cref="Source"/> from <see cref="System.ComponentModel.DescriptionAttribute"/>.
		/// Computed once in the constructor; no per-keystroke reflection.
		/// </summary>
		public string SourceDescription { get; }

		/// <summary>Whether <see cref="Status"/> is not <see cref="SampleStatus.Stable"/>.</summary>
		public bool IsNotStable { get; }

		/// <summary>
		/// Short status label for non-Stable statuses (e.g. "Preview", "Experimental").
		/// Empty string when <see cref="Status"/> is <see cref="SampleStatus.Stable"/>.
		/// </summary>
		public string StatusLabel { get; }

		/// <summary>
		/// Screen-reader accessible name for the search suggestion.
		/// Format: "Title[, CategoryCaption][, StatusLabel]".
		/// Sentinel/Overview samples (no category, Stable) return Title only.
		/// Computed once in the constructor; never null.
		/// </summary>
		public string SearchAccessibleName { get; }

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
