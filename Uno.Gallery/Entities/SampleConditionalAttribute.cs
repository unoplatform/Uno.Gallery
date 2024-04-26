using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.Gallery
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class SampleConditionalAttribute : Attribute
	{
		public SampleConditionalAttribute(SampleConditionals conditionals)
		{
			this.Conditionals = conditionals;
		}

		public SampleConditionals Conditionals { get; }
		
		public string Reason { get; set; }
	}
}
