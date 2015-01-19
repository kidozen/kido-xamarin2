using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;

using Newtonsoft;
using Newtonsoft.Json;

#if __ANDROID__
namespace Kidozen.Android.Offline
#else
namespace Kidozen.iOS.Offline
#endif
{
	public static partial class Offline
	{
		#if __ANDROID__
		static string targetdirectory = new AndroidOfflineCache().GetTargetDirectory();
		#else
		static string targetdirectory = new iOSOfflineCache().GetTargetDirectory();
		#endif


		static string DataSourceServiceName = "Datasource";

		public static Task<T> CachedQuery<T>(this DataSource datasource, OfflineCacheEnumeration type, TimeSpan? optionalTimeSpan = null ) {
			var query = datasource.Query();
			var manager = new OfflineCache (targetdirectory, datasource.dsname, DataSourceServiceName);
			if (optionalTimeSpan!=null) {
				manager.Expiration = optionalTimeSpan.Value;
			}
			var task = manager.TaskManager (query, type);

			return Task.Factory.StartNew<T>(()=> {
				var result = JsonConvert.DeserializeObject<T>(task.Result);
				return result;
			});		
		}

		public static Task<T> CachedQuery<T,U>(this DataSource datasource, U parameters, OfflineCacheEnumeration type, TimeSpan? optionalTimeSpan = null ) {
			var query = datasource.Query(parameters);
			var manager = new OfflineCache (targetdirectory, datasource.dsname, DataSourceServiceName, JsonConvert.SerializeObject(parameters));
			if (optionalTimeSpan!=null) {
				manager.Expiration = optionalTimeSpan.Value;
			}

			var task = manager.TaskManager (query, type);

			return Task.Factory.StartNew<T>(()=>{
				var result = JsonConvert.DeserializeObject<T>(task.Result);
				return result;
			});		
		}

		public static Task<string> CachedInvoke<U>(this DataSource datasource, U parameters,TimeSpan? optionalTimeSpan = null ) {
			var manager = new OfflineCache (targetdirectory, datasource.dsname,DataSourceServiceName);
			var invoke = datasource.Invoke (parameters);
			var task = manager.RequestQueueManager (invoke,parameters,OfflineCacheEnumeration.NetworkElseLocal);
			manager.RequestQueueManagerFetch+= (object sender, OfflineCache.RequestQueueManagerEventArgs e) => {
				e.Requests.ForEach( r => {
						Console.WriteLine("calling again invoke with parameters: " + r);
						Task.Factory.StartNew(()=>{
							var dsparam = JsonConvert.DeserializeObject<U>(r); 
							var result = datasource.Invoke(dsparam).Result;
							Console.WriteLine("calling again result: " + result);
						});
						manager.DeleteRequest(r);
					}
				);
			};
			return Task.Factory.StartNew<string>(()=>{
				return task.Result;
			});		

		}
	}
}
