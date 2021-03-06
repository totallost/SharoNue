﻿using SharoNue.Helper;
using SharoNue.Persistance;
using SharoNue.Test;
using SharoNue.View;
using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SharoNue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class weeklyCalendar : ContentPage
    {
        private SQLiteAsyncConnection _connection;
        private Grid _grid;
        public weeklyCalendar(int i)
        {
            InitializeComponent();
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

            _grid = CalenderCreator.CreateCalendar(CreateTapGesture(), CreteTapGestureTop(), DateTime.Now.AddDays(MainPage.DaysFromToday));

            Content = _grid;


        }
        protected override async void OnAppearing()
        {
            await CalenderCreator.PopulateLabels(_grid, DateTime.Now.AddDays(MainPage.DaysFromToday));
            CreateSwipeGesture();
            base.OnAppearing();
        }
        private void CreateSwipeGesture()
        {
            var leftSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            leftSwipeGesture.Swiped += OnSwiped;
            var rightSwipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            rightSwipeGesture.Swiped += OnSwiped;
            _grid.GestureRecognizers.Add(leftSwipeGesture);
            _grid.GestureRecognizers.Add(rightSwipeGesture);
        }
        public async void OnLabelClicked(object s, EventArgs e)
        {
            Label label = (Label)s;

            var day = int.Parse(label.ClassId[1].ToString());
            var mealtype = int.Parse(label.ClassId[0].ToString());

            string mealId ="";
            for (int i=2; label.ClassId.Length-1>=i; i++)
            {
                mealId = mealId + label.ClassId[i].ToString();

            }

            if (mealId == "")
            {
                int mealIdFromDb = await CreateNewMeal(label);
                label.ClassId = label.ClassId + mealIdFromDb.ToString();
                mealId = mealIdFromDb.ToString();
            }
            await Navigation.PushAsync(new MealDetails(day,int.Parse(mealId), mealtype));
        }
        private TapGestureRecognizer CreateTapGesture()
        {
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => OnLabelClicked(s, e);
            return tgr;
        }
        private TapGestureRecognizer CreteTapGestureTop()
        {
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => TopLabelsClick(s, e);
            return tgr;
        }
        private async Task<int> CreateNewMeal(Label currentLabel)
        {
            int mealID = int.Parse(currentLabel.ClassId) / 10;
            int dayId = int.Parse(currentLabel.ClassId) % 10;
            Meal meal = new Meal
            {
                    MealDay = dayId,
                    MealType = mealID,
                    MealDate = HelperMethods.WeekDays(dayId, DateTime.Now.AddDays(MainPage.DaysFromToday)),
            };
            int mealIdFromDb = 0;
            if (meal != null)
            {
                await _connection.InsertAsync(meal);
                mealIdFromDb = meal.MealId;
            }
            return mealIdFromDb;
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            ToolbarItem toolBarItem = (ToolbarItem)sender;
            if (toolBarItem.Text == "Next")
            {
                MainPage.DaysFromToday += 7;
                if (MainPage.DaysFromToday <= 0)
                {
                    await Navigation.PopAsync(true);
                }
                else
                {
                    await Navigation.PushAsync(new weeklyCalendar(MainPage.DaysFromToday));
                }
            }
            else
            {
                MainPage.DaysFromToday -= 7;
                if (MainPage.DaysFromToday >= 0)
                {
                    await Navigation.PopAsync(true);
                }
                else
                {
                    await Navigation.PushAsync(new weeklyCalendar(MainPage.DaysFromToday));
                }
            }
                
        }
        private async void More_Clicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("What do you want to do?", "Cancel", null, "Auto Fill", "Reset week");
            switch (action)
            {
                case "Auto Fill":
                    var autoFill = new AutoFill();
                    await autoFill.populateDatabase(_grid, MainPage.DaysFromToday);
                    await CalenderCreator.PopulateLabels(_grid, DateTime.Now.AddDays(MainPage.DaysFromToday));
                    break;
                case "Reset week":
                    await CalenderCreator.ResetWeek(_grid, DateTime.Now.AddDays(MainPage.DaysFromToday));
                    _grid = null;
                    _grid = CalenderCreator.CreateCalendar(CreateTapGesture(), CreteTapGestureTop(), DateTime.Now.AddDays(MainPage.DaysFromToday));
                    await CalenderCreator.PopulateLabels(_grid, DateTime.Now.AddDays(MainPage.DaysFromToday));
                    break;
            }
        }
        private void OnSwiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Right:
                    ToolbarItem_Clicked(new ToolbarItem { Text = "Previous" }, new EventArgs());
                    break;
                case SwipeDirection.Left:
                    ToolbarItem_Clicked(new ToolbarItem { Text = "Next" }, new EventArgs());
                    break;
                case SwipeDirection.Up:
                    // Handle the swipe
                    break;
                case SwipeDirection.Down:
                    // Handle the swipe
                    break;
            }
        }
        private async void TopLabelsClick(object s, EventArgs e)
        {
            await Navigation.PushAsync(new DaysSettings());
        }
    }
}