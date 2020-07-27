using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class OverviewExampleAttribute : Attribute
	{
		public OverviewExampleAttribute(Design design, string templateKey)
		{
			Design = design;
			TemplateKey = templateKey;
		}

		public Design Design { get; }

		public string TemplateKey { get; }
	}
}
