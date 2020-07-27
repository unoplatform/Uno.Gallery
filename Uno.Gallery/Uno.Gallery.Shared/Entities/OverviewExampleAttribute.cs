using System;
using System.Collections.Generic;
using System.Text;
using static Uno.Gallery.Controls.SamplePageLayout;

namespace Uno.Gallery
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class OverviewExampleAttribute : Attribute
	{
		public OverviewExampleAttribute(SamplePageLayoutMode mode, string templateKey)
		{
			Mode = mode;
			TemplateKey = templateKey;
		}

		public SamplePageLayoutMode Mode { get; }

		public string TemplateKey { get; }
	}
}
