using System;
#if __UNIFIED__
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace Kidozen.iOS
{
	class CustomUrlRequest : NSUrlRequest {
		[Export ("allowsAnyHTTPSCertificateForHost:")]
		static bool Allow (string host) { return true; }

		public CustomUrlRequest(NSUrl url):base(url){}
	}
		
	public class DataVisualizationViewController: UIViewController
	{
		UIWebView webview;
		String localHtmlUrl;

		public DataVisualizationViewController (String fileurl)
		{
			localHtmlUrl = fileurl;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (x,y) => this.DismissViewController(true,null));
			this.webview = new UIWebView (this.View.Frame);
			webview.ShouldStartLoad = (webView, request, navType) => {return true;};
			this.View.AddSubview (webview);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			var request = new NSUrlRequest (new NSUrl (this.localHtmlUrl));

			webview.LoadRequest (request);
		}

	}
}

