
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
using System.IO;

using Java.IO;
using Java.Util.Zip;
using System.Threading.Tasks;
using System.Threading;

using A = KzApplication;
using C = Crash;

using Android.Graphics;
using Android.Net.Http;

namespace Kidozen.Android
{
	[Activity (Label = "DataVisualizationActivity")]			
	public class DataVisualizationActivity : Activity
	{
		private WebView webView;
		private String visualization, destinationdir;
		//private static ProgressDialog progressDialog;

		JSValidationWebChromeClient jswc = new JSValidationWebChromeClient(); 

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			/*progressDialog = new ProgressDialog(this);
			progressDialog.SetMessage("Downloading visualizations");
			progressDialog.SetCancelable(false);
			progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
			progressDialog.Progress= 0; 
			progressDialog.Show();
			*/
			if (this.Intent.Extras != null) {
				jswc.ConsoleEvent += HandleConsoleEventHandler;;

				this.visualization = this.Intent.Extras.GetString("visualization") ;
				this.destinationdir = this.Intent.Extras.GetString("destinationdir");

				this.addWebView(this);
				var path = String.Format ("file://{0}", System.IO.Path.Combine (destinationdir, visualization) + "/index.html");
				webView.LoadUrl (path );
			}
		}

		protected override void OnDestroy ()
		{
			this.removeTempFiles();
			base.OnDestroy ();
		}
	
		private void removeTempFiles() {
			var d= new Java.IO.File(destinationdir);
			deleteRecursive(d);

			var zipFile = new Java.IO.File(visualization);
			zipFile.Delete();
		}

		private void deleteRecursive(Java.IO.File fileOrDirectory) {
			if (fileOrDirectory.IsDirectory)
				fileOrDirectory.ListFiles ().ToList ().ForEach (f => deleteRecursive (f));

			fileOrDirectory.Delete();
		}

		void HandleConsoleEventHandler (object sender, OnConsoleEventArgs e)
		{
			Intent broadcastIntent = new Intent();
			broadcastIntent.SetAction(DataVisualizationActivityConstants.DATA_VISUALIZATION_BROADCAST_ACTION);
			broadcastIntent.AddCategory(Intent.CategoryDefault);
			broadcastIntent.PutExtra(DataVisualizationActivityConstants.DATA_VISUALIZATION_BROADCAST_CONSOLE_MESSAGE, e.Message);
			SendBroadcast(broadcastIntent);
		}

		public class OnConsoleEventArgs {
			public String Message {get; private set;} 
			public OnConsoleEventArgs(string s) { Message = s; }
		}

		private class JSValidationWebChromeClient: WebChromeClient {
			public delegate void ConsoleEventHandler(object sender, OnConsoleEventArgs e);

			public event ConsoleEventHandler ConsoleEvent;

			public override void OnCloseWindow (WebView window)
			{
				base.OnCloseWindow (window);
			}

			public override void OnProgressChanged(WebView view, int progress) {
				//progressDialog.Progress = progress;
				base.OnProgressChanged(view,progress);
			}

			public override void OnReceivedTitle (WebView view, string title)
			{
				base.OnReceivedTitle (view, title);
			}

			public override bool OnJsBeforeUnload (WebView view, string url, string message, JsResult result)
			{
				return base.OnJsBeforeUnload (view, url, message, result);
			}

			public override bool OnCreateWindow (WebView view, bool isDialog, bool isUserGesture, Message resultMsg)
			{
				return base.OnCreateWindow (view, isDialog, isUserGesture, resultMsg);
			}

			public override void OnReceivedIcon (WebView view, Bitmap icon)
			{
				base.OnReceivedIcon (view, icon);
			}

			public override bool OnConsoleMessage(ConsoleMessage consoleMessage) {
				String message = String.Format("Line: {0}. Message: {1}. SourceId: {2}",consoleMessage.LineNumber(), consoleMessage.Message(), consoleMessage.SourceId());
				if (ConsoleEvent!=null) {
					ConsoleEvent (this, new OnConsoleEventArgs (message));
				}
				return base.OnConsoleMessage(consoleMessage);
			}
		}

		private void addWebView(Context context) {
			RequestWindowFeature(WindowFeatures.NoTitle);
			LinearLayout mainLayout = new LinearLayout(context);
			mainLayout.SetPadding(0, 0, 0, 0);


			FrameLayout.LayoutParams frame = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,ViewGroup.LayoutParams.MatchParent);
			webView = new WebView(context);
			webView.VerticalScrollBarEnabled=true;
			webView.HorizontalScrollBarEnabled=false;
			webView.Settings.JavaScriptEnabled =true;
			webView.LayoutParameters=frame;
			webView.Settings.AllowUniversalAccessFromFileURLs = true;
			webView.SetWebViewClient(new VisualizationWebViewClient() );
			webView.SetWebChromeClient(new JSValidationWebChromeClient());

			mainLayout.AddView(webView);
			SetContentView(mainLayout,
				new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent));
		}

		class VisualizationWebViewClient : WebViewClient {
			public override void OnReceivedSslError (WebView view, SslErrorHandler handler, SslError error)
			{
				handler.Proceed ();
			}

			public override void OnPageFinished (WebView view, string url)
			{
				base.OnPageFinished (view, url);
			}

			public override void OnReceivedError (WebView view, ClientError errorCode, string description, string failingUrl)
			{
				base.OnReceivedError (view, errorCode, description, failingUrl);
			}
		}
	}
}

