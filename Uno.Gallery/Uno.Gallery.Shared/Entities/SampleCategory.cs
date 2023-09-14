using Uno.Gallery.Entities;

namespace Uno.Gallery
{
	public enum SampleCategory
	{
		/// <summary>
		/// Reserved for samples placed on top with no category, eg: Home, Overview
		/// </summary>
		None,

		[SampleCategoryInfo("\uE790", "Theming")]
		Theming,

		[SampleCategoryInfo("\uE81E", "UI components")]
		UIComponents,

		[SampleCategoryInfo("\uE8AE", "UI features")]
		UIFeatures,

		[SampleCategoryInfo("\uE950", "Non-UI features")]
		NonUIFeatures,

		[SampleCategoryInfo("\uF0B4", "Toolkit")]
		Toolkit,

		[SampleCategoryInfo("\uE821", "Community Toolkit")]
		CommunityToolkit,
	}
}
