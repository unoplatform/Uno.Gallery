using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;
using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;

namespace Uno.Gallery.UITests
{
    public class Given_MainPage : TestBase
    {
        [Test]
        public void When_SmokeTest()
        {
            Query ContainedSelector = q => q.All().Marked("MaterialContainedButton");
            Query section = q => q.All().Marked("Section_Overview");

            App.WaitForElement(section);
            App.Tap(section);

            App.WaitForElement(ContainedSelector);
            App.Tap(ContainedSelector);

            TakeScreenshot("Opened");
        }
    }
}
