using System;
using System.Collections.Generic;
using System.Linq;
using static Uno.Gallery.Entities.Data.Plant;

namespace Uno.Gallery.Entities.Data
{
	public class Plant : IComparable
	{
		public Plant(
			string plantName,
			int plantsCount,
			FruitOrVegetableEnum fruitOrVegetable,
			DateTime plantDate,
			bool isWatered)
		{
			PlantName = plantName;
			PlantsCount = plantsCount;
			FruitOrVegetable = fruitOrVegetable;
			PlantDate = plantDate;
			IsWatered = isWatered;
		}

		public string PlantName { get; set; }
		public int PlantsCount { get; set; }
		public enum FruitOrVegetableEnum { Fruit, Vegetable }
		public FruitOrVegetableEnum FruitOrVegetable { get; set; }
		public DateTime PlantDate { get; set; }
		public bool IsWatered { get; set; }

		public IEnumerable<FruitOrVegetableEnum> FruitOrVegetableEnumValues { get; } =
			Enum.GetValues(typeof(FruitOrVegetableEnum)).Cast<FruitOrVegetableEnum>();

		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (obj is Plant otherPlant)
			{
				return PlantName.CompareTo(otherPlant.PlantName);
			}
			else
			{
				throw new ArgumentException("Object is not a Plant");
			}
		}
	}

	public class PlantCollection : List<Plant>
	{
		public PlantCollection() : base(GetPlants()) { }

		private static IEnumerable<Plant> GetPlants()
		{
			yield return new Plant("Tomato", 12, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 5, 29), true);
			yield return new Plant("Cucumber", 10, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 6, 15), true);
			yield return new Plant("Bell Pepper", 15, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 4, 15), false);
			yield return new Plant("Lettuce", 8, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 4, 1), true);
			yield return new Plant("Strawberry", 35, FruitOrVegetableEnum.Fruit, new DateTime(2021, 5, 1), true);
			yield return new Plant("Carrot", 5, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 3, 15), false);
			yield return new Plant("Watermelon", 1, FruitOrVegetableEnum.Fruit, new DateTime(2021, 6, 1), true);
			yield return new Plant("Blueberries", 31, FruitOrVegetableEnum.Fruit, new DateTime(2021, 6, 20), true);
			yield return new Plant("Eggplant", 7, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 5, 10), false);
			yield return new Plant("Broccoli", 9, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 4, 20), false);
			yield return new Plant("Raspberry", 18, FruitOrVegetableEnum.Fruit, new DateTime(2021, 5, 25), false);
			yield return new Plant("Pumpkin", 3, FruitOrVegetableEnum.Fruit, new DateTime(2021, 6, 10), true);
			yield return new Plant("Green Beans", 50, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 5, 5), true);
			yield return new Plant("Kale", 10, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 4, 15), true);
			yield return new Plant("Grapes", 25, FruitOrVegetableEnum.Fruit, new DateTime(2021, 9, 1), true);
			yield return new Plant("Potato", 6, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 10, 15), false);
			yield return new Plant("Orange", 20, FruitOrVegetableEnum.Fruit, new DateTime(2021, 11, 1), true);
			yield return new Plant("Zucchini", 11, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 8, 20), false);
			yield return new Plant("Apple", 30, FruitOrVegetableEnum.Fruit, new DateTime(2021, 10, 1), true);
			yield return new Plant("Spinach", 5, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 7, 15), true);
			yield return new Plant("Pear", 15, FruitOrVegetableEnum.Fruit, new DateTime(2021, 8, 1), true);
			yield return new Plant("Celery", 7, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 9, 10), true);
			yield return new Plant("Pineapple", 2, FruitOrVegetableEnum.Fruit, new DateTime(2021, 12, 1), true);
			yield return new Plant("Cherry", 40, FruitOrVegetableEnum.Fruit, new DateTime(2021, 6, 1), true);
			yield return new Plant("Mango", 8, FruitOrVegetableEnum.Fruit, new DateTime(2021, 9, 15), true);
			yield return new Plant("Cabbage", 12, FruitOrVegetableEnum.Vegetable, new DateTime(2021, 11, 15), true);
		}
	}
}
