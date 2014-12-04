using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.FSharp.Core;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using K = Kidozen;
using U = Utilities;
using A = Application;
using C = Crash;
using F = Files;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SQLite;
using System.Reflection;


#if __ANDROID__
using Kidozen.Android.Offline;

namespace Kidozen.Android.Offline.SQLite
#else
namespace Kidozen.iOS.Offline.SQLite
#endif
{
	public class KidoXAttribute :Attribute {

	}

	public class SyncEventArgs {
		public SyncEventArgs(){}
		public IList Updates {get;set;}
		public IList Inserts {get;set;}
	}


	public static partial class OfflineSQLite	{

		#if __ANDROID__
		static string targetdirectory = new AndroidOfflineCache().GetTargetDirectory();
		#else
		static string targetdirectory = new iOSOfflineCache().GetTargetDirectory();
		#endif
		public static Task<TableQuery<T>> SQLSync<T,U>(this DataSource.DataSource datasource, U parameters, string jpath, TimeSpan? optionalTimeSpan = null, EventHandler<SyncEventArgs> dbSyncComplete = null )  where T : new() 
		{
			var manager = new SQLiteCache<T> (targetdirectory, datasource.dsname, "", JsonConvert.SerializeObject(parameters));
			if (optionalTimeSpan!=null) {
				manager.Expiration = optionalTimeSpan.Value;
			}

			if (dbSyncComplete!=null) {
				manager.DbSyncComplete +=dbSyncComplete;
			}


			manager.SetupDataSource (datasource, JsonConvert.SerializeObject( parameters ));
			Task<string> query = null;
			//finds date in db
			var prop = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(p => p.GetCustomAttributes(typeof(KidoXAttribute), false).Count() ==1);
			var lastDate = manager.getMaxTimeStamp (prop.Name);
			if (!string.IsNullOrEmpty(lastDate)) {
				var paramsAsString = JsonConvert.SerializeObject (parameters);
				var updatedParameters = JsonConvert.DeserializeObject<JObject>(paramsAsString);
				updatedParameters.Add(prop.Name, lastDate);
				query = datasource.Query(updatedParameters);
			}

			else {
				query = datasource.Query(parameters);
			}

			return Task.Factory.StartNew<TableQuery<T>>(()=>{
				if (manager.TaskManager (query, jpath).Result) {
					return SQLiteCache<T>.Instance.GetTable();
				} 
				else {
					throw new Exception("There was an error");
				}
			});		
		}



		public static TableQuery<T> GetTable<T>(this DataSource.DataSource datasource) where T : new()
		{
			#pragma warning disable 0219
			var manager = new SQLiteCache<T> (targetdirectory, datasource.dsname, "");
			return SQLiteCache<T>.Instance.GetTable();
		}


		private static void save<T> (T data) {
			var conn = new SQLiteConnection (System.IO.Path.Combine (targetdirectory, "mydb.db"));
			conn.CreateTable<T>();
			conn.Insert (data);
		}
	}
}

