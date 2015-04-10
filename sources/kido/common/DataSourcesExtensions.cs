﻿using System.IO;
using System.Threading.Tasks;
using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif
{
	public static partial class DataSourcesExtensions
	{
		public static Task<Stream> Query(this DataSource ds) {
			var task = ds.QueryFile();
			return Task.Factory.StartNew<Stream>(()=> new MemoryStream(task.Result));		
		}

		public static Task<Stream> Invoke(this DataSource ds) {
			var task = ds.InvokeFile();
			return Task.Factory.StartNew<Stream>(()=> new MemoryStream(task.Result));		
		}

		public static Task<Stream> Query<T>(this DataSource ds, T parameters) {
			var task = ds.QueryFile(parameters);
			return Task.Factory.StartNew<Stream>(()=> new MemoryStream(task.Result));		
		}

		public static Task<Stream> Invoke<T>(this DataSource ds, T parameters) {
			var task = ds.InvokeFile(parameters);
			return Task.Factory.StartNew<Stream>(()=> new MemoryStream(task.Result));		
		}
	}
}
