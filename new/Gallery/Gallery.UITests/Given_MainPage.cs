namespace Gallery.UITests;

public class Given_MainPage : TestBase
{
    [Test]
    public async Task When_SmokeTest()
    {
        // NOTICE
        // To run UITests, Run the WASM target without debugger. Note
        // the port that is being used and update the Constants.cs file
        // in the UITests project with the correct port number.

        // Add delay to allow for the splash screen to disappear
        await Task.Delay(5000);

        // Query for the MainPage Text Block
        Query textBlock = q => q.All().Marked("HelloTextBlock");
        App.WaitForElement(textBlock);

        // Take a screenshot and add it to the test results
        TakeScreenshot("After launch");
    }
}
