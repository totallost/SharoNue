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
                        //await PopulateMeal(meal, foods, settings);
                        await PopulateMeal(meal, foods, settings);                    }
                }
            }
        }
        private List<Foods> CreateFoodsList(List<Foods> foods, List<string> settingsTypeList, Meal meal)
        {
            if (settingsTypeList.Count == 0)
                return new List<Foods>();
            

            var newList = new List<Foods>();

            foreach(var food in foods)
            {
                if (food.FoodTypeList == null)
                    continue;
                //check if mealtype is checked in this food 
                var mealTypList = food.MealTypeList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (mealTypList.Count == 0)
                    continue;
                if (!mealTypList.Exists(x => x == meal.MealType.ToString()))
                    continue;
          
                var foodtypeList = food.FoodTypeList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (foodtypeList.Count == 0)
                    continue;
                if (foodtypeList.Count > settingsTypeList.Count)
                    continue;
                if (settingsTypeList.Count == 1 && foodtypeList.Count == 1)
                    if (settingsTypeList[0] == foodtypeList[0])
                    {
                        newList.Add(food);
                        continue;
                    }
                //var ifEqualList = new List<string>();
                int counter = 0;
                foreach(var foodtype in foodtypeList)
                {
                    var isExist = settingsTypeList.Exists(x => x == foodtype);
                    if (isExist)
                        counter++;
                }
                if (foodtypeList.Count == counter)
                    newList.Add(food);

            }
            return newList;
        }

        private async Task PopulateMeal(Meal meal, List<Foods> foods, List<Settings> settings)
        {
            var currentSetting = settings.Where(x => x.IdDay == meal.MealDay)
                                         .Where(x => x.MealID == meal.MealType)
                                         .SingleOrDefault();
            List<MealLines> mealLinesList = new List<MealLines>();
            if (currentSetting.IsAutoPopulate)
            {
                var settingsTypeList = currentSetting.ListOfFoodTypes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();


                //creating the food list with all the valid food that can be populated.
                var newFoodList = CreateFoodsList(foods, settingsTypeList, meal);              

                //the populating
                if (newFoodList.Count != 0)
                {
                    Random random = new Random();
                    int foodListCount = newFoodList.Count;
                    int counter = 0;
                    while (counter < foodListCount)
                    {
                        var index = random.Next(newFoodList.Count);
                        var foodToPopulate = newFoodList[index];
                        var foodTypeOfFoodToPopulate = foodToPopulate.FoodTypeList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var isOk = false;

                        List<string> indexesToDelete = new List<string>();
                        foreach (var foodToPopulateType in foodTypeOfFoodToPopulate)
                        {
                            //check the how many times does the foodtype exist 
                            var x = settingsTypeList.Exists(f => f == foodToPopulateType);
                            if (x)
                                indexesToDelete.Add(foodToPopulateType);
                        }
                        if (indexesToDelete.Count == foodTypeOfFoodToPopulate.Count)
                        {
                            //if the amount of foodtype exist the same as in the food then delete from foodtype settings
                            isOk = true;
                            foreach (var del in indexesToDelete)
                            {
                                var i = settingsTypeList.FindIndex(x => x == del);
                                settingsTypeList[i] = "";
                            }
                        }
                        if (isOk)
                        {
                            //add meal line 
                            mealLinesList.Add(new MealLines
                            {
                                MealId = meal.MealId,
                                MealTypeId = meal.MealType,
                                FoodDesc = foodToPopulate.FoodDescription
                            });
                        }
                        //remove food from foods list
                        newFoodList.RemoveAt(index);
                        //of all food type settings are empty (it means all are populated) then end loop
                        var notEmptyTypes = settingsTypeList.Count(x => x != "");
                        if (notEmptyTypes == 0)
                        {
                            counter = foodListCount;
                            continue;
                        }
                        counter++;
                    }
                }
            }
            else
            {
                var settingsConstantFoodList = currentSetting.ListOfConstantFoods.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (settingsConstantFoodList.Count > 0)
                {
                    foreach (var food in settingsConstantFoodList)
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
