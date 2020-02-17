using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    class FoodTypes
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string FoodTypeDescription { get; set; }
    }
}
