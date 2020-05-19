using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharoNue.Helper
{
    class CalenderCreator
    {
        public static Grid CreateCalendar(TapGestureRecognizer tgr, DateTime dt)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.4, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.75, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.25, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.75, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) });
            

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    if (i == 0 && j != 7)
                    {
                        Label label = new Label();
                        //label.Text = HelperMethods.WeekDays(j, dt).ToString("ddd").ToUpper() + "\n" + HelperMethods.WeekDays(j, dt).ToString("dd/MM/yy");
                        label.Text = dt.AddDays(j).ToString("ddd").ToUpper() + "\n" + dt.AddDays(j).ToString("dd/MM/yy");
                        //label.ClassId = i.ToString() + j.ToString();
                        label.ClassId = i.ToString()+((int)dt.AddDays(j).DayOfWeek).ToString();
                        //grid.Children.Add(label, j, i);
                        grid.Children.Add(label, j, i);
                    }
                    else
                    {
                        if (i == 0 && j == 7)
                            grid.Children.Add(new Label { ClassId = i.ToString() + j.ToString() }, j, i);
                        else
                        {
                            if (j == 7)
                            {
                                if (i == 1)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Breakfast",
                                        ClassId = i.ToString() + j.ToString(),
                                        BackgroundColor = Color.FromHex("ffc4ad")
                                    }, j, i);
                                if (i == 2)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Snack",
                                        ClassId = i.ToString() + j.ToString(),
                                        BackgroundColor = Color.FromHex("adf5ff")
                                    }, j, i);
                                if (i == 3)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Lunch",
                                        ClassId = i.ToString() + j.ToString(),
                                        BackgroundColor = Color.FromHex("dbadff")
                                    }, j, i);
                                if (i == 4)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Dinner",
                                        ClassId = i.ToString() + j.ToString(),
                                        BackgroundColor = Color.FromHex("ffedad")
                                    }, j, i);
                            }
                            else
                            {
                                Label contentOfMeals = new Label();
                                //contentOfMeals.ClassId = i.ToString() + j.ToString();
                                contentOfMeals.ClassId = i.ToString() + ((int)dt.AddDays(j).DayOfWeek).ToString();
                                contentOfMeals.GestureRecognizers.Add(tgr);
                                //contentOfMeals.BackgroundColor = Color.AliceBlue;
                                switch (i)
                                {
                                    case 1:
                                        contentOfMeals.BackgroundColor = Color.FromHex("f5ddd2");
                                        break;
                                    case 2:
                                        contentOfMeals.BackgroundColor = Color.FromHex("d7f2fa");
                                        break;
                                    case 3:
                                        contentOfMeals.BackgroundColor = Color.FromHex("f0e1fa");
                                        break;
                                    case 4:
                                        contentOfMeals.BackgroundColor = Color.FromHex("faf4d7");
                                        break;
                                }

                                grid.Children.Add(contentOfMeals, j, i);
                            }
                        }
                    }
                }
            }
            return grid;
        }

        public static async Task ResetWeek(Grid grid, DateTime dt)
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            var startOfWeekDate = dt.Date;
            var EndOfWeekDate = dt.AddDays(6).Date;
            var meals = await _connection.Table<Meal>().Where(x => x.MealDate >= startOfWeekDate && x.MealDate <= EndOfWeekDate).ToListAsync();
            if (meals.Count != 0)
            {
                var minMealId = meals[0].MealId;
                var maxMealId = meals[meals.Count - 1].MealId;
                await _connection.Table<MealLines>().DeleteAsync(x => x.MealId >= minMealId && x.MealId <= maxMealId);
                await _connection.Table<Meal>().DeleteAsync(x => x.MealDate >= startOfWeekDate && x.MealDate <= EndOfWeekDate);
            }
        }

        public static async Task PopulateLabels(Grid grid, DateTime dt)
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            //var startOfWeekDate = HelperMethods.WeekDays(0, dt);
            //var EndOfWeekDate = HelperMethods.WeekDays(6, dt);
            var startOfWeekDate = dt.Date;
            var EndOfWeekDate = dt.AddDays(6).Date;

            //get meals headers
            var meals = await _connection.Table<Meal>().Where(x => x.MealDate >= startOfWeekDate && x.MealDate <= EndOfWeekDate).ToListAsync();
            if (meals.Count != 0)
            {
                var minMealId = meals[0].MealId;
                var maxMealId = meals[meals.Count - 1].MealId;
                //get meals lines 
                var mealsLines = await _connection.Table<MealLines>().Where(x => x.MealId >= minMealId && x.MealId <= maxMealId).ToListAsync();

                foreach (var child in grid.Children)
                {
                    var item = (Label)child;
                    int LabelmealID = int.Parse(item.ClassId[0].ToString());
                    int LabeldayId = int.Parse(item.ClassId[1].ToString());
                    if (LabelmealID > 0 && LabeldayId < 7)
                    {
                        item.Text = "";
                        var meal = meals.Where(x => x.MealDay == LabeldayId)
                                          .Where(x => x.MealType == LabelmealID).FirstOrDefault();
                        if (meal != null)
                        {
                            if (item.ClassId.Length == 2)
                            {
                                item.ClassId = item.ClassId + meal.MealId.ToString();
                            }
                            var mealLines = mealsLines.Where(x => x.MealId == meal.MealId).ToList();
                            foreach (var line in mealLines)
                            {
                                item.Text = item.Text + "•" + line.FoodDesc + "\n";
                            }
                        }
                    }
                }
            }
        }
    }
}
