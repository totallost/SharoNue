using SharoNue.Persistance;
using SharoNue.View;
using SQLite;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SharoNue.Helper
{
    class DefaultSettings
    {
        public static async void SetDefaultSettings()
        {
            var _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            List<Settings> settings = new List<Settings>();

            for(int i=0; i<7; i++)
            {
                for(int j=1; j<5; j++)
                {
                    settings.Add(new Settings
                    {
                        IdDay = i,
                        IsAutoPopulate = true,
                        MealID = j
                    });
                };
            };
            await _connection.InsertAllAsync(settings);
        }
    }
}
