using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Newtonsoft.Json.Linq;
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
		Storage.Storage database;

		public TodoItemDatabase()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
		}

		public Task<Boolean> Login() {
			#if __ANDROID__
			var authTask = this.kidozenApplication.Authenticate (App.AndroidContext);
			#else
			var authTask = this.kidozenApplication.Authenticate ();
			#endif

			return authTask.ContinueWith (
				t => {
					database = kidozenApplication.Storage ("todo");
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
				return  database.Query<TodoItem> (@"{}").Result;
			}
		}

		public void SaveItem (TodoItem item) 
		{
			lock (locker) {
				database.Save<TodoItem>(item).Wait(); //upsert
			}
		}

		// DataSource.DataSource queryDataSource, saveDataSource;


		// queryDataSource = kidozenApplication.DataSource["QueryTodo"];
		// saveDataSource = kidozenApplication.DataSource["AddTodo"];


		// ******************************
		// *** DataSource sample code ***
		// ******************************
		/*
		public IEnumerable<TodoItem> GetItems ()
		{
			lock (locker) {
				var results = queryDataSource.Query().Result.Data;
				return createTodoItemList (results);
			}
		}

		//Ensure that your DataSource can execute an UPSERT
		public void SaveItem (TodoItem item) 
		{
			lock (locker) {
				var result = saveDataSource.Invoke(item).Result;
			}
		}

		IEnumerable<TodoItem> createTodoItemList (JObject results)
		{
			var result = JArray.Parse (results.SelectToken("data").ToString());
			return result.Select ( todo => new TodoItem {
				Name = todo.Value<string>("Name"),
				Notes = todo.Value<string>("Notes") ,
				_id = todo.Value<string>("_id") ,
			}
			).ToList();
		}
		*/
	}
}

