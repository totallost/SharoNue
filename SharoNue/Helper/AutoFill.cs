using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharoNue.Helper
{
    class AutoFill
    {
        private SQLiteAsyncConnection _connection;
        //this next method creates the Meal
        public async Task populateDatabase(Grid grid, int daysFromToday)
        {
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<Meal> meals = new List<Meal>();
            var foods = await _connection.Table<Foods>().ToListAsync();
            var settings = await _connection.Table<Settings>().ToListAsync();
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
                                      .Select(x => x.IsActive).SingleOrDefault();
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

        public async Task PopulateMeal(Meal meal, List<Foods> foods, List<Settings> settings)
        {
            List<MealLines> mealLinesList = new List<MealLines>();
            var currentSetting = settings.Where(x => x.IdDay == meal.MealDay)
                                         .Where(x => x.MealID == meal.MealType)
                                         .SingleOrDefault();
            List<string> listOfFoods;
            if (currentSetting.IsAutoPopulate)
            {
                listOfFoods = currentSetting.ListOfFoodTypes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                            .ToList();
                if (listOfFoods.Count > 0)
                {
                    foreach (var foodType in listOfFoods)
                    {
                        var foodTypeDB = await _connection.Table<FoodTypes>()
                                                           .Where(x => x.FoodTypeDescription == foodType)
                                                           .FirstOrDefaultAsync();
                        if (foodTypeDB != null)
                        {
                            var mealType = meal.MealType.ToString();
                            var listOfRelevantFoods = await _connection.Table<Foods>()
                                                                 .Where(x => x.FoodType == foodTypeDB.Id)
                                                                 .Where(x => x.MealTypeList.Contains(mealType))
                                                                 .ToListAsync();
                            if (listOfRelevantFoods.Count != 0)
                            {
                                Random random = new Random();
                                var index = random.Next(listOfRelevantFoods.Count);
                                var foodDesc = listOfRelevantFoods[index].FoodDescription;
                                mealLinesList.Add(new MealLines
                                {
                                    MealId = meal.MealId,
                                    MealTypeId = meal.MealType,
                                    FoodDesc = foodDesc
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                listOfFoods = currentSetting.ListOfConstantFoods.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                .ToList();
                if (listOfFoods.Count > 0)
                {
                    foreach (var food in listOfFoods)
                    {
                        mealLinesList.Add(new MealLines
                        {
                            MealId = meal.MealId,
                            MealTypeId = meal.MealType,
                            FoodDesc = food
                        });
                    }
                }
            }
            if (mealLinesList.Count > 0)
                await _connection.InsertAllAsync(mealLinesList);
        }
    }
}
