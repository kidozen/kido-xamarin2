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

        public KidoApplication kido = new KidoApplication("kidodemo.kidocloud.com", "tasks", "wb8KTX2/21A6ISM7PncaNozhxxCxcL8+TtB2aKbZyu8=");

        public override void ViewDidLoad() {
            base.ViewDidLoad();
            kido.EnableCrash();
        }

        partial void authButton_TouchUpInside(UIButton sender){
            kido.Authenticate("demo@kidozen.com", "pass", "Kidozen").Wait();
            Console.WriteLine(kido.IsAuthenticated);
        }

        partial void outOFIndexButton_TouchUpInside(UIButton sender) {
            int[] values = {0,1,2};
            Console.WriteLine(values[5]);
        }
    }
}