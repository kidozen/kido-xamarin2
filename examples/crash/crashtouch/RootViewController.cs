using System;
using System.Drawing;

using Foundation;
using UIKit;
using Kidozen;
using Kidozen.iOS;

namespace crashtouch
{
    public partial class RootViewController : UIViewController
    {
        public RootViewController(IntPtr handle): base(handle){}

        public override void DidReceiveMemoryWarning() {
            base.DidReceiveMemoryWarning();
        }
        
        #region View lifecycle

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

        public KidoApplication kido = new KidoApplication("tenant.kidocloud.com", "tasks", "appkey");

        public override void ViewDidLoad() {
            base.ViewDidLoad();
            kido.EnableCrash();
        }

        partial void authButton_TouchUpInside(UIButton sender){
            kido.AddCrashBreadCrumb("Pre-Authenticate");
            kido.Authenticate("demo@kidozen.com", "pass", "Kidozen").Wait();
            kido.AddCrashBreadCrumb("Authenticated");
            Console.WriteLine(kido.IsAuthenticated);
        }

        partial void outOFIndexButton_TouchUpInside(UIButton sender) {
            int[] values = {0,1,2};
            Console.WriteLine(values[5]);
        }
    }
}