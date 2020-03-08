using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    class Foods
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FoodDescription { get; set; }
        public int FoodType { get; set; }

    }
}
