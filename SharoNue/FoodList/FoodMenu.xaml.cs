using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharoNue.Test;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SharoNue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FoodMenu : ContentPage
    {
        private SQLiteAsyncConnection _connection;
        private ObservableCollection<Foods> FoodsCollection;
        public FoodMenu()
        {
            InitializeComponent();

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
        }
        protected override async void OnAppearing()
        {
            await PopulateStackLayout();
        }

        private async Task PopulateStackLayout()
        {
            var Foods = await _connection.Table<Foods>().ToListAsync();
            if (Foods != null)
            {
                FoodsCollection = null;
                FoodsCollection = new ObservableCollection<Foods>(Foods);
            }
            listView.ItemsSource = FoodsCollection.OrderBy(x => x.FoodDescription.ToLower());

        }

        private async void More_Clicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("What do you want to do?", "Cancel", null, "Add New Food", "Go to Food Types");
            switch (action)
            {
                case "Add New Food":
                    await Navigation.PushAsync(new AddNewFood(null));
                    break;
                case "Go to Food Types":
                    break;
            }
        }
        private async void Edit_Button_Clicked(object sender, EventArgs e)
        {
            var itemsender = (Button)sender;
            Foods item = (Foods)itemsender?.CommandParameter;
            await Navigation.PushAsync(new AddNewFood(item));
        }
        private async void Delete_Button_Clicked(object sender, EventArgs e)
        {
            var itemsender = (Xamarin.Forms.Button)sender;
            Foods item = (Foods)itemsender?.CommandParameter;
            var isOk = await DisplayAlert("Delete", "Are you sure you want to Delete", "Delete", "Cancel");
            if (isOk)
            {
                var x = await _connection.Table<Foods>().DeleteAsync(y => y.Id == item.Id);
                FoodsCollection.Remove(item);
                listView.ItemsSource = FoodsCollection;
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchString = e.NewTextValue.ToLower();
            listView.ItemsSource = FoodsCollection.Where(x => x.FoodDescription.ToLower().Contains(searchString))
                                                  .ToList();
        }
    }
}