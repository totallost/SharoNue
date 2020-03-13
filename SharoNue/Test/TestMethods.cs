using SharoNue.Helper;
using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharoNue.Test
{
    class TestMethods
    {
        public static async Task populateDatabase(Grid grid, int daysFromToday)
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<Meal> meals = new List<Meal>();
            var foods = await _connection.Table<Foods>().ToListAsync();
            Random random = new Random();
            foreach (var child in grid.Children)
            {
                var item = (Label)child;
                int mealID = int.Parse(item.ClassId[0].ToString());
                int dayId = int.Parse(item.ClassId[1].ToString());
                if (mealID > 0 && dayId < 7)
                {
                    //var date = DateTime.Now.AddDays(daysFromToday);
                    Meal meal = null;
                    if (item.ClassId.Length > 2)
                    {
                        var id = int.Parse(item.ClassId.Substring(2, item.ClassId.Length -2));

                        meal = await _connection.Table<Meal>().Where(m => m.MealId == id)
                                                 .FirstOrDefaultAsync();
                    }
                    if (meal == null)
                    {
                        meal = new Meal
                        {
                            MealDay = dayId,
                            MealType = mealID,
                            MealDate = HelperMethods.WeekDays(dayId, DateTime.Now.AddDays(daysFromToday)),
                        };
                        int x = 0;
                        if (meal != null)
                        {
                            x = await _connection.InsertAsync(meal);
                        }
                    }

                    var rnd = random.Next(1, foods.Count);
                    var firstMeal = foods.SingleOrDefault(f => f.Id == rnd);

                    List<MealLines> mealLinesList = new List<MealLines>();
                    MealLines mealLines = new MealLines()
                    {
                        MealId = meal.MealId,
                        MealTypeId = mealID,
                        FoodDesc = firstMeal.FoodDescription
                    };
                    rnd = random.Next(1, foods.Count);
                    var SecMeal = foods.SingleOrDefault(f => f.Id == rnd);
                    mealLinesList.Add(mealLines);
                    MealLines mealLines1 = new MealLines()
                    {
                        MealId = meal.MealId,
                        MealTypeId = mealID,
                        FoodDesc = SecMeal.FoodDescription
                    };
                    mealLinesList.Add(mealLines1);
                    await _connection.InsertAllAsync(mealLinesList);

                }
            }
        }
        public static async Task PopulateFoods()
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
                Id = 5
            });

            await _connection.InsertAllAsync(foodTypes);
            List<Foods> foods = new List<Foods>();
            foods.Add(new Foods
            {
                FoodDescription = "Eggs",
                FoodType = 3,
                Id  = (int)foodTypesEnum.Carbs
            });
            foods.Add(new Foods
            {
                FoodDescription = "Salad",
                FoodType = 2,
                Id = (int)foodTypesEnum.Salad
            });
            foods.Add(new Foods
            {
                FoodDescription = "Bread",
                FoodType = 1,
                Id = (int)foodTypesEnum.Carbs
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
