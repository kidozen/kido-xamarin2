
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Java.Interop;
using Android.Webkit;
using Android.Net.Http;

namespace Kidozen.Android
{
	public class AuthenticationResponseEventArgs : EventArgs {
		public Boolean Success { get; set;}
		public String ErrorMessage { get; set;}
		public Dictionary<string,string> TokenInfo { get; set;}
	}

	public delegate void AuthenticationResponse(object sender, AuthenticationResponseEventArgs e);

	public class AuthenticationJavaScriptInterface : Java.Lang.Object {
		private Context context;

		public event AuthenticationResponse AuthenticationResponseArrived;

		public AuthenticationJavaScriptInterface(Context context ) {
			this.context = context;
		}

		protected virtual void OnAuthenticationResponseArrived(AuthenticationResponseEventArgs e) 
		{
			if (AuthenticationResponseArrived != null)
				AuthenticationResponseArrived(this, e);
		}

		[Export("getTitleCallback")]
		[JavascriptInterface]
		public void getTitleCallback(string payload) {
			if (payload.Contains ("Success payload=")) {
				payload = Encoding.UTF8.GetString (Convert.FromBase64String (payload.Replace ("Success payload=", String.Empty)));
				var rawToken = JsonConvert.DeserializeObject<Dictionary<string, string>> (payload);
				OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs {
					Success = true,
					TokenInfo = rawToken
				});
			}
			else
				if (payload.Contains ("Error message=")) {
					OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs {
						Success = false,
						ErrorMessage = payload.Replace ("Error message=", String.Empty)
					});
				}
		}
	}

	public class AuthenticationWebViewClient : WebViewClient {
		private String getTitleJsFunction = "javascript:( function () { window.HTMLOUT.getTitleCallback(document.title); } ) ()";

		public override void OnReceivedError (WebView view, ClientError errorCode, string description, string failingUrl)
		{
			base.OnReceivedError (view, errorCode, description, failingUrl);
		}

		public override void OnReceivedSslError (WebView view, SslErrorHandler handler, SslError error)
		{

			handler.Proceed ();
		}

		public override void OnPageFinished (WebView view, string url)
		{
			if (view.Title.StartsWith("Success payload=")) {
				view.LoadUrl(getTitleJsFunction);
			}  else if (view.Title.StartsWith("Error message=")) {
				//TODO: Still not implemented on services side
			}
		}

	}

	[Activity (Label = "PassiveAuthActivity")]			
	public class PassiveAuthActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			this.RequestWindowFeature (WindowFeatures.NoTitle);
			var endpoint = this.Intent.GetStringExtra ("signInUrl");

			var mainLayout = new LinearLayout (this);
			mainLayout.SetPadding (0, 0, 0, 0);
			var jsInterface = new AuthenticationJavaScriptInterface (this);
			jsInterface.AuthenticationResponseArrived+= HandleAuthenticationResponseArrived;

			var webClient = new AuthenticationWebViewClient ();
			var webView = new WebView (this);
			webView.VerticalScrollBarEnabled = false;
			webView.HorizontalScrollBarEnabled = false;
			webView.SetWebViewClient (webClient);
			webView.Settings.JavaScriptEnabled = true;
			webView.LayoutParameters = new FrameLayout.LayoutParams (ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
			webView.Settings.SaveFormData = false;
			webView.LoadUrl (endpoint);
			webView.AddJavascriptInterface (jsInterface, "HTMLOUT");
			mainLayout.AddView (webView);
			this.SetContentView(mainLayout,new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,ViewGroup.LayoutParams.MatchParent));;
		}

		void HandleAuthenticationResponseArrived (object sender, AuthenticationResponseEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ("PassiveAuthActivity: " + e.Success.ToString ());
			KidozenExtensions.HandleAuthenticationResponseArrived (e);
			this.Finish ();
		}
	}
}

