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
                        label.Text = HelperMethods.WeekDays(j, dt).ToString("ddd").ToUpper() + "\n" + HelperMethods.WeekDays(j, dt).ToString("dd/MM/yy");
                        label.ClassId = i.ToString() + j.ToString();
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
                                        ClassId = i.ToString() + j.ToString()
                                    }, j, i);
                                if (i == 2)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Snack",
                                        ClassId = i.ToString() + j.ToString()
                                    }, j, i);
                                if (i == 3)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Lunch",
                                        ClassId = i.ToString() + j.ToString()
                                    }, j, i);
                                if (i == 4)
                                    grid.Children.Add(new Label
                                    {
                                        Text = "Dinner",
                                        ClassId = i.ToString() + j.ToString()
                                    }, j, i);
                            }
                            else
                            {
                                Label contentOfMeals = new Label();
                                contentOfMeals.ClassId = i.ToString() + j.ToString();
                                contentOfMeals.GestureRecognizers.Add(tgr);
                                contentOfMeals.BackgroundColor = Color.AliceBlue;

                                grid.Children.Add(contentOfMeals, j, i);
                            }
                        }
                    }
                }
            }
            return grid;
        }

        public static async Task PopulateLabels(Grid grid, DateTime dt)
        {
            SQLiteAsyncConnection _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            var startOfWeekDate = HelperMethods.WeekDays(0, dt);
            var EndOfWeekDate = HelperMethods.WeekDays(6, dt);

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
                                item.Text = item.Text + line.FoodDesc + "\n";
                            }
                        }
                    }
                }
            }
        }
    }
}
