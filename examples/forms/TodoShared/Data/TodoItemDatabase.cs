using Kidozen.Examples;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kidozen;
#if __IOS__
using Kidozen.iOS;
#else
using Kidozen.Android;
using Android.Content;
#endif

namespace Todo
{
	public class TodoItemDatabase 
	{
		static object locker = new object ();
		KidoApplication kidozenApplication;
		ObjectSet database;

		public TodoItemDatabase()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
		}

		public Task<Boolean> Login() {
			#if __IOS__
			var authTask = this.kidozenApplication.Authenticate ();
			#else
			var authTask = this.kidozenApplication.Authenticate (App.AndroidContext);
#endif
            return authTask.ContinueWith (
				t => {
					database = kidozenApplication.ObjectSet("todo");
					return !t.IsFaulted;
				}
			);
		}

		public IEnumerable<TodoItem> GetItemsNotDone ()
		{
			lock (locker) {
				return  database.Query<TodoItem>(@"{""Done"":false}").Result;
			}
		}

		public void DeleteItem(string id)
		{
			lock (locker) {
				var deleted = database.Delete (id).Result;
			}
		}

		public IEnumerable<TodoItem> GetItems ()
		{
			lock (locker) {
                try
                {
                    var results = kidozenApplication.DataSource("updateapprovalrequest").Invoke(new { RefId = "A" }).Result;
                    Console.WriteLine(results);
                    return null;
                }
                catch (Exception w)
                {
                    
                    Console.WriteLine(w.Message);

                    return null;
                }
			}
		}

		public void SaveItem (TodoItem item) 
		{
			lock (locker) {
				database.Save<TodoItem>(item).Wait(); //upsert
			}
		}
	}
}

