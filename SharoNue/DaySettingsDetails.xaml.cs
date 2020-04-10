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
    public partial class DaySettingsDetails : ContentPage
    {
        private SQLiteAsyncConnection _connection;
        private List<Settings> _settings;
        private int _day;
        private ObservableCollection<string> _ConstantFoodList;
        private ObservableCollection<string> _FoodTypeList;
        private bool IsAutoFilledOn = true;
        public DaySettingsDetails(Button item)
        {
            InitializeComponent();
            _settings = null;
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            PageTitle.Text = item.Text;
            switch (item.Text)
            {
                case "Sunday":
                    _day = 0;
                    break;
                case "Monday":
                    _day = 1;
                    break;
                case "Tuesday":
                    _day = 2;
                    break;
                case "Wednesday":
                    _day = 3;
                    break;
                case "Thursday":
                    _day = 4;
                    break;
                case "Friday":
                    _day = 5;
                    break;
                case "Saturday":
                    _day = 6;
                    break;
            }
        }
        protected override async void OnAppearing()
        {
            await GetSettings();
            await PopulateSelectBox();
            MealTypePicker.SelectedIndex = 0;
            PopulateSettings(0);
        }
        private async Task GetSettings()
        {
            _settings = await _connection.Table<Settings>().Where(x => x.IdDay == _day).ToListAsync();
        }
        private void PopulateSettings(int mealType)
        {
            mealType += 1;
            IsAutoFilledOn = _settings.Where(x => x.MealID == mealType)
                        .Select(x => x.IsAutoPopulate)
                        .SingleOrDefault();
            IsAutoFilled.IsToggled = IsAutoFilledOn;
            var tempConstantFoodList = _settings.Where(x => x.MealID == mealType)
                                    .Select(x => x.ListOfConstantFoods)
                                    .SingleOrDefault().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .ToList();
            _ConstantFoodList = new ObservableCollection<string>(tempConstantFoodList);
            var tempFoodTypesList = _settings.Where(x => x.MealID == mealType)
                                    .Select(x => x.ListOfFoodTypes)
                                    .SingleOrDefault().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .ToList();
            _FoodTypeList = new ObservableCollection<string>(tempFoodTypesList);
            FoodTypeList.ItemsSource = _FoodTypeList;
            ConstantFoodList.ItemsSource = _ConstantFoodList;

            ConstantFoodsStack.IsVisible = !IsAutoFilledOn;
            FoodTypeStack.IsVisible = IsAutoFilledOn;

        }

        private void IsAutoFilledOn_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value != IsAutoFilledOn)
            {
                var mealType = MealTypePicker.SelectedIndex + 1;
                IsAutoFilledOn = e.Value;
                foreach (var item in _settings)
                {
                    if (item.MealID == mealType)
                        item.IsAutoPopulate = IsAutoFilledOn;
                }
            }
            ConstantFoodsStack.IsVisible = !IsAutoFilledOn;
            FoodTypeStack.IsVisible = IsAutoFilledOn;
        }
        public async Task PopulateSelectBox()
        {
            var foodTypesList = await _connection.Table<FoodTypes>().ToListAsync();
            if (foodTypesList != null)
            {
                SelectBox.ItemsSource = foodTypesList;
            }
            SelectBox.SelectedItem = 0;
        }

        private void MealTypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (Picker)sender;
            PopulateSettings(item.SelectedIndex);
        }

        private void AddFoodTypes_Clicked(object sender, EventArgs e)
        {
            if(SelectBox.SelectedItem.ToString() != "0")
            {
                var addItem = (FoodTypes)SelectBox.SelectedItem;
                _FoodTypeList.Add(addItem.FoodTypeDescription);

                var mealType = MealTypePicker.SelectedIndex + 1;
                foreach (var setting in _settings)
                {
                    if (setting.MealID == mealType)
                        setting.ListOfFoodTypes = string.Join(",", _FoodTypeList);
                }
            }       
        }

        private void DeleteFoodType_Button_Clicked(object sender, EventArgs e)
        {
            var itemSender = (Button)sender;
            _FoodTypeList.Remove(itemSender.CommandParameter.ToString());

            var mealType = MealTypePicker.SelectedIndex + 1;
            foreach (var setting in _settings)
            {
                if (setting.MealID == mealType)
                    setting.ListOfFoodTypes = string.Join(",", _FoodTypeList);
            }
        }

        private void DeleteConstantFood_Button_Clicked(object sender, EventArgs e)
        {
            var itemSender = (Button)sender;
            _ConstantFoodList.Remove(itemSender.CommandParameter.ToString());
            var mealType = MealTypePicker.SelectedIndex + 1;
            foreach (var setting in _settings)
            {
                if (setting.MealID == mealType)
                    setting.ListOfConstantFoods = string.Join(",", _ConstantFoodList);
            }
        }

        private void AddConstantFood_Clicked(object sender, EventArgs e)
        {
            if (ConstantFoodEntry.Text != null)
            {
                var addItem = ConstantFoodEntry.Text;
                _ConstantFoodList.Add(addItem);

                var mealType = MealTypePicker.SelectedIndex + 1;
                foreach (var setting in _settings)
                {
                    if (setting.MealID == mealType)
                        setting.ListOfConstantFoods = string.Join(",", _ConstantFoodList);
                }
            }
            ConstantFoodEntry.Text = null;
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Warning", "All changes will be saved", "Ok", "Cancel");
            if (answer)
            {
                await _connection.UpdateAllAsync(_settings);
                await DisplayAlert("Info", "Settings Saved", "Ok");
            }
        }
    }
}