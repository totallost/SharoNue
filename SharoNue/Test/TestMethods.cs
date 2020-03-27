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
            var settings = await _connection.Table<Settings>().ToListAsync();
            Random random = new Random();
            foreach (var child in grid.Children)
            {
                var item = (Label)child;
                int mealID = int.Parse(item.ClassId[0].ToString());
                int dayId = int.Parse(item.ClassId[1].ToString());
                if (mealID > 0 && dayId < 7)
                {
                    //in the settings you have a bool that determin if you are allowed
                    //to auto populate the meal of the day
                    var allowed = settings.Where(x => x.IdDay == dayId)
                                      .Where(x => x.MealID == mealID)
                                      .Select(x => x.IsAutoPopulate).SingleOrDefault();
                    if (allowed)
                    {
                        Meal meal = null;
                        if (item.ClassId.Length > 2)
                        {
                            var id = int.Parse(item.ClassId.Substring(2, item.ClassId.Length - 2));

                            meal = await _connection.Table<Meal>().Where(m => m.MealId == id)
                                                    .FirstOrDefaultAsync();
                        }
                        //create a new meal if not exists in the DB
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
                        await PopulateMeal(meal, foods, settings);
                    }
                }
            }
        }
        public static async Task PopulateMeal(Meal meal, List<Foods> foods, List<Settings> settings)
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<MealLines> mealLinesList = new List<MealLines>();
            if(meal.MealType == (int)MealType.Breakfast || 
                meal.MealType == (int)MealType.Lunch || 
                meal.MealType == (int)MealType.Dinner)
            {
                Random random = new Random();
                var foodsByMeals = foods.Where(x => x.MealTypeList.Contains(meal.MealType.ToString())).ToList();
                
                var foodCarbList = foodsByMeals.Where(f => f.FoodType == (int)foodTypesEnum.Carbs).ToList();
                var rnd = random.Next(1, foodCarbList.Count + 1);
                var firstMeal = foodCarbList[rnd-1];
                mealLinesList.Add(new MealLines
                {
                    MealId = meal.MealId,
                    MealTypeId = meal.MealType,
                    FoodDesc = firstMeal.FoodDescription
                });

                var foodProteinList = foodsByMeals.Where(f => f.FoodType == (int)foodTypesEnum.Protein).ToList();
                rnd = random.Next(1, foodProteinList.Count + 1);
                var secMeal = foodProteinList[rnd-1];
                mealLinesList.Add(new MealLines
                {
                    MealId = meal.MealId,
                    MealTypeId = meal.MealType,
                    FoodDesc = secMeal.FoodDescription
                });

                var foodOthersList = foodsByMeals.Where(f => f.FoodType != (int)foodTypesEnum.Carbs && f.FoodType != (int)foodTypesEnum.Protein).ToList();
                rnd = random.Next(1, foodOthersList.Count + 1);
                var thirdMeal = foodOthersList[rnd-1];
                mealLinesList.Add(new MealLines
                {
                    MealId = meal.MealId,
                    MealTypeId = meal.MealType,
                    FoodDesc = thirdMeal.FoodDescription
                });

                await _connection.InsertAllAsync(mealLinesList);
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
                MealTypeList = ""
            });
            foods.Add(new Foods
            {
                FoodDescription = "Bread",
                FoodType = (int)foodTypesEnum.Carbs,
                Id = 3,
                MealTypeList = ""
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
