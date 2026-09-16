using System;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// Regression test: proves that <see cref="Uno.Gallery.Sample.Data"/> is lazily
	/// instantiated on first access and that the binding between the ViewModel and a
	/// named XAML element propagates the constructed value correctly.
	///
	/// Target: DataGridSamplePage (DataType = DataGridSamplePageViewModel).
	/// The ViewModel ctor sets ColumnHeaderHeight = 32; the DataGrid is named "dataGrid"
	/// and binds that property via {Binding Data.ColumnHeaderHeight, Mode=OneWay}.
	/// A ColumnHeaderHeight of 32 can only come from a successfully lazily-created ViewModel;
	/// 0 (the default) would indicate that Data was null or the lazy creation failed.
	/// </summary>
	public class Given_DataGrid : TestBase
	{
		[Test]
		public void When_DataGrid_LazyData_ColumnHeaderHeight_IsPopulatedFromViewModel()
		{
			// Navigate to the DataGrid sample. This is the first time Sample.Data is
			// accessed for this sample on the UI thread, triggering lazy construction
			// of DataGridSamplePageViewModel. No design suffix is required because the
			// page uses DesignAgnosticTemplate.
			NavigateToSample("DataGrid");

			// Confirm the page rendered and the named DataGrid is in the visual tree.
			App.WaitForElement(q => q.All().Marked("dataGrid"), timeout: TimeSpan.FromSeconds(30));

			var dataGrid = new QueryEx(x => x.All().Marked("dataGrid"));

			// ColumnHeaderHeight is set to 32 exclusively by DataGridSamplePageViewModel's
			// constructor. The default (unset) value for this DP is 0, so 32 proves that
			// the lazy Sample.Data access succeeded and the binding propagated.
			var columnHeaderHeight = dataGrid.GetDependencyPropertyValue<double>("ColumnHeaderHeight");

			TakeScreenshot("DataGrid_LazyData_Verified");

			Assert.AreEqual(
				32.0,
				columnHeaderHeight,
				$"Expected ColumnHeaderHeight=32 (from DataGridSamplePageViewModel ctor via lazy Sample.Data); " +
				$"actual={columnHeaderHeight} — this indicates that lazy Data creation failed or the binding did not propagate.");
		}
	}
}
