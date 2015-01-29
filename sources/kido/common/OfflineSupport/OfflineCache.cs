using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using K = Kidozen;
using U = Utilities;
using A = KzApplication;
using C = Crash;

#if __ANDROID__
namespace Kidozen.Android.Offline
#else
namespace Kidozen.iOS.Offline
#endif
{
	public partial class OfflineCache
	{
		private string parametersAsHash;
		public string DirectoryPath { get; set;}
		public string Filename { get; set;}
		public string FilenamePath { get; set;}
		public string Service { get; set;}
		public string ServiceName { get; set;}

		public TimeSpan? Expiration { get; set;}

		public OfflineCache (string path, string name, string service, string parameters = "") {
			Service = service;
			ServiceName = name;
			DirectoryPath = Path.Combine(path, Service);
			parametersAsHash = (string.IsNullOrEmpty(parameters) ? parameters : createHash (parameters)) ;
			Filename = string.Format("{0}.{1}.json",name, parametersAsHash);
		}

		private string save (string content, string parameters = "") {
			if (!Directory.Exists(DirectoryPath)) {
				Directory.CreateDirectory (DirectoryPath);
			}
			if (Expiration==null) {
				Expiration = new TimeSpan (DateTime.Now.Ticks);
			}
			var expiration = DateTime.Now.AddTicks(Expiration.Value.Ticks).Ticks;
			var contentWithHeader = string.Format("{0}\t{1}", expiration, content);
			System.IO.File.WriteAllText(getFullFilePath(), contentWithHeader,UTF8Encoding.UTF8);
			return contentWithHeader;
		}

		public string get () {
			return System.IO.File.ReadAllText(getFullFilePath(),UTF8Encoding.UTF8);
		}

		private string getFullFilePath() {
			if (string.IsNullOrEmpty(FilenamePath)) {
				FilenamePath = Path.Combine (DirectoryPath, Filename);
			}
			return FilenamePath;
		}

		private Task<Tuple<bool, string> > getAsTask() {
			return Task.Factory.StartNew<Tuple<bool, string>>(()=>{

				var contents = get();

				bool expired = false;
				string body = contents;

				//Check expiration
				var headerIndex = contents.IndexOf("\t");
				if (headerIndex > -1) {
					var header = contents.Substring (0, headerIndex);
					var ticks = long.Parse(header);
					var expirationDate = new DateTime(ticks);
					if (DateTime.Compare(DateTime.Now,expirationDate) > 0) {
						expired = true;
						Console.WriteLine ("Expired, deleting from cache");
					}
					body = contents.Substring((headerIndex +1), contents.Length - (headerIndex + 1));
				}
				return new Tuple<bool,string>(expired,body);
			});		
		}

		private Task<string> saveAsTask(string contents) {
			return Task.Factory.StartNew(()=>{
				return save(contents);
			});		
		}

		public Task<string> TaskManager(Task<string> apitask, OfflineCacheEnumeration offlineType) {
			if (offlineType == OfflineCacheEnumeration.NetworkElseLocal) {
				Console.WriteLine ("OfflineCacheEnumeration.NetworkElseLocal");
				return apitask.ContinueWith (
					task => {
						if (task.IsFaulted) {
							Console.WriteLine ("Network fault, going to cache");
							var result = this.getAsTask ().Result; 
							if (result.Item1) { //expired
								Console.WriteLine ("Invalid, has expired");
								System.IO.File.Delete(getFullFilePath());
								throw new Exception("Cache data has expired.");
							}
							else {
								Console.WriteLine ("Valid! return data from cache");
								return result.Item2;
							}
						} else {
							Console.WriteLine ("Valid! return data from Network");
							this.save(task.Result);

							return task.Result;
						}
					}
				);

			} else if (offlineType == OfflineCacheEnumeration.LocalElseNetwork) {
				Console.WriteLine ("OfflineCacheEnumeration.LocalElseNetwork");
				return this.getAsTask ().ContinueWith (
					task => {
						if (task.IsFaulted) {
							Console.WriteLine ("Cache fault, going to network");
							this.save(apitask.Result);
							return apitask.Result;
						}
						else {
							var result = task.Result;
							if (result.Item1) { //expired
								System.IO.File.Delete(getFullFilePath());
								this.save(apitask.Result);
								Console.WriteLine ("Invalid, has expired");
								Console.WriteLine ("Going to network");
								return apitask.Result;
							} else {
								return result.Item2;
							}
						}
					}
				);

			} else if (offlineType == OfflineCacheEnumeration.NetworkOnly) {
				return apitask.ContinueWith (
					task => {
						if (task.IsFaulted) {
							throw task.Exception;
						} else {
							this.save(task.Result); // updates local cache
							return task;
						}
					}
				).Result;
			} else
				return apitask;
		}


		// Create an md5 sum string of this string
		static public string createHash(string str)
		{
			// First we need to convert the string into bytes, which
			// means using a text encoder.
			var enc = Encoding.Unicode.GetEncoder();

			// Create a buffer large enough to hold the string
			var unicodeText = new byte[str.Length * 2];
			enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

			// Now that we have a byte array we can ask the CSP to hash it
			var md5 = new MD5CryptoServiceProvider();
			var result = md5.ComputeHash(unicodeText);

			// Build the final string by converting each byte
			// into hex and appending it to a StringBuilder
			var sb = new StringBuilder();
			for (int i=0;i<result.Length;i++)
			{
				sb.Append(result[i].ToString("X2"));
			}

			// And return it
			return sb.ToString();
		}

		public class RequestQueueManagerEventArgs {public List<string> Requests {get;set;}}
		public event EventHandler<RequestQueueManagerEventArgs> RequestQueueManagerFetch;

		private Task<string> queueRequest(string content) {
			return Task.Factory.StartNew<string>(()=> {
				if (!Directory.Exists(DirectoryPath)) {
					Directory.CreateDirectory (DirectoryPath);
				}
				var requestDirectoryPath = Path.Combine(this.DirectoryPath,this.ServiceName);
				if (!Directory.Exists(requestDirectoryPath)) {
					Directory.CreateDirectory (requestDirectoryPath);
				}

				var requestFileName = string.Format("{0}.request",Guid.NewGuid().ToString());

				System.IO.File.WriteAllText(Path.Combine(requestDirectoryPath,requestFileName), content,UTF8Encoding.UTF8);

				return requestFileName;
			});		
		}

		private Task processPending() {
			return Task.Factory.StartNew(()=> {
				var requestDirectoryPath = Path.Combine(this.DirectoryPath,this.ServiceName);
				var requests = Directory.EnumerateFiles(requestDirectoryPath,"*.request").ToList().Select(f=> {
					var fullfilename = Path.Combine(requestDirectoryPath,f);
					return System.IO.File.ReadAllText(fullfilename,UTF8Encoding.UTF8);
				});
				if (RequestQueueManagerFetch!=null) {
					RequestQueueManagerFetch(this,new RequestQueueManagerEventArgs { Requests = requests.ToList() });
				}
			});		
		}

		public void DeleteRequest( string request) {
			Console.WriteLine ("now delete this request: " + request);
		}

		public Task<string> RequestQueueManager<T>(Task<string> apitask, T parameters, OfflineCacheEnumeration offlineType) {
			if (offlineType == OfflineCacheEnumeration.NetworkElseLocal) {
				return apitask.ContinueWith (
					task => {
						if (task.IsFaulted) {
							var request = JsonConvert.SerializeObject(parameters);

							return this.queueRequest(request);
						} else {
							Task.Factory.StartNew(()=>{
								processPending();
							});
							return task;
						}
					}
				).Result;

			} else
				throw new NotImplementedException ();
		}
	}
}
