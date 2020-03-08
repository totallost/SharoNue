using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
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
        public AddNewFood()
        {
            InitializeComponent();
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
        }
        protected override async void OnAppearing()
        {
            var x = _connection.Table<FoodTypes>().ToListAsync();
            await PopulateSelectBox();
        }
        public async Task PopulateSelectBox()
        {
            var foodTypesList = await _connection.Table<FoodTypes>().ToListAsync();
            if (foodTypesList != null)
            {
                SelectBox.ItemsSource = foodTypesList;
                SelectBox.SelectedItem = foodTypesList[0];
            }
        }

        private async void Add_Button_Clicked(object sender, EventArgs e)
        {
            if (FoodDesc.Text !=null)
            {
                bool isExist = false;
                var foodList = await _connection.Table<Foods>().ToListAsync();
                foreach(var food in foodList)
                {
                    if (food.FoodDescription.ToLower() == FoodDesc.Text.ToLower())
                    {
                        isExist = true;
                        break;
                    }   
                }
                if (isExist == false)
                {
                    var newFood = new Foods()
                    {
                        FoodDescription = FoodDesc.Text,
                        FoodType = SelectBox.SelectedIndex
                    };
                    var x = await _connection.InsertAsync(newFood);
                    if (x == 1)
                        await DisplayAlert("Notification", newFood.FoodDescription + " was added", "OK");
                    else
                        await DisplayAlert("Error", "Failed Adding the Food", "OK");
                    FoodDesc.Text = "";
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
    }
}