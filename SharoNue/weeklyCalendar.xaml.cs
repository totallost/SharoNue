using SharoNue.Helper;
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
        //private int DaysFromToday;
        public weeklyCalendar(int i)
        {
            InitializeComponent();
            //MainPage.DaysFromToday += i;
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

            _grid = CalenderCreator.CreateCalendar(CreateTapGesture(), DateTime.Now.AddDays(MainPage.DaysFromToday));

            Content = _grid;

        }

        protected override async void OnAppearing()
        {
            await CalenderCreator.PopulateLabels(_grid, DateTime.Now.AddDays(MainPage.DaysFromToday));
            base.OnAppearing();
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
            await Navigation.PushAsync(new MealDetails(day,int.Parse(mealId)));
        }
        private TapGestureRecognizer CreateTapGesture()
        {
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => OnLabelClicked(s, e);
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

        private async void AutoPopulate_Clicked(object sender, EventArgs e)
        {
            await TestMethods.populateDatabase(_grid, MainPage.DaysFromToday);
        }
    }
}