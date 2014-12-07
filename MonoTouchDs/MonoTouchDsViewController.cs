using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Kidozen.Examples.Portable;
namespace MonoTouchDs
{
    public partial class MonoTouchDsViewController : UIViewController
    {
        private DataSources myDataSource = new DataSources();
        public MonoTouchDsViewController(IntPtr handle)
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

        partial void btnAuthenticate_TouchUpInside(UIButton sender)
        {
            myDataSource.Authenticate().ContinueWith(
                t => {
                    btnQuery.Enabled = t.Result; 
                }
                );

        }

        partial void btnQuery_TouchUpInside(UIButton sender)
        {
        }
    }
}