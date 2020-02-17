using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    class MealTypes
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string MealTypeDescription { get; set; }
    }
}
