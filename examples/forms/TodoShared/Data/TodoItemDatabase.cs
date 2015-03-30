using Kidozen.Examples;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kidozen;

#if __IOS__
using Kidozen.iOS;
using Foundation;

#else
using Kidozen.Android;
using Android.Content;
#endif

namespace Todo
{
	public class TodoItemDatabase 
	{
		static object locker = new object ();

		#if __IOS__
			KidozenDelegate kidoDelegate;
		#endif

		KidoApplication kidozenApplication;


		ObjectSet database;

		public TodoItemDatabase()
		{

			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);

			#if __IOS__
				this.kidoDelegate = new KidozenDelegate();
				this.kidoDelegate.kzApplication = this.kidozenApplication;
			#endif

		}

		public Task<Boolean> Login() {
		#if __IOS__

			var launchOptions = new NSMutableDictionary();

			launchOptions.Add((NSString)"marketPlaceURL",(NSString)Settings.Marketplace);
			launchOptions.Add((NSString)"applicationKey", (NSString)Settings.Key);
			launchOptions.Add((NSString)"applicationName", (NSString)Settings.Application);
			launchOptions.Add((NSString)"username", (NSString)Settings.User);
			launchOptions.Add((NSString)"password", (NSString)Settings.Pass);
			launchOptions.Add((NSString)"provider", (NSString)Settings.Provider);

			var authTask = this.kidoDelegate.initializeKidozen(launchOptions);

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
                    return database.Query<TodoItem>(@"{}").Result;
                }
                catch (Exception w)
                {
                    
                    throw;
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

