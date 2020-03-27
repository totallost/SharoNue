using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        //from 0 to 6
        public int IdDay { get; set; }
        //from 0 to 3
        public int MealID { get; set; }
        //true or false
        public bool IsAutoPopulate { get; set; }
        //list of constant food for a meal 
        public string ListOfConstantFoods { get; set; }
        //List of food types that are going to be generated automatically per meal 
        public string ListOfFoodTypes { get; set; }
    }
}
