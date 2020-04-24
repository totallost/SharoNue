using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharoNue.Helper
{
    class HelperMethods
    {
        //public static DateTime WeekDays(int day, DateTime dt)
        //{
        //    //gives the date of the day of the week 
        //    int diff = ((dt.DayOfWeek - DayOfWeek.Sunday)) % 7;
        //    return dt.AddDays(-1 * diff + day).Date;
        //}

        public static DateTime WeekDays(int day, DateTime dt)
        {
            int diff = (-(dt.DayOfWeek - DayOfWeek.Sunday)+day+7)%7;
            return dt.AddDays(diff);
        }
    }
    public enum MealType
    {
        Breakfast = 1,
        Snack,
        Lunch,
        Dinner
    }
    
}
