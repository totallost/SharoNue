using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using SQLite;

namespace SharoNue.View
{
    [Table("Meal")]
    [Serializable]
    [DataContract]
    class Meal
    {
        [PrimaryKey, AutoIncrement]
        public int MealId { get; set; }
        public int MealDay { get; set; }
        public int MealType { get; set; }
        public DateTime MealDate { get; set; }

    }
}
