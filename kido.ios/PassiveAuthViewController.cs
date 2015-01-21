using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if __UNIFIED__
using MonoTouch;
using UIKit;
using Foundation;
#else
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace Kidozen.iOS
{
	public class AuthenticationResponseEventArgs : EventArgs {
		public Boolean Success { get; set;}
		public String ErrorMessage { get; set;}
		public Dictionary<string,string> TokenInfo { get; set;}
	}

	public delegate void AuthenticationResponse(object sender, AuthenticationResponseEventArgs e);

	public class PassiveAuthViewController : UIViewController
	{
		UIWebView webview;
		NSUrl signInEndpoint;
        UIActivityIndicatorView activitySpinner;

		public event AuthenticationResponse AuthenticationResponseArrived;

		protected virtual void OnAuthenticationResponseArrived(AuthenticationResponseEventArgs e) 
		{
			if (AuthenticationResponseArrived != null)
				AuthenticationResponseArrived(this, e);
		}
		public PassiveAuthViewController (String endpoint)
		{
			signInEndpoint = new NSUrl(endpoint);
		}

        private void configureActivitySpinner() {
            InvokeOnMainThread(() =>
            {
                this.activitySpinner = new UIActivityIndicatorView()
                {
                    ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
                    AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin |
                        UIViewAutoresizing.FlexibleRightMargin |
                        UIViewAutoresizing.FlexibleTopMargin |
                        UIViewAutoresizing.FlexibleBottomMargin,
                    Tag = kViewTag
                }; 
                this.activitySpinner.Color = UIColor.DarkGray;
                this.activitySpinner.HidesWhenStopped = true;
                this.activitySpinner.Center = this.View.Center;
                this.activitySpinner.StopAnimating();
                this.View.UserInteractionEnabled = true;
                this.View.AddSubview(this.activitySpinner);
            });
        }

        const int kViewTag = 1;


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (x,y) => this.DismissViewController(true,null));
			this.webview = new UIWebView (this.View.Frame);
            
            
			webview.ShouldStartLoad = (webView, request, navType) => {return true;};
			webview.LoadFinished += HandleLoadFinished;
            webview.LoadStarted += HandleLoadStarted;
			this.View.AddSubview (webview);
            configureActivitySpinner();
        }

        private void HandleLoadStarted(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                this.activitySpinner.StartAnimating();
                this.View.UserInteractionEnabled = false;
            });
        }

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			webview.LoadRequest (new NSUrlRequest (this.signInEndpoint));
		}

		void HandleLoadFinished (object sender, EventArgs e)
		{
            InvokeOnMainThread(() =>
            {
                this.activitySpinner.StopAnimating();
                this.View.UserInteractionEnabled = true;
            });
            var payload = webview.EvaluateJavascript ("document.title");
			Console.WriteLine (payload);
			if (payload.Contains ("Success payload=")) {
				payload = Encoding.UTF8.GetString (Convert.FromBase64String (payload.Replace ("Success payload=", String.Empty)));
				var rawToken = JsonConvert.DeserializeObject<Dictionary<string, string>> (payload);
				OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs {
					Success = true,
					TokenInfo = rawToken
				});

				this.InvokeOnMainThread (() => this.DismissViewController (true, null));
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
}

