#pragma warning disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ShowMeTheXAML;
using Uno.Extensions;
using Uno.Gallery;
using Windows.UI.Xaml;

namespace ShowMeTheXAML
{
	public static class XamlDisplayExtensions
	{
		/* Header, Description: [optional] texts for display
		 * IgnorePath: [required] ignore everything except descendent(s) of this path; see: PrettyXamlFormatter.IgnorePath
		 *             ^ made [optional='XamlDisplay'] with default style setter
		 * PrettyXaml: [reserved] custom formatted xaml based on IgnorePath
		 */

		#region Property: Header

		public static DependencyProperty HeaderProperty { get; } = DependencyProperty.RegisterAttached(
			"Header",
			typeof(string),
			typeof(XamlDisplayExtensions),
			new PropertyMetadata(default));

		public static string GetHeader(XamlDisplay obj) => (string)obj.GetValue(HeaderProperty);
		public static void SetHeader(XamlDisplay obj, string value) => obj.SetValue(HeaderProperty, value);

		#endregion
		#region Property: Description

		public static DependencyProperty DescriptionProperty { get; } = DependencyProperty.RegisterAttached(
			"Description",
			typeof(string),
			typeof(XamlDisplayExtensions),
			new PropertyMetadata(default));

		public static string GetDescription(XamlDisplay obj) => (string)obj.GetValue(DescriptionProperty);
		public static void SetDescription(XamlDisplay obj, string value) => obj.SetValue(DescriptionProperty, value);

		public static DependencyProperty DocumentationLinkProperty { get; } = DependencyProperty.RegisterAttached(
			"DocumentationLink",
			typeof(string),
			typeof(XamlDisplayExtensions),
			new PropertyMetadata(default));

		public static string GetDocumentationLink(XamlDisplay obj) => (string)obj.GetValue(DocumentationLinkProperty);
		public static void SetDocumentationLink(XamlDisplay obj, string value) => obj.SetValue(DocumentationLinkProperty, value);

		#endregion
		#region Property: IgnorePath

		public static DependencyProperty IgnorePathProperty { get; } = DependencyProperty.RegisterAttached(
			"IgnorePath",
			typeof(string),
			typeof(XamlDisplayExtensions),
			new PropertyMetadata(default, (d, e) => d.Maybe<XamlDisplay>(control => OnIgnorePathChanged(control, e))));

		public static string GetIgnorePath(XamlDisplay obj) => (string)obj.GetValue(IgnorePathProperty);
		public static void SetIgnorePath(XamlDisplay obj, string value) => obj.SetValue(IgnorePathProperty, value);

		#endregion
		#region Property: PrettyXaml

		public static DependencyProperty PrettyXamlProperty { get; } = DependencyProperty.RegisterAttached(
			"PrettyXaml",
			typeof(string),
			typeof(XamlDisplayExtensions),
			new PropertyMetadata(default));

		public static string GetPrettyXaml(XamlDisplay obj) => (string)obj.GetValue(PrettyXamlProperty);
		public static void SetPrettyXaml(XamlDisplay obj, string value) => obj.SetValue(PrettyXamlProperty, value);

		#endregion

		private static void OnIgnorePathChanged(XamlDisplay sender, DependencyPropertyChangedEventArgs e)
		{
			sender.RegisterPropertyChangedCallback(XamlDisplay.XamlProperty, OnXamlChanged);
			if (sender.Xaml != null)
			{
				OnXamlChanged(sender, XamlDisplay.XamlProperty);
			}
		}

		private static void OnXamlChanged(DependencyObject sender, DependencyProperty dp)
		{
			if (sender is XamlDisplay target)
			{
				var ignorePath = GetIgnorePath(target);
				var formatter = new PrettyXamlFormatter() { IgnorePath = ignorePath };
				var xaml = formatter.FormatXaml(target.Xaml);

				SetPrettyXaml(target, xaml);
			}
		}

		#region Implementation

		private class XmlWriterProxy : XmlWriter
		{
			protected readonly StringBuilder _buffer;
			protected readonly TextWriter _textWriter;
			private readonly XmlWriter _writer;
			private bool? _noopInnerCallOverride;

			public XmlWriterProxy(StringBuilder buffer, XmlWriterSettings settings)
			{
				_buffer = buffer;
				_textWriter = new StringWriter(buffer);
				_writer = XmlWriter.Create(_textWriter, settings);
			}
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				GetProxy()?.Flush();
			}

