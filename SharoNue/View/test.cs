using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharoNue.View
{
    class test
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
