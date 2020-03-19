﻿using SharoNue.Helper;
using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace SharoNue
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        static public int DaysFromToday=0;
        private SQLiteAsyncConnection _connection;
        public MainPage()
        {
            InitializeComponent();
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
        }

        protected override async void OnAppearing()
        {
            await _connection.CreateTableAsync<Meal>();
            await _connection.CreateTableAsync<Foods>();
            await _connection.CreateTableAsync<MealLines>();
            await _connection.CreateTableAsync<MealTypes>();
            await _connection.CreateTableAsync<FoodTypes>();
            await _connection.CreateTableAsync<test>();
            await _connection.CreateTableAsync<Settings>();

            base.OnAppearing();
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            DaysFromToday = 0;
            await Navigation.PushAsync(new weeklyCalendar(0));
        }

        async private void Button_Clicked_1(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FoodMenu());
        }
    }
}