			protected virtual bool NoopInnerCall => false;
			protected IDisposable OverrideNoopInnerCall(bool value)
			{
				return new AnonymousDisposable(() => _noopInnerCallOverride = value, () => _noopInnerCallOverride = null);
			}
			private XmlWriter GetProxy() => _noopInnerCallOverride ?? NoopInnerCall ? default : _writer;

			#region XmlWriter implementation
			public override XmlWriterSettings Settings => _writer.Settings;
			public override WriteState WriteState => _writer.WriteState;

			public override string LookupPrefix(string ns) => _writer.LookupPrefix(ns);

			public override void Flush() => GetProxy()?.Flush();
			public override void WriteBase64(byte[] buffer, int index, int count) => GetProxy()?.WriteBase64(buffer, index, count);
			public override void WriteCData(string text) => GetProxy()?.WriteCData(text);
			public override void WriteCharEntity(char ch) => GetProxy()?.WriteCharEntity(ch);
			public override void WriteChars(char[] buffer, int index, int count) => GetProxy()?.WriteChars(buffer, index, count);
			public override void WriteComment(string text) => GetProxy()?.WriteComment(text);
			public override void WriteDocType(string name, string pubid, string sysid, string subset) => GetProxy()?.WriteDocType(name, pubid, sysid, subset);
			public override void WriteEndAttribute() => GetProxy()?.WriteEndAttribute();
			public override void WriteEndDocument() => GetProxy()?.WriteEndDocument();
			public override void WriteEndElement() => GetProxy()?.WriteEndElement();
			public override void WriteEntityRef(string name) => GetProxy()?.WriteEntityRef(name);
			public override void WriteFullEndElement() => GetProxy()?.WriteFullEndElement();
			public override void WriteProcessingInstruction(string name, string text) => GetProxy()?.WriteProcessingInstruction(name, text);
			public override void WriteRaw(char[] buffer, int index, int count) => GetProxy()?.WriteRaw(buffer, index, count);
			public override void WriteRaw(string data) => GetProxy()?.WriteRaw(data);
			public override void WriteStartAttribute(string prefix, string localName, string ns) => GetProxy()?.WriteStartAttribute(prefix, localName, ns);
			public override void WriteStartDocument() => GetProxy()?.WriteStartDocument();
			public override void WriteStartDocument(bool standalone) => GetProxy()?.WriteStartDocument(standalone);
			public override void WriteStartElement(string prefix, string localName, string ns) => GetProxy()?.WriteStartElement(prefix, localName, ns);
			public override void WriteString(string text) => GetProxy()?.WriteString(text);
			public override void WriteSurrogateCharEntity(char lowChar, char highChar) => GetProxy()?.WriteSurrogateCharEntity(lowChar, highChar);
			public override void WriteWhitespace(string ws) => GetProxy()?.WriteWhitespace(ws);

			#endregion
		}

		private class NiceXmlWriter : XmlWriterProxy
		{
			private Stack<(string Prefix, string LocalName, string NS)> _elements = new Stack<(string Prefix, string LocalName, string NS)>();
			private bool _wroteFirstElementAttribute = false;
			private int _currentIndentLevel = -1;
			private int _currentElementAttributeIndentLength = 0;

			// align attributes to the first one, remove XamlDisplay element, but leave nested elements intact
			public NiceXmlWriter(StringBuilder buffer) : base(buffer, GetSettings())
			{
			}
			private static XmlWriterSettings GetSettings()
			{
				return new XmlWriterSettings
				{
					Indent = true,
					IndentChars = "   ",
					NamespaceHandling = NamespaceHandling.OmitDuplicates,
					NewLineOnAttributes = false,
					OmitXmlDeclaration = true,
					NewLineHandling = NewLineHandling.Replace,
					NewLineChars = Environment.NewLine,
					ConformanceLevel = ConformanceLevel.Fragment,
				};
			}

			protected virtual IReadOnlyCollection<(string Prefix, string LocalName, string NS)> ElementStack => _elements;
			protected override bool NoopInnerCall => _elements.Any() && _elements.Peek().LocalName == nameof(XamlDisplay);

