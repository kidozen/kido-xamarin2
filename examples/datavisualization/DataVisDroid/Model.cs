using System;
using System.Threading;
using System.Threading.Tasks;

using Android;
using Android.Content;
using Android.Util;

using Kidozen;
using Kidozen.Android;

namespace DataVisDroid
{
	public class Model
	{
		KidoApplication kido;
		String tag = "Model";

		public Model ()
		{
			kido = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
		}
		/// <summary>
		/// Authenticates against Kidozen. 
		/// returns true is success
		/// </summary>
		public Task<bool> Authenticate() {
			return kido.Authenticate (Settings.User, Settings.Pass, Settings.Provider)
				.ContinueWith(t=> { return !t.IsFaulted;});
		}


		/// <summary>
		/// The current version of ShowDataVisualization is synchronous, so this method wraps the call in a task
		/// </summary>
		/// <returns>The data visualization.</returns>
		/// <param name="context">Android application caller Context.</param>
		/// <param name="name">Name of the data visualization to invoke.</param>
		public Task<bool> DisplayDataVisualization (Context context, string name) {
			return Task.Factory.StartNew( () => {
				try {
					kido.ShowDataVisualization(context,name);
					return true;
				} catch (Exception ex) {
					return false;					
				}
			});
		}

		/// <summary>
		/// Receives any Console message event fired by our internal implementation of WebChromeClient Android class. 
		/// </summary>
		public class MyBroadcastReceiver : BroadcastReceiver {
			String tag = string.Empty;
			public override void OnReceive(Context context, Intent intent) {
				var message = intent.GetStringExtra(DataVisualizationActivityConstants.DATA_VISUALIZATION_BROADCAST_CONSOLE_MESSAGE);
			}
		}
	}
}

