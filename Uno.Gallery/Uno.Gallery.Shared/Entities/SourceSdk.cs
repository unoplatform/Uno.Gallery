using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Uno.Gallery
{
	public enum SourceSdk
	{
		[Description("Win.UI/Uno.UI")]
		WinUI,
		[Description("Uno.Material")]
		UnoMaterial,
		[Description("Windows Community Toolkit")]
		WCT,
		[Description("Uno.Toolkit")]
		UnoToolkit
	}
}
