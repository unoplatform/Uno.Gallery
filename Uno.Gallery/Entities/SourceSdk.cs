using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Uno.Gallery
{
	public enum SourceSdk
	{
		[Description("Uno.WinUI")]
		WinUI,
		[Description("Uno.Material")]
		UnoMaterial,
		[Description("Windows Community Toolkit")]
		WCT,
		[Description("Uno.Toolkit")]
		UnoToolkit,
#if EXTENSIONS_PATTERNS
		[Description("Uno.Extensions 7.2.3")]
		UnoExtensions
#endif
	}
}
