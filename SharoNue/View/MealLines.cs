using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    public class MealLines
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MealId { get; set; }
        public int MealTypeId { get; set; }
        public string FoodDesc { get; set; }
        [Ignore]
        public bool IsMet { get; set; }
    }
}
