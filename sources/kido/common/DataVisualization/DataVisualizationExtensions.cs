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
using A = KzApplication;
using C = Crash;

#if __ANDROID__
using Android.Content;
using Android.OS;
#elif __IOS__
#endif

using Newtonsoft.Json;
using Newtonsoft;

#if __ANDROID__
namespace Kidozen.Android
#else
namespace Kidozen.iOS
#endif

{

	public static partial class KidozenExtensions	{
		static IDataVisualization datavisualization;

		#if __ANDROID__
		public static void ShowDataVisualization(this Kidozen.KidoApplication app, Context context, String visualization) {
			datavisualization = new AndroidDataVisualization { Context = context };
			ShowVisualization (app, datavisualization, visualization);
		}
		#else
		public static void ShowDataVisualization(this Kidozen.KidoApplication app, String visualization) {
			datavisualization = new iOSDataVisualization ();
			ShowVisualization (app, datavisualization, visualization);
		}
		#endif


		private static void ShowVisualization(this Kidozen.KidoApplication app, IDataVisualization datavisualization, String visualization) {
			var appConfig = A.getAppConfig (A.createConfigUrl (app.marketplace, app.application));
			if (appConfig.IsConfiguration) {
				var curentConfiguration = (appConfig as A.GetConfigurationResult.Configuration).Item;
				var baseurl = U.getJsonObjectValue (curentConfiguration , "url");
				if (baseurl.Value != null) {
					String url = String.Format("{0}api/v2/visualizations/{1}/app/download?type=mobile",baseurl.Value.Trim( "\"".ToCharArray()), visualization);
					var files = new Files (app.GetIdentity);
					Task.Factory.StartNew(()=> {
						var bytes = files.DownloadFromUrl(url).Result.Value;
						var targetDir = datavisualization.GetTargetDirectory();
						var filename = string.Format("{0}{1}", visualization, ".zip");
						System.IO.File.WriteAllBytes(Path.Combine (targetDir, filename),bytes);
						if (datavisualization.UnzipFiles(targetDir,visualization)) {
							replacePlaceholders(app,visualization);
							datavisualization.LaunchView(visualization, targetDir);
						}
					});	
				} 
				else {
					throw new Exception ("Invalid configuration settings. Please check username and password");
				}
			}
		}

		private static void replacePlaceholders( Kidozen.KidoApplication app, String dataVizName ) {
			var indexString = System.IO.File.ReadAllText( indexFilePath(dataVizName));
			var options = optionsString (app);
			indexString = indexString.Replace("{{:options}}", options);
			indexString = indexString.Replace("{{:marketplace}}", "\""+ app.marketplace +"\"");
			indexString = indexString.Replace("{{:name}}", "\"" + app.application + "\"");
			System.IO.File.WriteAllText( indexFilePath(dataVizName),indexString);
		}

		private static String optionsString(Kidozen.KidoApplication app) {
			if (app.GetIdentity.authenticationRequest.ProviderRequest!=null) {
				var token = new DsProviderJsSDKBridge (app.GetIdentity);
				return JsonConvert.SerializeObject(token);
			} else {
				var token = new DsPassiveJsSDKBridge (app.GetIdentity.token.Value);
				return JsonConvert.SerializeObject(token);
			}
		}
		private static String indexFilePath(String dataVizName) {
			return datavisualization.GetDestinationDirectory(dataVizName) + "/index.html";
		}

		[JsonObject("token")]
		public class DvTokenJsSDKBridge {
			[JsonProperty("refresh_token")]
			public String RefreshToken { get; set;}
			[JsonProperty("rawToken")]
			public String RawToken { get; set;}

			public DvTokenJsSDKBridge(Utilities.Token t) {
				this.RefreshToken = t.refresh.Value;
				this.RawToken = t.raw.Value;
			}
		}

		public class DsPassiveJsSDKBridge {
			[JsonProperty("token")]
			public DvTokenJsSDKBridge Token { get; set;}
			public DsPassiveJsSDKBridge(Utilities.Token t) {
				this.Token = new DvTokenJsSDKBridge(t);
			}
		}

		public class DsProviderJsSDKBridge {
			[JsonProperty("token")]
			public DvTokenJsSDKBridge Token { get; set;}
			[JsonProperty("username")]
			public String Username { get; set;}
			[JsonProperty("provider")]
			public String Provider { get; set;}
			[JsonProperty("password")]
			public String Password { get; set;}

			public DsProviderJsSDKBridge(KzApplication.Identity id) {
				var authrequest = id.authenticationRequest.ProviderRequest.Value;
				this.Token = new DvTokenJsSDKBridge( id.token.Value );
				this.Username = authrequest.User;
				this.Provider = authrequest.Key;
				this.Username = authrequest.Password;
			}
		}
	}
}

