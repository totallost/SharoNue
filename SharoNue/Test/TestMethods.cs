using SharoNue.Helper;
using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharoNue.Test
{
    class TestMethods
    {
        public static async Task populateDatabase(Grid grid)
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<Meal> meals = new List<Meal>();
            foreach (var child in grid.Children)
            {
                var item = (Label)child;
                int mealID = int.Parse(item.ClassId) / 10;
                int dayId = int.Parse(item.ClassId) % 10;
                if (mealID > 0 && dayId < 7)
                {
                    Meal meal = new Meal
                    {
                        MealDay = dayId,
                        MealType = mealID,
                        MealDate = HelperMethods.WeekDays(dayId, DateTime.Now),
                    };
                    int x = 0;
                    if (meal != null)
                    {
                        x = await _connection.InsertAsync(meal);
                    }

                    List<MealLines> mealLinesList = new List<MealLines>();
                    MealLines mealLines = new MealLines()
                    {
                        MealId = meal.MealId,
                        MealTypeId = mealID,
                        FoodDesc = "test1"
                    };
                    mealLinesList.Add(mealLines);
                    MealLines mealLines1 = new MealLines()
                    {
                        MealId = meal.MealId,
                        MealTypeId = mealID,
                        FoodDesc = "test2"
                    };
                    mealLinesList.Add(mealLines1);
                    await _connection.InsertAllAsync(mealLinesList);

                }
            }

        }
    }
}
