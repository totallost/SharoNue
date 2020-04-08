using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharoNue.Helper
{
    class DefaultSettings
    {
        public async Task SetDefaultSettings()
        {
            var _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<Settings> settings = new List<Settings>();

            for(int i=0; i<7; i++)
            {
                for(int j=1; j<5; j++)
                {
                    settings.Add(new Settings
                    {
                        IdDay = i,
                        IsAutoPopulate = true,
                        MealID = j,
                        ListOfConstantFoods = "",
                        ListOfFoodTypes = "",
                        IsActive = true
                    });
                };
            };
            await _connection.InsertAllAsync(settings);
        }

        public async Task PopulateFoods()
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<FoodTypes> foodTypes = new List<FoodTypes>();
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Carbs",
                Id = 1
            });
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Vegtables",
                Id = 2
            });
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Protein",
                Id = 3
            });
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Soup",
                Id = 4
            });
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Salad",
                Id = 5
            });
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Desert",
                Id = 6
            });
            foodTypes.Add(new FoodTypes
            {
                FoodTypeDescription = "Fruit",
                Id = 7
            });

            await _connection.InsertAllAsync(foodTypes);
            List<Foods> foods = new List<Foods>();
            foods.Add(new Foods
            {
                FoodDescription = "Eggs",
                FoodType = (int)foodTypesEnum.Protein,
                Id = 1,
                MealTypeList = ""
            });
            foods.Add(new Foods
            {
                FoodDescription = "Salad",
                FoodType = (int)foodTypesEnum.Salad,
                Id = 2,
                MealTypeList = null
            });
            foods.Add(new Foods
            {
                FoodDescription = "Bread",
                FoodType = (int)foodTypesEnum.Carbs,
                Id = 3,
                MealTypeList = null
            });
            await _connection.InsertAllAsync(foods);
        }

        public enum foodTypesEnum
        {
            Carbs = 1,
            Vegetables,
            Protein,
            Soup,
            Salad,
            Desert,
            Fruit
        }
    }
}
