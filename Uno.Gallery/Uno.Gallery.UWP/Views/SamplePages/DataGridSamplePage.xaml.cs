using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Uno.Extensions;
using Uno.Gallery.Entities.Data;
using Uno.Gallery.ViewModels;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using static Uno.Gallery.Entities.Data.Plant;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.CommunityToolkit, "DataGrid",
		Description = "Provides a flexible way to display a collection of data in rows and columns.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/communitytoolkit/controls/datagrid",
		DataType = typeof(DataGridSamplePageViewModel))]
	public sealed partial class DataGridSamplePage : Page
	{
		public DataGridSamplePage()
		{
			this.InitializeComponent();
		}

		private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
		{
			var dg = sender as DataGrid;

			if (!(((Sample)DataContext).Data is DataGridSamplePageViewModel viewModel))
				return;

			switch (e.Column.Tag)
			{
				case "PlantName":
					viewModel.SortDataGrid(p => p.PlantName, e.Column);
					break;
				case "PlantsCount":
					viewModel.SortDataGrid(p => p.PlantsCount, e.Column);
					break;
				case "FruitOrVegetable":
					viewModel.SortDataGrid(p => p.FruitOrVegetable, e.Column);
					break;
				case "PlantDate":
					viewModel.SortDataGrid(p => p.PlantDate, e.Column);
					break;
				case "IsWatered":
					viewModel.SortDataGrid(p => p.IsWatered, e.Column);
					break;
			}

			foreach (var dgColumn in dg.Columns)
			{
				if (!dgColumn.Tag.Equals(e.Column.Tag))
				{
					dgColumn.SortDirection = null;
				}
			}
		}

		private void FilterFlyoutItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is DataGridSamplePageViewModel viewModel))
				return;

			switch ((sender as Control).Tag)
			{
				case "TenMore":
					viewModel.FilterDataGrid(p => p.PlantsCount > 10);
					break;
				case "TenLess":
					viewModel.FilterDataGrid(p => p.PlantsCount < 10);
					break;
				case "IsWatered":
					viewModel.FilterDataGrid(p => p.IsWatered);
					break;
				case "Fruit":
					viewModel.FilterDataGrid(p => p.FruitOrVegetable == FruitOrVegetableEnum.Fruit);
					break;
				case "Vegetable":
					viewModel.FilterDataGrid(p => p.FruitOrVegetable == FruitOrVegetableEnum.Vegetable);
					break;
				default:
					viewModel.FilterDataGrid(null);
					break;
			}
		}

		private void GroupByFlyoutItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is DataGridSamplePageViewModel viewModel))
				return;

			var tag = (sender as Control).Tag;

			switch (tag)
			{
				case "FruitOrVegetable":
					viewModel.RowGroupHeader = tag.ToString();

					viewModel.GroupByDataGrid(p => p.FruitOrVegetable);
					break;
				case "IsWatered":
					viewModel.RowGroupHeader = tag.ToString();

					viewModel.GroupByDataGrid(p => p.IsWatered);
					break;
				default:
					viewModel.GroupByDataGrid(null);
					break;
			}
		}

		private void DataGrid_LoadingRowGroup(object sender, DataGridRowGroupHeaderEventArgs e)
		{
			if (!(((Sample)DataContext).Data is DataGridSamplePageViewModel viewModel))
				return;

			var group = e.RowGroupHeader.CollectionViewGroup;
			var item = group.GroupItems[0] as Plant;

			(sender as DataGrid).RowGroupHeaderPropertyNameAlternative = viewModel.RowGroupHeader;
			if (viewModel.RowGroupHeader == "IsWatered")
			{
				e.RowGroupHeader.PropertyValue = (item?.IsWatered ?? false) ? "Yes" : "No";
			}
			else
			{
				e.RowGroupHeader.PropertyValue = (item?.FruitOrVegetable == FruitOrVegetableEnum.Fruit) ? "Fruit" : "Vegetable";
			}
		}

		private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
		{
			if (e.EditingElement is CalendarDatePicker calendar)
			{
				calendar.IsCalendarOpen = true;
			}
			else
			if (e.EditingElement is ComboBox comboBox)
			{
				comboBox.IsDropDownOpen = true;
			}
		}
	}

	public class DataGridSamplePageViewModel : ViewModelBase
	{
		public int ColumnHeaderHeight { get => GetProperty<int>(); set => SetProperty(value); }
		public int MaxColumnWidth { get => GetProperty<int>(); set => SetProperty(value); }
		public int MinColumnWidth { get => GetProperty<int>(); set => SetProperty(value); }
		public bool AreRowDetailsFrozen { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool AreRowGroupHeadersFrozen { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool CanUserSortColumns { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool CanUserReorderColumns { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool CanUserResizeColumns { get => GetProperty<bool>(); set => SetProperty(value); }
		public bool IsReadOnly { get => GetProperty<bool>(); set => SetProperty(value); }
		public SolidColorBrush AlternatingRowForeground { get => GetProperty<SolidColorBrush>(); set => SetProperty(value); }
		public SolidColorBrush AlternatingRowBackground { get => GetProperty<SolidColorBrush>(); set => SetProperty(value); }
		public DataGridGridLinesVisibility GridLinesVisibility { get => GetProperty<DataGridGridLinesVisibility>(); set => SetProperty(value); }
		public DataGridHeadersVisibility HeadersVisibility { get => GetProperty<DataGridHeadersVisibility>(); set => SetProperty(value); }
		public DataGridSelectionMode SelectionMode { get => GetProperty<DataGridSelectionMode>(); set => SetProperty(value); }
		public List<SolidColorBrush> AvailableColors { get => GetProperty<List<SolidColorBrush>>(); set => SetProperty(value); }
		public List<DataGridGridLinesVisibility> GridLinesVisibilityOptions { get => GetProperty<List<DataGridGridLinesVisibility>>(); set => SetProperty(value); }
		public List<DataGridHeadersVisibility> HeadersVisibilityOptions { get => GetProperty<List<DataGridHeadersVisibility>>(); set => SetProperty(value); }
		public List<DataGridSelectionMode> SelectionModeOptions { get => GetProperty<List<DataGridSelectionMode>>(); set => SetProperty(value); }

		public string RowGroupHeader { get => GetProperty<string>(); set => SetProperty(value); }

		public IEnumerable DataGridItems { get => GetProperty<IEnumerable>(); set => SetProperty(value); }
		public FruitOrVegetableEnum[] FruitOrVegetableEnumValues;

		// This is currently causes bug on WASM, removed until fixed
		//public DataGridRowDetailsVisibilityMode RowDetailsVisibilityMode { get => GetProperty<DataGridRowDetailsVisibilityMode>(); set => SetProperty(value); }
		//public List<DataGridRowDetailsVisibilityMode> RowDetailsVisibilityModeOptions { get => GetProperty<List<DataGridRowDetailsVisibilityMode>>(); set => SetProperty(value); }

		public DataGridSamplePageViewModel()
		{
			ColumnHeaderHeight = 32;
			MaxColumnWidth = 400;
			MinColumnWidth = 50;
			AreRowDetailsFrozen = false;
			AreRowGroupHeadersFrozen = true;
			CanUserSortColumns = true;
			CanUserReorderColumns = true;
			CanUserResizeColumns = true;
			IsReadOnly = false;
			AlternatingRowForeground = new SolidColorBrush(Colors.Gray);
			AlternatingRowBackground = new SolidColorBrush(Colors.Transparent);
			GridLinesVisibility = DataGridGridLinesVisibility.None;
			HeadersVisibility = DataGridHeadersVisibility.Column;
			SelectionMode = DataGridSelectionMode.Extended;

			AvailableColors = new List<SolidColorBrush>(typeof(Colors).GetRuntimeProperties()
				.Select(c => new SolidColorBrush((Color)c.GetValue(null))));

			GridLinesVisibilityOptions = Enum.GetValues(typeof(DataGridGridLinesVisibility)).Cast<DataGridGridLinesVisibility>().ToList();
			HeadersVisibilityOptions = Enum.GetValues(typeof(DataGridHeadersVisibility)).Cast<DataGridHeadersVisibility>().ToList();
			SelectionModeOptions = Enum.GetValues(typeof(DataGridSelectionMode)).Cast<DataGridSelectionMode>().ToList();

			DataGridItems = new PlantCollection().ToObservableCollection();

			FruitOrVegetableEnumValues = (FruitOrVegetableEnum[])Enum.GetValues(typeof(FruitOrVegetableEnum));
		}

		public void SortDataGrid(Func<Plant, object> selector, DataGridColumn column)
		{
			if (column.SortDirection == null || column.SortDirection == DataGridSortDirection.Descending)
			{
				DataGridItems = new ObservableCollection<Plant>((DataGridItems as IEnumerable<Plant>).OrderBy(selector));
				column.SortDirection = DataGridSortDirection.Ascending;
			}
			else
			{
				DataGridItems = new ObservableCollection<Plant>((DataGridItems as IEnumerable<Plant>).OrderByDescending(selector));
				column.SortDirection = DataGridSortDirection.Descending;
			}
		}

		public void FilterDataGrid(Func<Plant, bool> selector)
		{
			DataGridItems = new PlantCollection().ToObservableCollection();

			if (selector != null)
			{
				DataGridItems = new ObservableCollection<Plant>((DataGridItems as IEnumerable<Plant>).Where(selector));
			}
		}

		public void GroupByDataGrid(Func<Plant, object> selector)
		{
			if (DataGridItems == null || selector == null)
			{
				DataGridItems = new PlantCollection();
				return;
			}

			ObservableCollection<GroupInfoCollection<Plant>> groups = new ObservableCollection<GroupInfoCollection<Plant>>();
			var items = new List<Plant>(new PlantCollection());

			var query = items.OrderBy(item => item.PlantName)
									  .GroupBy(selector, item => item)
									  .Select(g => new { GroupName = g.Key, Items = g });

			foreach (var g in query)
			{
				GroupInfoCollection<Plant> info = new GroupInfoCollection<Plant>()
				{
					Key = g.GroupName
				};

				foreach (var item in g.Items)
				{
					info.Add(item);
				}

				groups.Add(info);
			}

			var groupedItems = new CollectionViewSource
			{
				IsSourceGrouped = true,
				Source = groups
			};

			DataGridItems = groupedItems.View;
		}

		public class GroupInfoCollection<T> : ObservableCollection<T>
		{
			public object Key { get; set; }

			public new IEnumerator<T> GetEnumerator()
			{
				return base.GetEnumerator();
			}
		}
	}
}
