using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SharoNue.Droid.Persistance;
using SharoNue.Persistance;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteDb))]

namespace SharoNue.Droid.Persistance
{
	public class SQLiteDb : ISQLiteDb
	{
		public SQLiteAsyncConnection GetConnection()
		{
			var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var path = Path.Combine(documentsPath, "SharonNuedb.db3");

			return new SQLiteAsyncConnection(path);
		}
	}
}