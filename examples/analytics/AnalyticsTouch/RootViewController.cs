using System;
using System.Drawing;

using Foundation;
using UIKit;

using Kidozen;
using Kidozen.iOS;

using Examples;
namespace AnalyticsTouch
{
    public partial class RootViewController : UIViewController
    {
        Model kidoModel = new Model();

        public RootViewController(IntPtr handle)
            : base(handle)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        #region View lifecycle

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
        }

        #endregion

        partial void SignInButton_TouchUpInside(UIButton sender)
        {
            kidoModel.Authenticate().ContinueWith(t =>
                InvokeOnMainThread(() =>
                {
                    TagClickButton.Enabled = t.Result;
                    TagCustomButton.Enabled = t.Result;
                    if (t.Result)
                    {
                        kidoModel.EnableAnalytics();
                    }
                })
            );
        }

        partial void SignOutButton_TouchUpInside(UIButton sender)
        {
        }

        partial void TagClickButton_TouchUpInside(UIButton sender)
        {
            kidoModel.TagButton();
        }

        partial void TagCustomButton_TouchUpInside(UIButton sender)
        {
        }
    }
}