using SharoNue.Helper;
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
    public partial class ChooseMealCalendar : ContentPage
    {
        private List<MealLines> _mealLines;
        private SQLiteAsyncConnection _connection;
        private Grid _grid;
        public ChooseMealCalendar(List<MealLines> mealLines)
        {
            InitializeComponent();

            _mealLines = mealLines;

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

            _grid = CalenderCreator.CreateCalendar(CreateTapGesture(), CreateTapGestureTop(), DateTime.Now);

            Content = _grid;
        }
        protected override async void OnAppearing()
        {
            await CalenderCreator.PopulateLabels(_grid, DateTime.Now);
            base.OnAppearing();
        }
        private TapGestureRecognizer CreateTapGestureTop()
        {
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => TopLabelsClick(s, e);
            return tgr;
        }
        private TapGestureRecognizer CreateTapGesture()
        {
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => OnLabelClicked(s, e);
            return tgr;
        }
        private async void TopLabelsClick(object s, EventArgs e)
        {
            await Navigation.PushAsync(new DaysSettings());
        }
        public async void OnLabelClicked(object s, EventArgs e)
        {
            Label label = (Label)s;

            var day = int.Parse(label.ClassId[1].ToString());
            var mealtype = int.Parse(label.ClassId[0].ToString());

            string mealId = "";
            for (int i = 2; label.ClassId.Length - 1 >= i; i++)
            {
                mealId = mealId + label.ClassId[i].ToString();

            }

            if (mealId == "")
            {
                int mealIdFromDb = await CreateNewMeal(label);
                label.ClassId = label.ClassId + mealIdFromDb.ToString();
                mealId = mealIdFromDb.ToString();
            }
            else
            {
                foreach (var line in _mealLines)
                {
                    line.Id = 0;
                    line.MealId = int.Parse(mealId);
                    line.MealTypeId = mealtype;
                    line.IsMet = false;
                }
                await _connection.InsertAllAsync(_mealLines);
            }
            await DisplayAlert("Copied", "Meal was copied", "OK");
            await Navigation.PopAsync();
        }
        private async Task<int> CreateNewMeal(Label currentLabel)
        {
            int mealID = int.Parse(currentLabel.ClassId) / 10;
            int dayId = int.Parse(currentLabel.ClassId) % 10;
            Meal meal = new Meal
            {
                MealDay = dayId,
                MealType = mealID,
                MealDate = HelperMethods.WeekDays(dayId, DateTime.Now),
            };
            int mealIdFromDb = 0;
            if (meal != null)
            {
                await _connection.InsertAsync(meal);
                mealIdFromDb = meal.MealId;
            }
            foreach(var line in _mealLines)
            {
                line.Id = 0;
                line.MealId = mealIdFromDb;
                line.MealTypeId = mealID;
                line.IsMet = false;
            }
            await _connection.InsertAllAsync(_mealLines);
            return mealIdFromDb;
        }
    }
}