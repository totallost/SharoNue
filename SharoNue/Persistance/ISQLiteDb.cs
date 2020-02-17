using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace SharoNue.Persistance
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetConnection();
    }
}
