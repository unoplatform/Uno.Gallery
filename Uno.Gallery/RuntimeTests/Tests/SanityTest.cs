using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.UI.RuntimeTests;

namespace Uno.Gallery.RuntimeTests.Tests;

[TestClass]
public class SanityTests
{
	[TestMethod]
	public void Is_Sane()
	{
	}

	[TestMethod]
	public async Task Is_Still_Sane()
	{
		await Task.Delay(500);
	}

	[TestMethod]
	[RunsOnUIThread]
	public async Task When_Test_ContentHelper()
	{
		var SUT = new TextBlock() { Text = "New paths lead to new nightmares." };
		UnitTestsUIContentHelper.Content = SUT;

		await UnitTestsUIContentHelper.WaitForIdle();
		await UnitTestsUIContentHelper.WaitForLoaded(SUT);
	}

#if false // used for testing the engine
	[TestMethod]
	public async Task No_Longer_Sane() // expected to fail
	{
		await Task.Delay(500);

		throw new Exception("Great works require a touch of insanity.");
	}
#endif

	[TestMethod, Ignore]
	public void Is_An_Illusion() // expected to be ignored
	{
	}
}
