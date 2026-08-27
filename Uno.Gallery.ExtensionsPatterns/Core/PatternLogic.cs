using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Uno.Gallery.ExtensionsPatterns.Core;

public enum FeedScenario
{
	Loading,
	Data,
	Empty,
	Error,
	Refresh
}

public sealed record PatternFeedItem(string Name, string Detail);

public sealed record PatternFeedResult(string Heading, IReadOnlyList<PatternFeedItem> Items);

public sealed class FeedScenarioController
{
	private static readonly PatternFeedItem[] InitialItems =
	[
		new("Release checklist", "Verify offline state"),
		new("Accessibility pass", "Keyboard and live-region status"),
		new("Package audit", "Uno.Extensions 7.2.3")
	];

	public FeedScenario Scenario { get; private set; } = FeedScenario.Data;

	public int RefreshCount { get; private set; }

	public void Select(FeedScenario scenario) => Scenario = scenario;

	public void Reset()
	{
		Scenario = FeedScenario.Data;
		RefreshCount = 0;
	}

	public PatternFeedResult CreateResult()
	{
		if (Scenario == FeedScenario.Error)
		{
			throw new InvalidOperationException("Deterministic offline feed failure.");
		}

		if (Scenario == FeedScenario.Refresh)
		{
			RefreshCount++;
			return new PatternFeedResult(
				$"Refreshed locally ({RefreshCount})",
				InitialItems.Append(new("Refresh marker", $"Local refresh {RefreshCount}")).ToArray());
		}

		return Scenario == FeedScenario.Empty
			? new PatternFeedResult("No local work items", Array.Empty<PatternFeedItem>())
			: new PatternFeedResult("Local work items", InitialItems);
	}
}

public sealed record ExtensionsPatternOptions(string Environment, int PageSize, bool DiagnosticsEnabled)
{
	public static ExtensionsPatternOptions FromValues(Func<string, string?> value)
		=> new(
			value("AppPatterns:Environment") ?? "Offline",
			int.TryParse(value("AppPatterns:PageSize"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var pageSize)
				? pageSize
				: 3,
			bool.TryParse(value("AppPatterns:DiagnosticsEnabled"), out var diagnostics) && diagnostics);
}

public sealed class PatternLocalizationCatalog
{
	private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Resources =
		new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
		{
			["en-US"] = new Dictionary<string, string>
			{
				["Greeting"] = "Welcome to the offline app-pattern showcase.",
				["Saved"] = "Culture selected locally."
			},
			["fr-FR"] = new Dictionary<string, string>
			{
				["Greeting"] = "Bienvenue dans la vitrine hors connexion.",
				["Saved"] = "Culture sélectionnée localement."
			},
			["es-ES"] = new Dictionary<string, string>
			{
				["Greeting"] = "Bienvenido a la muestra sin conexión.",
				["Saved"] = "Cultura seleccionada localmente."
			}
		};

	public IReadOnlyList<CultureInfo> SupportedCultures { get; } =
		Resources.Keys.OrderBy(x => x, StringComparer.Ordinal).Select(x => new CultureInfo(x)).ToArray();

	public string Get(CultureInfo culture, string key)
		=> Resources.TryGetValue(culture.Name, out var values) && values.TryGetValue(key, out var value)
			? value
			: Resources["en-US"][key];
}

public sealed class RegistrationForm
{
	[Required(ErrorMessage = "Name is required.")]
	[MinLength(2, ErrorMessage = "Name must contain at least 2 characters.")]
	public string Name { get; set; } = "";

	[Required(ErrorMessage = "Email is required.")]
	[EmailAddress(ErrorMessage = "Enter a valid email address.")]
	public string Email { get; set; } = "";

	[Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
	public int Age { get; set; } = 18;
}
