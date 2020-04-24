using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SharoNue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MealDetails : ContentPage
    {
        private SQLiteAsyncConnection _connection;
        private ObservableCollection<MealLines> mealLines { get; set; }
        private int GlobalMealId { get; set; }
        private int GlobalDay { get; set; }
        private int GlobalMealType { get; set; }
        public MealDetails(int day, int mealId, int mealType)
        {  
            InitializeComponent();
            mealLines = null;
            GlobalMealId = mealId;
            GlobalDay = day;
            GlobalMealType = mealType;

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
        }

        protected override async void OnAppearing()
        {
            await PopulateStackLayout();
        }

        private async Task PopulateStackLayout()
        {
            var meal = await _connection.Table<Meal>().Where(x => x.MealDay == GlobalDay && x.MealId == GlobalMealId).FirstOrDefaultAsync();
            if (meal != null)
            {
                List<MealLines> mealLinesFromDb = await _connection.Table<MealLines>().Where(x => x.MealId == meal.MealId).ToListAsync();
                mealLines = new ObservableCollection<MealLines>(mealLinesFromDb);   
            }
            listView.ItemsSource = mealLines;

        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("What do you want to do?", "Cancel", null, "Add", "Copy to another day", "Reset");
            switch (action)
            {
                case "Add":
                    string result = await DisplayPromptAsync("Add", "Add new meal", "OK", "Cancel", "Add New Item", 100, Keyboard.Default,"");
                    if (result !=null)
                    {
                        mealLines.Add(new MealLines
                        {
                            FoodDesc = result,
                            MealId = GlobalMealId,
                            MealTypeId = GlobalMealType
                        });
                        var x = await _connection.Table<MealLines>().DeleteAsync(y => y.MealId == GlobalMealId);
                        var z = await _connection.InsertAllAsync(mealLines);

                        await CheckIfFoodExists(result);
                    }
                    break;
                case "Copy to another day":
                    var checkedItems = mealLines.Where(x => x.IsMet == true).ToList();
                    if(checkedItems.Count()==0)
                    {
                        await DisplayAlert("Warning", "You didn't select any Meal" , "OK");
                        break;
                    }
                    await Navigation.PushAsync(new ChooseMealCalendar(checkedItems));
                    break;
                case "Reset":
                    var isOk = await DisplayAlert("Delete", "Are you sure you want to Delete", "Delete", "Cancel");
                    if (isOk)
                    {
                        var x = await _connection.Table<MealLines>().DeleteAsync(y => y.MealId == GlobalMealId);
                        mealLines = null;
                        listView.ItemsSource = null;
                    }
                    break;
                default:
                    break;
            }
        }

        private async void Delete_Button_Clicked(object sender, EventArgs e)
        {
            var itemsender = (Xamarin.Forms.Button)sender;
            MealLines item = (MealLines)itemsender?.CommandParameter;
            var isOk =await DisplayAlert("Delete", "Are you sure you want to Delete", "Delete", "Cancel");
            if (isOk)
            {
                var x = await _connection.Table<MealLines>().DeleteAsync(y => y.MealId == item.MealId);
                mealLines.Remove(item);
                var z = await _connection.InsertAllAsync(mealLines);
            }
        }

        private async void Edit_Button_Clicked(object sender, EventArgs e)
        {
            var itemsender = (Xamarin.Forms.Button)sender;
            MealLines item = (MealLines)itemsender?.CommandParameter;
            //string result = await DisplayPromptAsync("Edit "+item.FoodDesc, "Edit your meal","OK","Cancel", item.FoodDesc,25,Keyboard.Default);
            string result = await DisplayPromptAsync("Edit "+item.FoodDesc, "Edit your meal", "OK", "Cancel", "", 100,Keyboard.Default, item.FoodDesc);
            if (result != null)
            {
                mealLines.Where(x => x.Id == item.Id).FirstOrDefault().FoodDesc= result;
                listView.ItemsSource = null;
                listView.ItemsSource = mealLines;
                var f = await _connection.Table<MealLines>().DeleteAsync(y => y.MealId == GlobalMealId);
                var z = await _connection.InsertAllAsync(mealLines);
            }

        }
        private async Task CheckIfFoodExists(string food)
        {
            var exist = await _connection.Table<Foods>().Where(x => x.FoodDescription.ToLower() == food.ToLower()).FirstOrDefaultAsync();
            if (exist == null)
            {
                var answer = await DisplayAlert("Info", "This food doesn't exist in the food-list. whould you like to add it?", "YES", "NO");
                if (answer)
                {
                    var newFood = new Foods
                    {
                        FoodDescription = food,
                        MealTypeList = mealLines[0].MealTypeId.ToString()
                    };
                    await _connection.InsertAsync(newFood);
                }
            }
        }

    }
}