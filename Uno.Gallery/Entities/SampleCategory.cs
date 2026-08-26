using Uno.Gallery.Entities;

namespace Uno.Gallery
{
	public enum SampleCategory
	{
		/// <summary>
		/// Reserved for samples placed on top with no category, eg: Home, Overview
		/// </summary>
		None = 0,

		[SampleCategoryInfo("\uE790", "CategoryTheming", "Theming")]
		Theming = 1,

		[SampleCategoryInfo("\uE81E", "CategoryUIComponents", "UI components")]
		UIComponents = 2,

		[SampleCategoryInfo("\uE8AE", "CategoryUIFeatures", "UI features")]
		UIFeatures = 3,

		[SampleCategoryInfo("\uE950", "CategoryNonUIFeatures", "Non-UI features")]
		NonUIFeatures = 4,

		[SampleCategoryInfo("\uF0B4", "CategoryToolkit", "Toolkit")]
		Toolkit = 5,

		[SampleCategoryInfo("\uE821", "CategoryCommunityToolkit", "Community Toolkit")]
		CommunityToolkit = 6,

		// Hidden for non-canary builds
		[SampleCategoryInfo("\uE115", "CategoryCanary", "Canary")]
		Canary = 7,

		[SampleCategoryInfo("\uE776", "CategoryAccessibility", "Accessibility")]
		Accessibility = 8,
	}
}
