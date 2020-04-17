using SharoNue.Helper;
using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SharoNue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddNewFood : ContentPage
    {
        private SQLiteAsyncConnection _connection;
        private Foods _food;
        private List<FoodTypes> _GlobalFoodTypesList;
        private ObservableCollection<string> _FoodTypeList = new ObservableCollection<string>();
        public AddNewFood(Foods foods)
        {
            InitializeComponent();
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            _food = foods;
        }
        protected override async void OnAppearing()
        {
            await PopulateSelectBox();
            if (_food != null)
                PopulatePage(_food);
        }
        public async Task PopulateSelectBox()
        {
            var foodTypesList = await _connection.Table<FoodTypes>().ToListAsync();
            if (foodTypesList != null)
            {
                SelectBox.ItemsSource = foodTypesList;
                _GlobalFoodTypesList = foodTypesList;
            }
        }

        private async Task<bool> CheckIfFoodExist(string foodDesc)
        {
            var foodList = await _connection.Table<Foods>().ToListAsync();
            foreach (var food in foodList)
            {
                if (food.FoodDescription.ToLower() == foodDesc.ToLower() && _food.Id != food.Id)
                {
                    return true;
                }
            }
            return false;
        }

        private async void Add_Button_Clicked(object sender, EventArgs e)
        {
            if (FoodDesc.Text !=null)
            {
                bool isExist = await CheckIfFoodExist(FoodDesc.Text);
                if (isExist == false)
                {
                    var newFood = new Foods()
                    {
                        FoodDescription = FoodDesc.Text,
                        FoodType = SelectBox.SelectedIndex+1,
                        MealTypeList = CreateMealTypeList(), 
                    };
                    string listString = "";
                    foreach (var type in _FoodTypeList)
                    {
                        listString += type.ToString() + ",";
                    }
                    newFood.FoodTypeList = listString;
                    var x = await _connection.InsertAsync(newFood);
                    if (x == 1)
                        await DisplayAlert("Notification", newFood.FoodDescription + " was added", "OK");
                    else
                        await DisplayAlert("Error", "Failed Adding the Food", "OK");
                    FoodDesc.Text = "";
                    ChkBreakfast.IsChecked = false;
                    ChkSnack.IsChecked = false;
                    ChkLunch.IsChecked = false;
                    ChkDinner.IsChecked = false;
                    _FoodTypeList.Clear();
                }
                else
                {
                    await DisplayAlert("Error", FoodDesc.Text + " Already exists", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "The Food Description Shouldn't be empty", "OK");
            }
        }

        private string CreateMealTypeList()
        {
            string result = "";
            if (ChkBreakfast.IsChecked)
                result = result + "1,";
            if (ChkSnack.IsChecked)
                result = result + "2,";
            if (ChkLunch.IsChecked)
                result = result + "3,";
            if (ChkDinner.IsChecked)
                result = result + "4";
            return result;
        }
        private void PopulatePage(Foods foods)
        {
            PopulateFoodTypes(foods);
            Btn.Text = "Save";
            Btn.Clicked -= Add_Button_Clicked;
            Btn.Clicked += Save_Button_Clicked;
            FoodDesc.Text = foods.FoodDescription;
            //SelectBox.SelectedIndex = foods.FoodType-1;
            if (foods.MealTypeList != null)
            {
                if (foods.MealTypeList.Contains("1"))
                    ChkBreakfast.IsChecked = true;
                if (foods.MealTypeList.Contains("2"))
                    ChkSnack.IsChecked = true;
                if (foods.MealTypeList.Contains("3"))
                    ChkLunch.IsChecked = true;
                if (foods.MealTypeList.Contains("4"))
                    ChkDinner.IsChecked = true;
            }

        }

        private void PopulateFoodTypes(Foods foods)
        {
            if (foods.FoodTypeList == null)
                foods.FoodTypeList = ",";
            var typelist = foods.FoodTypeList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            _FoodTypeList = new ObservableCollection<string>(typelist);
            FoodTypeList.ItemsSource = _FoodTypeList;
        }

        private async void Save_Button_Clicked(object sender, EventArgs e)
        {
            _food.FoodDescription = FoodDesc.Text;
            _food.FoodType = SelectBox.SelectedIndex+1;
            _food.MealTypeList = CreateMealTypeList();
            _food.FoodTypeList = string.Join(",",_FoodTypeList);
            if (!await CheckIfFoodExist(_food.FoodDescription))
            {
                await _connection.UpdateAsync(_food);
                await DisplayAlert("Save", "Food Saved", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", FoodDesc.Text+" already exists", "OK");
            }
        }

        private void SelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectBox.SelectedItem.ToString() != "0")
            {
                var addItem = (FoodTypes)SelectBox.SelectedItem;
                if (addItem != null)
                {
                    _FoodTypeList.Add(addItem.FoodTypeDescription);
                    FoodTypeList.ItemsSource = _FoodTypeList;
                }
            }
        }

        private void DeleteFoodType_Button_Clicked(object sender, EventArgs e)
        {
            var itemSender = (Button)sender;
            _FoodTypeList.Remove(itemSender.CommandParameter.ToString());
            FoodTypeList.ItemsSource = _FoodTypeList;
        }
    }
}