			public override void WriteStartAttribute(string prefix, string localName, string ns)
			{
				if (NoopInnerCall) return;
				if (_wroteFirstElementAttribute)
				{
					// flush xml buffer, so we can write with _textWrite at appropriate location
					Flush();

					// align to first attribute
					_textWriter.Write(Environment.NewLine);
					if (Settings.Indent && !string.IsNullOrEmpty(Settings.IndentChars))
					{
						for (int i = 0; Settings.Indent && i < _currentIndentLevel; i++)
						{
							_textWriter.Write(Settings.IndentChars);
						}
					}
					_textWriter.Write(new string(' ', _currentElementAttributeIndentLength));
				}

				base.WriteStartAttribute(prefix, localName, ns);
			}
			public override void WriteEndAttribute()
			{
				if (NoopInnerCall) return;

				base.WriteEndAttribute();

				_wroteFirstElementAttribute = true;
			}
			public override void WriteStartElement(string prefix, string localName, string ns)
			{
				_elements.Push((prefix, localName, ns));
				if (NoopInnerCall) return;

				base.WriteStartElement(prefix, localName, ns);

				_currentIndentLevel++;
				_wroteFirstElementAttribute = false;
				_currentElementAttributeIndentLength =
					// length: (${prefix}:)?localName\s
					(string.IsNullOrEmpty(prefix) ? 0 : prefix.Length + 1) + localName.Length + 1;
			}
			public override void WriteEndElement()
			{
				base.WriteEndElement();
				_elements.Pop();

				_currentIndentLevel--;
			}
			public override void WriteFullEndElement()
			{
				base.WriteFullEndElement();
				_elements.Pop();

				_currentIndentLevel--;
			}
			public override void WriteComment(string text)
			{
				using (OverrideNoopInnerCall(false))
				{
					base.WriteComment(text);
				}
			}
		}

		private class CustomXamlFormatterBase
		{
			public virtual string FormatXaml(string xaml)
			{
				if (string.IsNullOrWhiteSpace(xaml)) return string.Empty;
				try
				{
					xaml = PreprocessXaml(xaml);
					xaml = RewriteXaml(xaml);
					xaml = PostprocessXaml(xaml);

					return xaml;
				}
				catch (Exception e)
				{
					return HandleException(e, xaml);
				}
			}

			protected virtual string PreprocessXaml(string xaml) => xaml;
			protected virtual string RewriteXaml(string xaml)
			{
				var buffer = new StringBuilder();
				using (var rewriter = CreateRewriter(buffer))
				{
					var element = XElement.Parse(xaml);
					element.WriteTo(rewriter);
					rewriter.Flush();

					return buffer.ToString();
				}
			}
			protected virtual string PostprocessXaml(string xaml) => xaml;
			protected virtual string HandleException(Exception e, string xaml) => xaml;

			protected virtual XmlWriter CreateRewriter(StringBuilder buffer) => XmlWriter.Create(buffer);
		}

		private class PrettyXamlFormatter : CustomXamlFormatterBase
		{
			public const string PathSeparator = @"\";

			/// <summary>Ignore everything except descendent(s) of this path, eg: XamlDisplay>StackPanel </summary>
			/// <remarks>This path is the local-name of elements joined by <see cref="PathSeparator"/>.</remarks>
			public string IgnorePath { get; set; } = "XamlDisplay";

			protected override XmlWriter CreateRewriter(StringBuilder buffer)
			{
				return new PrettyXmlWriter(
					buffer,
					stack => // return true to ignore current element
						!string.IsNullOrEmpty(IgnorePath) && stack.Any() &&
						(
							stack.Count() <= IgnorePath.Split(new[] { PathSeparator }, StringSplitOptions.None).Length ||
							!string.Join(PathSeparator, stack.Reverse().Select(x => x.LocalName)).StartsWith(IgnorePath)
						)
				);
			}
			protected override string PostprocessXaml(string xaml)
			{
				xaml = RemoveXmlns(xaml);

				return xaml;
			}

			private static string RemoveXmlns(string xaml)
			{
				var namespaces = new[]
				{
					"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
					"http://schemas.microsoft.com/winfx/2006/xaml",
				};
				return Regex.Replace(xaml, @"\s+xmlns(:(?<prefix>\w+))?=""(?<uri>[^""]+)""", m =>
				{
					return namespaces?.Contains(m.Groups["uri"].Value) == true
						? null
						: m.Value;
				});
			}

			private class PrettyXmlWriter : NiceXmlWriter
			{
				Func<IReadOnlyCollection<(string Prefix, string LocalName, string NS)>, bool> _noopInnerCallImpl;

				public PrettyXmlWriter(
					StringBuilder buffer,
					Func<IReadOnlyCollection<(string Prefix, string LocalName, string NS)>, bool> noopInnerCallImpl)
					: base(buffer)
				{
					_noopInnerCallImpl = noopInnerCallImpl;
				}

				protected override bool NoopInnerCall => _noopInnerCallImpl(ElementStack);
			}
		}

		#endregion
	}
}
