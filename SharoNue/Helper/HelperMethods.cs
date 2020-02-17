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
        public static DateTime WeekDays(int day, DateTime dt)
        {
            //gives the date of the day of the week 
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return dt.AddDays(-1 * diff + day).Date;
        }
    }
    
}
