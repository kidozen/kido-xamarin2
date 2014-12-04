using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.FSharp.Core;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;
using System.Text;


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
namespace Kidozen.Android.Offline.SQLite
#else
namespace Kidozen.iOS.Offline.SQLite
#endif
{
	public partial class SQLiteCache<T> where T : new()
	{
		static public System.Timers.Timer syncTimer;
		static public List<T> updatedRecords = new List<T>();
		static public List<T> newRecords = new List<T>();

		public string parametersAsHash = string.Empty;
		public string DirectoryPath { get; set;}
		public string Filename { get; set;}
		public string FilenamePath { get; set;}
		public string Service { get; set;}
		public string ServiceName { get; set;}


		object lockTimer = new object();
		object lockUpserts = new object();

		public string AttributeName { get; set;}
		public string jPath { get; set;}

		DataSource.DataSource baseDataSource ;
		string baseParameters;

		JsonSerializerSettings noneDatePaser = new Newtonsoft.Json.JsonSerializerSettings() { 
			DateParseHandling = Newtonsoft.Json.DateParseHandling.None 
		};

		void setupTimer (TimeSpan? value)
		{
			syncTimer = new System.Timers.Timer ();
			syncTimer.Elapsed += HandleElapsed;
			syncTimer.Interval = ( value.HasValue ? value.Value.TotalMilliseconds : 1000 * 60 * 60 * 5);
		}

		public TimeSpan? Expiration { 
			set {
				setupTimer (value);
			}
		}

		public void SetupDataSource (DataSource.DataSource ds, string parameters) {
			this.baseDataSource = ds;
			this.baseParameters = parameters;
		}

		void HandleElapsed (object sender, System.Timers.ElapsedEventArgs e)
		{
			if (Monitor.TryEnter (lockTimer)) {
				try {
					syncTimer.Stop();
					Task.Factory.StartNew(() => {
						var updatedParameters = JsonConvert.DeserializeObject<JObject>(baseParameters);
						var ts = getMaxTimeStamp(AttributeName);
						if (!string.IsNullOrEmpty(ts)) {
							updatedParameters.Add(AttributeName, ts);
						}
						//
						var ds = new DataSource.DataSource(baseDataSource.dsname,
							baseDataSource.identity);
						//
						var query = ds.Query(updatedParameters);
						var rawresult = JsonConvert.DeserializeObject<JObject>(query.Result,noneDatePaser);
						var kidoJPath = string.Format("data{0}", jPath);

						if (rawresult.SelectToken("data[0]")!=null) {
							resetRecords();

							var dataContents = rawresult.SelectToken(kidoJPath).ToString();
							saveEnumerable (dataContents);
						}
						syncTimer.Start();
						});

				} finally {
					Monitor.Exit (lockTimer);
				}	
			}

			Console.WriteLine ("timer elapsed, volver a ejecutar el DS");
		}

		private static SQLiteCache<T> instance;

		public static SQLiteCache<T> Instance {
			get	{ 
				return instance;
			}
		}

		public SQLiteConnection GetDatabase() {
			return new SQLiteConnection (getFullFilePath());
		}

		public TableQuery<T> GetTable() {
			var cnn =new SQLiteConnection (getFullFilePath ());
			return cnn.Table<T>();
		}

		public SQLiteCache (string path, string name, string service, string parameters = "") {
			Service = service;
			ServiceName = name;
			DirectoryPath = System.IO.Path.Combine(path, Service);
			this.parametersAsHash = parameters;
			Filename = string.Format("offlinesql.db");
			instance = this;
		}

		private object getPropertyValue(object src, string propName) {
			return src.GetType().GetProperty(propName).GetValue(src, null);
		}



		public string getMaxTimeStamp(string attributeName) {
			AttributeName = attributeName;
			var conn = new SQLiteConnection (getFullFilePath());
			try {
				var rows = conn.Query<T>("SELECT " + attributeName + " FROM CMS_News ORDER BY " + attributeName + " DESC " );
				var date = getPropertyValue(rows[0], attributeName);
				return date.ToString();
			} catch (SQLiteException ex) {
				Console.WriteLine (ex.Message);
				return null;
			} catch (Exception e) {
				Console.WriteLine (e.Message);
				return null;
			}
		}

		//TODO: support datasources parameters
		private void save (T content, string parameters = "") {
			var conn = new SQLiteConnection (getFullFilePath());
			try {
				conn.CreateTable<T>();
				conn.Insert (content);
				Console.WriteLine("Adding new object to SQLite");
				newRecords.Add(content);
			} catch (SQLiteException ex) {
				if (ex.Message.ToLower().Contains("constraint")) {
					conn.Update (content);
					Console.WriteLine ("Conflict trying to update");
					updatedRecords.Add (content);
				}
			} catch (Exception e) {
				Console.WriteLine ("********* UNKNOWN ERROR=>" + e.Message);
				throw e;
			}
		}
		private string getFullFilePath() {
			if (string.IsNullOrEmpty(FilenamePath)) {
				FilenamePath = System.IO.Path.Combine (DirectoryPath, Filename);
			}
			return FilenamePath;
		}
			
		void saveEnumerable (string contents)
		{
			var result = JsonConvert.DeserializeObject<List<T>> (contents, new Newtonsoft.Json.Converters.IsoDateTimeConverter());
			result.ForEach (r => this.save (r));
			dispatchSyncComplete ();
		}

		void resetRecords ()
		{
			if (Monitor.TryEnter(lockUpserts)) {
				try {
					updatedRecords = new List<T> ();
					newRecords = new List<T> ();
				} finally {
					Monitor.Exit(lockUpserts);
				}
			}
		}

		public Task<Boolean> TaskManager(Task<string> apitask, string jpath) {
			jPath = jpath;
			return apitask.ContinueWith 
			(
				dstask => {
					if (dstask.IsFaulted) {
						return false;
					} else {
							var r = apitask.Result;
							var rawresult = JsonConvert.DeserializeObject<JObject>(r,noneDatePaser);
							var kidoJPath = string.Format("data{0}", jpath);

							if (rawresult.SelectToken("data[0]")!=null) {
								resetRecords ();

								var dataContents = rawresult.SelectToken(kidoJPath).ToString();
								saveEnumerable (dataContents);
							}
							syncTimer.Start();

						return true;
					}
				}
			);
		}


		public class DbSyncEventArgs {
			public List<T> Updates {get;set;}
			public List<T> Inserts {get;set;}
		}

		public event EventHandler<SyncEventArgs> DbSyncComplete;

		private void dispatchSyncComplete() {
			if (DbSyncComplete!=null && (updatedRecords.Count>0 || newRecords.Count>0)) {
				DbSyncComplete(this,new SyncEventArgs { Updates = updatedRecords, Inserts = newRecords });
			}
		}

		public void DeleteRequest( string request) {
			Console.WriteLine ("now delete this request: " + request);
		}


	}
}
