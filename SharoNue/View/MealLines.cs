﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    class MealLines
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MealId { get; set; }
        public int MealTypeId { get; set; }
        public string FoodDesc { get; set; }
        public bool IsMet { get; set; }
    }
}
