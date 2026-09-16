using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Uno.Extensions;

namespace Uno.Gallery.ExtensionsPatterns.Core;

internal sealed class PatternSettings : ISettings
{
	private static readonly object Gate = new();
	private static readonly Dictionary<string, string?> Values = new();

	public IReadOnlyCollection<string> Keys
	{
		get
		{
			lock (Gate) return Values.Keys.ToArray();
		}
	}

	public string? Get(string key)
	{
		lock (Gate) return Values.TryGetValue(key, out var value) ? value : null;
	}

	public void Set(string key, string? value)
	{
		lock (Gate) Values[key] = value;
	}

	public void Remove(string key)
	{
		lock (Gate) Values.Remove(key);
	}

	public void Clear()
	{
		lock (Gate) Values.Clear();
	}
}

[JsonSerializable(typeof(string))]
internal partial class PatternJsonContext : JsonSerializerContext
{
}
